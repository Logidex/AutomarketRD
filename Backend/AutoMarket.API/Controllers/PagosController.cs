using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
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
public class PagosController : ControllerBase
{
    private readonly IPayPalService _payPalService;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanCatalogoService _planCatalogoService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        IPayPalService payPalService,
        ISuscripcionService suscripcionService,
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoService planCatalogoService,
        IConfiguration configuration,
        ILogger<PagosController> logger)
    {
        _payPalService = payPalService;
        _suscripcionService = suscripcionService;
        _usuarioRepository = usuarioRepository;
        _planCatalogoService = planCatalogoService;
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
}