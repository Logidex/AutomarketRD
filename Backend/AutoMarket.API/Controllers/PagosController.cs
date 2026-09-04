using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
/// <summary>
/// Controlador para gestionar Pagos.
/// </summary>
public class PagosController : ControllerBase
{
    private readonly IPayPalService _payPalService;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanCatalogoService _planCatalogoService;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PagosController> _logger;

/// <summary>
/// Inicializa una nueva instancia de la clase PagosController.
/// </summary>
    public PagosController(
        IPayPalService payPalService,
        ISuscripcionService suscripcionService,
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoService planCatalogoService,
        IAlmacenadorArchivos almacenadorArchivos,
        IConfiguration configuration,
        ILogger<PagosController> logger)
    {
        _payPalService = payPalService;
        _suscripcionService = suscripcionService;
        _usuarioRepository = usuarioRepository;
        _planCatalogoService = planCatalogoService;
        _almacenadorArchivos = almacenadorArchivos;
        _configuration = configuration;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("generar-link")]
    public async Task<IActionResult> GenerarLinkDePago([FromBody] CrearOrdenDto request)
    {
        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);

            if (dealer is null || dealer.PerfilDealer is null)
            {
                return BadRequest(new { mensaje = "El usuario autenticado no tiene perfil de dealer." });
            }

            var perfilDealerId = dealer.PerfilDealer.UsuarioId;

            if (!Enum.TryParse<PlanNivel>(request.NombrePlan, false, out var planNivel) ||
                !Enum.TryParse<CicloFacturacion>(request.Ciclo, false, out var ciclo))
            {
                return BadRequest(new { mensaje = "El plan o el ciclo seleccionado no es válido." });
            }

            var plan = await _planCatalogoService.ObtenerPlanPorNivelAsync(planNivel);

            if (plan is null)
            {
                return BadRequest(new { mensaje = "El plan seleccionado no está disponible en este momento." });
            }

            var precioRd = ciclo switch
            {
                CicloFacturacion.Mensual => plan.PrecioMensual,
                CicloFacturacion.Trimestral => plan.PrecioTrimestral,
                CicloFacturacion.Anual => plan.PrecioAnual,
                _ => 0m
            };

            if (precioRd <= 0m)
            {
                return BadRequest(new { mensaje = "El plan Gratis no requiere pago." });
            }

            var tasaStr = _configuration["Pago:TasaCambioRD_USD"];
            var tasa = decimal.TryParse(tasaStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var tasaParseada)
                ? tasaParseada
                : 0.017m;

            if (tasa <= 0m)
            {
                tasa = 0.017m;
            }

            // Solo usa el precio del catálogo de planes (RD$), convertido a USD para PayPal.
            var montoUsd = Math.Round(precioRd * tasa, 2);

            if (montoUsd < 0.01m)
            {
                return BadRequest(new { mensaje = "El monto de pago resultante es demasiado pequeño." });
            }

            var linkPago = await _payPalService.CrearOrdenDeSuscripcionAsync(
                perfilDealerId,
                montoUsd,
                request.NombrePlan,
                request.Ciclo
            );

            _logger.LogInformation(
                "Link de PayPal generado correctamente para DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}, MontoUSD {MontoUSD}",
                perfilDealerId, request.NombrePlan, request.Ciclo, montoUsd);

            return Ok(new { url = linkPago, monto = montoUsd, moneda = "USD" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando link de PayPal para el usuario autenticado.");
            return StatusCode(500, new { mensaje = "Ocurrió un error al generar el link de pago." });
        }
    }

    /// <summary>
    /// Confirma y activa la suscripción al regresar de PayPal.
    /// Cubre los casos en que el webhook no llega (p. ej. desarrollo local sin URL pública).
    /// </summary>
    [Authorize]
    [HttpPost("confirmar-pago")]
    public async Task<IActionResult> ConfirmarPago([FromBody] ConfirmarPagoDto dto)
    {
        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            if (string.IsNullOrWhiteSpace(dto.OrderId))
            {
                return BadRequest(new { mensaje = "Falta el identificador de la orden de PayPal." });
            }

            var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);

            if (dealer is null || dealer.PerfilDealer is null)
            {
                return BadRequest(new { mensaje = "El usuario autenticado no tiene perfil de dealer." });
            }

            var perfilDealerId = dealer.PerfilDealer.UsuarioId;

            var detalle = await _payPalService.ObtenerDetalleOrdenAsync(dto.OrderId);

            // La orden debe pertenecer al dealer autenticado
            if (detalle.PerfilDealerId is null || detalle.PerfilDealerId != perfilDealerId)
            {
                return BadRequest(new { mensaje = "La orden de PayPal no corresponde a este usuario." });
            }

            if (detalle.Nivel is null || detalle.Ciclo is null)
            {
                return BadRequest(new { mensaje = "La orden de PayPal no contiene un plan válido." });
            }

            var planNivel = detalle.Nivel.Value;
            var ciclo = detalle.Ciclo.Value;

            // Si la orden ya fue capturada (p. ej. por el webhook) se continúa de forma idempotente
            if (string.Equals(detalle.Status, "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                var capturado = await _payPalService.CapturarOrdenAsync(dto.OrderId);

                if (!capturado)
                {
                    _logger.LogWarning(
                        "No fue posible capturar la orden de PayPal al confirmar. OrderId {OrderId}, DealerId {DealerId}",
                        dto.OrderId, perfilDealerId);

                    return StatusCode(409, new { mensaje = "No fue posible confirmar el pago. Inténtalo nuevamente." });
                }
            }
            else if (!string.Equals(detalle.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Orden de PayPal no aprobada al confirmar. OrderId {OrderId}, Status {Status}",
                    dto.OrderId, detalle.Status);

                return BadRequest(new { mensaje = "La orden de PayPal no está aprobada." });
            }

            // Validación de monto: debe coincidir con el precio del plan en el catálogo
            var plan = await _planCatalogoService.ObtenerPlanPorNivelAsync(planNivel);

            if (plan is null)
            {
                return BadRequest(new { mensaje = "El plan seleccionado no está disponible en este momento." });
            }

            var precioEsperadoRd = ciclo switch
            {
                CicloFacturacion.Mensual => plan.PrecioMensual,
                CicloFacturacion.Trimestral => plan.PrecioTrimestral,
                CicloFacturacion.Anual => plan.PrecioAnual,
                _ => 0m
            };

            var tasaStr = _configuration["Pago:TasaCambioRD_USD"];
            var tasa = decimal.TryParse(tasaStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var tasaParseada)
                ? tasaParseada
                : 0.017m;

            if (tasa <= 0m)
                tasa = 0.017m;

            var montoEsperadoUsd = Math.Round(precioEsperadoRd * tasa, 2);

            if (precioEsperadoRd <= 0m || Math.Abs(detalle.Monto - montoEsperadoUsd) > 0.01m)
            {
                _logger.LogWarning(
                    "Confirmación con monto que no coincide con el plan. OrderId {OrderId}, DealerId {DealerId}, Esperado {MontoEsperadoUsd}, Recibido {Monto}",
                    dto.OrderId, perfilDealerId, montoEsperadoUsd, detalle.Monto);

                return BadRequest(new { mensaje = "El monto del pago no coincide con el plan seleccionado." });
            }

            // Idempotencia: si esta orden ya fue procesada (webhook o reintento), se ignora
            if (await _suscripcionService.ExistePagoPorOrdenAsync(dto.OrderId))
            {
                _logger.LogInformation(
                    "Confirmación de pago duplicada ignorada. OrderId {OrderId}",
                    dto.OrderId);

                return Ok(new { exito = true, yaProcesado = true });
            }

            await _suscripcionService.ProcesarPagoSuscripcionAsync(perfilDealerId, planNivel, ciclo);

            string? captureIdPayPal = null;
            try
            {
                captureIdPayPal = await _payPalService.ObtenerCaptureIdDeOrdenAsync(dto.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo obtener el CaptureId de la orden al confirmar. OrderId {OrderId}",
                    dto.OrderId);
            }

            try
            {
                await _suscripcionService.RegistrarPagoAsync(
                    perfilDealerId,
                    planNivel,
                    ciclo,
                    detalle.Monto,
                    detalle.Moneda,
                    dto.OrderId,
                    null,
                    captureIdPayPal,
                    detalle.ReferenceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "No se pudo registrar el historial del pago al confirmar. OrderId {OrderId}, DealerId {DealerId}",
                    dto.OrderId, perfilDealerId);
            }

            _logger.LogInformation(
                "Pago PayPal confirmado y suscripción activada. OrderId {OrderId}, DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}",
                dto.OrderId, perfilDealerId, planNivel, ciclo);

            return Ok(new { exito = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirmando el pago de PayPal para el usuario autenticado.");
            return StatusCode(500, new { mensaje = "Ocurrió un error al confirmar el pago." });
        }
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> PayPalWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var jsonBody = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                _logger.LogWarning("Webhook PayPal rechazado porque el body llegó vacío.");
                return Unauthorized();
            }

            var transmissionId = Request.Headers["PAYPAL-TRANSMISSION-ID"].ToString();
            var transmissionTime = Request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString();
            var transmissionSig = Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString();
            var certUrl = Request.Headers["PAYPAL-CERT-URL"].ToString();
            var authAlgo = Request.Headers["PAYPAL-AUTH-ALGO"].ToString();

            if (string.IsNullOrWhiteSpace(transmissionId) ||
                string.IsNullOrWhiteSpace(transmissionTime) ||
                string.IsNullOrWhiteSpace(transmissionSig) ||
                string.IsNullOrWhiteSpace(certUrl) ||
                string.IsNullOrWhiteSpace(authAlgo))
            {
                _logger.LogWarning("Webhook PayPal rechazado por headers incompletos.");
                return Unauthorized();
            }

            var firmaValida = await _payPalService.VerificarFirmaWebhookAsync(
                jsonBody,
                transmissionId,
                transmissionTime,
                transmissionSig,
                certUrl,
                authAlgo);

            if (!firmaValida)
            {
                _logger.LogWarning(
                    "Webhook PayPal rechazado por firma inválida. TransmissionId {TransmissionId}",
                    transmissionId);

                return Unauthorized();
            }

            using var document = JsonDocument.Parse(jsonBody);
            var root = document.RootElement;

            var eventId = root.TryGetProperty("id", out var eventIdProp)
                ? eventIdProp.GetString()
                : null;

            var eventType = root.TryGetProperty("event_type", out var eventTypeProp)
                ? eventTypeProp.GetString()
                : null;

            _logger.LogInformation(
                "Webhook PayPal validado correctamente. EventId {EventId}, EventType {EventType}",
                eventId, eventType);

            if (!string.Equals(eventType, "CHECKOUT.ORDER.APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Evento PayPal ignorado. EventId {EventId}, EventType {EventType}",
                    eventId, eventType);

                return Ok();
            }

            // Sin id de evento no es posible aplicar idempotencia: se descarta
            // para no arriesgar un cobro duplicado.
            if (string.IsNullOrWhiteSpace(eventId))
            {
                _logger.LogWarning(
                    "Webhook PayPal sin EventId; evento descartado para evitar cobros duplicados.");

                return Ok();
            }

            if (!root.TryGetProperty("resource", out var resource))
            {
                _logger.LogWarning("Webhook PayPal sin resource. EventId {EventId}", eventId);
                return Ok();
            }

            var orderId = resource.TryGetProperty("id", out var orderIdProp)
                ? orderIdProp.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("Webhook PayPal sin OrderId. EventId {EventId}", eventId);
                return Ok();
            }

            if (!resource.TryGetProperty("purchase_units", out var purchaseUnits) ||
                purchaseUnits.ValueKind != JsonValueKind.Array ||
                purchaseUnits.GetArrayLength() == 0)
            {
                _logger.LogWarning(
                    "Webhook PayPal sin purchase_units válidos. EventId {EventId}, OrderId {OrderId}",
                    eventId, orderId);

                return Ok();
            }

            var firstPurchaseUnit = purchaseUnits[0];

            var monto = 0m;
            var moneda = "USD";

            if (firstPurchaseUnit.TryGetProperty("amount", out var amountEl))
            {
                if (amountEl.TryGetProperty("value", out var valorEl))
                {
                    decimal.TryParse(valorEl.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out monto);
                }

                if (amountEl.TryGetProperty("currency_code", out var monedaEl))
                {
                    moneda = monedaEl.GetString() ?? "USD";
                }
            }

            var referenceId = firstPurchaseUnit.TryGetProperty("reference_id", out var referenceIdProp)
                ? referenceIdProp.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(referenceId))
            {
                _logger.LogWarning(
                    "Webhook PayPal sin ReferenceId. EventId {EventId}, OrderId {OrderId}",
                    eventId, orderId);

                return Ok();
            }

            var partes = referenceId.Split('-');

            if (partes.Length != 6 ||
                !string.Equals(partes[0], "DEALER", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(partes[2], "PLAN", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(partes[4], "CICLO", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(partes[1], out var dealerId))
            {
                _logger.LogWarning(
                    "ReferenceId inválido recibido desde PayPal. EventId {EventId}, OrderId {OrderId}, ReferenceId {ReferenceId}",
                    eventId, orderId, referenceId);

                return Ok();
            }

            var nombrePlanString = partes[3];
            var cicloString = partes[5];

            if (!Enum.TryParse<PlanNivel>(nombrePlanString, true, out var planEnum) ||
                !Enum.TryParse<CicloFacturacion>(cicloString, true, out var cicloEnum))
            {
                _logger.LogWarning(
                    "Plan o ciclo inválido recibido desde PayPal. EventId {EventId}, OrderId {OrderId}, ReferenceId {ReferenceId}",
                    eventId, orderId, referenceId);

                return Ok();
            }

            // El dealer debe existir para poder aplicar el pago
            var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(dealerId);

            if (dealer is null || dealer.PerfilDealer is null)
            {
                _logger.LogWarning(
                    "Webhook PayPal con dealer inexistente. EventId {EventId}, OrderId {OrderId}, DealerId {DealerId}",
                    eventId, orderId, dealerId);

                return Ok();
            }

            // Idempotencia: si este evento ya fue procesado, se ignora (PayPal puede reenviarlo)
            if (await _suscripcionService.ExistePagoPorEventoAsync(eventId ?? string.Empty))
            {
                _logger.LogInformation(
                    "Webhook PayPal duplicado ignorado. EventId {EventId}, OrderId {OrderId}",
                    eventId, orderId);

                return Ok();
            }

            // Validación de monto: el pago debe coincidir con el precio del plan en el catálogo
            var planWebhook = await _planCatalogoService.ObtenerPlanPorNivelAsync(planEnum);

            if (planWebhook is null)
            {
                _logger.LogWarning(
                    "Webhook PayPal para plan no disponible. EventId {EventId}, OrderId {OrderId}, Plan {Plan}",
                    eventId, orderId, planEnum);

                return Ok();
            }

            var precioEsperadoRd = cicloEnum switch
            {
                CicloFacturacion.Mensual => planWebhook.PrecioMensual,
                CicloFacturacion.Trimestral => planWebhook.PrecioTrimestral,
                CicloFacturacion.Anual => planWebhook.PrecioAnual,
                _ => 0m
            };

            var tasaStr = _configuration["Pago:TasaCambioRD_USD"];
            var tasa = decimal.TryParse(tasaStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var tasaParseada)
                ? tasaParseada
                : 0.017m;

            if (tasa <= 0m)
                tasa = 0.017m;

            var montoEsperadoUsd = Math.Round(precioEsperadoRd * tasa, 2);

            if (precioEsperadoRd <= 0m || Math.Abs(monto - montoEsperadoUsd) > 0.01m)
            {
                _logger.LogWarning(
                    "Webhook PayPal con monto que no coincide con el plan. EventId {EventId}, OrderId {OrderId}, DealerId {DealerId}, Esperado {MontoEsperadoUsd}, Recibido {Monto}",
                    eventId, orderId, dealerId, montoEsperadoUsd, monto);

                return Ok();
            }

            var cobroExitoso = await _payPalService.CapturarOrdenAsync(orderId);

            _logger.LogInformation(
                "Resultado de captura PayPal. EventId {EventId}, OrderId {OrderId}, DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}, CobroExitoso {CobroExitoso}",
                eventId, orderId, dealerId, planEnum, cicloEnum, cobroExitoso);

            if (!cobroExitoso)
            {
                _logger.LogWarning(
                    "No fue posible capturar la orden de PayPal. EventId {EventId}, OrderId {OrderId}",
                    eventId, orderId);

                return Ok();
            }

            await _suscripcionService.ProcesarPagoSuscripcionAsync(dealerId, planEnum, cicloEnum);

            string? captureIdPayPal = null;
            try
            {
                captureIdPayPal = await _payPalService.ObtenerCaptureIdDeOrdenAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo obtener el CaptureId de la orden en el webhook. OrderId {OrderId}",
                    orderId);
            }

            try
            {
                await _suscripcionService.RegistrarPagoAsync(
                    dealerId,
                    planEnum,
                    cicloEnum,
                    monto,
                    moneda,
                    orderId,
                    eventId,
                    captureIdPayPal,
                    referenceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "No se pudo registrar el historial del pago. OrderId {OrderId}, DealerId {DealerId}",
                    orderId, dealerId);
            }

            _logger.LogInformation(
                "Pago PayPal procesado correctamente. EventId {EventId}, OrderId {OrderId}, DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}",
                eventId, orderId, dealerId, planEnum, cicloEnum);

            return Ok();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parseando JSON del webhook de PayPal.");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook de PayPal.");
            return Ok();
        }
    }

    [Authorize]
    [HttpPost("transferencia")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegistrarTransferencia(
        [FromForm] string nombrePlan,
        [FromForm] string ciclo,
        [FromForm] IFormFile imagen)
    {
        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            if (imagen == null || imagen.Length == 0)
            {
                return BadRequest(new { mensaje = "Debes adjuntar la captura de la transferencia." });
            }

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new { mensaje = "Solo se permiten archivos JPG o PNG." });
            }

            if (imagen.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { mensaje = "La imagen no debe exceder 5MB." });
            }

            var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);

            if (dealer is null || dealer.PerfilDealer is null)
            {
                return BadRequest(new { mensaje = "El usuario autenticado no tiene perfil de dealer." });
            }

            var perfilDealerId = dealer.PerfilDealer.UsuarioId;

            if (!Enum.TryParse<PlanNivel>(nombrePlan, false, out var planNivel) ||
                !Enum.TryParse<CicloFacturacion>(ciclo, false, out var cicloFacturacion))
            {
                return BadRequest(new { mensaje = "El plan o el ciclo seleccionado no es válido." });
            }

            var plan = await _planCatalogoService.ObtenerPlanPorNivelAsync(planNivel);

            if (plan is null)
            {
                return BadRequest(new { mensaje = "El plan seleccionado no está disponible en este momento." });
            }

            var precioRd = cicloFacturacion switch
            {
                CicloFacturacion.Mensual => plan.PrecioMensual,
                CicloFacturacion.Trimestral => plan.PrecioTrimestral,
                CicloFacturacion.Anual => plan.PrecioAnual,
                _ => 0m
            };

            if (precioRd <= 0m)
            {
                return BadRequest(new { mensaje = "El plan Gratis no requiere pago." });
            }

            using var stream = imagen.OpenReadStream();
            var nombreArchivo = $"transferencias/{perfilDealerId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var urlCaptura = await _almacenadorArchivos.GuardarArchivoAsync(stream, nombreArchivo, imagen.ContentType);

            var pagoId = await _suscripcionService.RegistrarPagoTransferenciaAsync(
                perfilDealerId,
                planNivel,
                cicloFacturacion,
                precioRd,
                "RD$",
                urlCaptura);

            _logger.LogInformation(
                "Transferencia registrada. PagoId {PagoId}, DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}",
                pagoId, perfilDealerId, nombrePlan, ciclo);

            return Ok(new
            {
                exito = true,
                pagoId,
                mensaje = "Tu comprobante fue recibido. Tu suscripción se activará por 1 día mientras el admin confirma el pago."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando transferencia para el usuario autenticado.");
            return StatusCode(500, new { mensaje = "Ocurrió un error al registrar la transferencia." });
        }
    }
}
