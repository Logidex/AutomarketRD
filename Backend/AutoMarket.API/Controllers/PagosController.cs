using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        IPayPalService payPalService,
        ISuscripcionService suscripcionService,
        IUsuarioRepository usuarioRepository,
        ILogger<PagosController> logger)
    {
        _payPalService = payPalService;
        _suscripcionService = suscripcionService;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("generar-link")]
    public async Task<IActionResult> GenerarLinkDePago([FromBody] CrearOrdenDto request)
    {
        try
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                return Unauthorized(new { mensaje = "Token inválido." });
            }

            var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);

            if (dealer is null || dealer.PerfilDealer is null)
            {
                return BadRequest(new { mensaje = "El usuario autenticado no tiene perfil de dealer." });
            }

            var perfilDealerId = dealer.PerfilDealer.UsuarioId;

            var linkPago = await _payPalService.CrearOrdenDeSuscripcionAsync(
                perfilDealerId,
                request.Monto,
                request.NombrePlan,
                request.Ciclo
            );

            _logger.LogInformation(
                "Link de PayPal generado correctamente para DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}",
                perfilDealerId, request.NombrePlan, request.Ciclo);

            return Ok(new { url = linkPago });
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