using System.Text.Json;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

public class PagoOrquestacionService : IPagoOrquestacionService
{
    private readonly IPayPalService _payPalService;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanCatalogoService _planCatalogoService;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PagoOrquestacionService> _logger;

    public PagoOrquestacionService(
        IPayPalService payPalService,
        ISuscripcionService suscripcionService,
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoService planCatalogoService,
        IAlmacenadorArchivos almacenadorArchivos,
        IConfiguration configuration,
        ILogger<PagoOrquestacionService> logger)
    {
        _payPalService = payPalService;
        _suscripcionService = suscripcionService;
        _usuarioRepository = usuarioRepository;
        _planCatalogoService = planCatalogoService;
        _almacenadorArchivos = almacenadorArchivos;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(string Url, decimal MontoUsd)> GenerarLinkDePagoAsync(int usuarioId, CrearOrdenDto request)
    {
        var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId)
            ?? throw new InvalidOperationException("El usuario autenticado no tiene perfil de dealer.");

        var perfilDealerId = dealer.PerfilDealer!.UsuarioId;

        if (!Enum.TryParse<PlanNivel>(request.NombrePlan, false, out var planNivel) ||
            !Enum.TryParse<CicloFacturacion>(request.Ciclo, false, out var ciclo))
            throw new InvalidOperationException("El plan o el ciclo seleccionado no es válido.");

        var plan = await _planCatalogoService.ObtenerPlanPorNivelAsync(planNivel)
            ?? throw new InvalidOperationException("El plan seleccionado no está disponible en este momento.");

        var precioRd = PagoHelper.ObtenerPrecioEnRD(plan, ciclo);
        if (precioRd <= 0m)
            throw new InvalidOperationException("El plan Gratis no requiere pago.");

        var tasa = PagoHelper.ObtenerTasaCambio(_configuration);
        var montoUsd = PagoHelper.ConvertirAUSD(precioRd, tasa);
        if (montoUsd < 0.01m)
            throw new InvalidOperationException("El monto de pago resultante es demasiado pequeño.");

        var linkPago = await _payPalService.CrearOrdenDeSuscripcionAsync(
            perfilDealerId, montoUsd, request.NombrePlan, request.Ciclo);

        _logger.LogInformation(
            "Link de PayPal generado. DealerId {DealerId}, Plan {Plan}, Ciclo {Ciclo}, MontoUSD {MontoUSD}",
            perfilDealerId, request.NombrePlan, request.Ciclo, montoUsd);

        return (linkPago, montoUsd);
    }

    public async Task<ConfirmarPagoResult> ConfirmarPagoAsync(int usuarioId, ConfirmarPagoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OrderId))
            return new ConfirmarPagoResult { Mensaje = "Falta el identificador de la orden de PayPal." };

        var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId)
            ?? throw new InvalidOperationException("El usuario autenticado no tiene perfil de dealer.");

        var perfilDealerId = dealer.PerfilDealer!.UsuarioId;
        var detalle = await _payPalService.ObtenerDetalleOrdenAsync(dto.OrderId);

        if (detalle.PerfilDealerId is null || detalle.PerfilDealerId != perfilDealerId)
            return new ConfirmarPagoResult { Mensaje = "La orden de PayPal no corresponde a este usuario." };

        if (detalle.Nivel is null || detalle.Ciclo is null)
            return new ConfirmarPagoResult { Mensaje = "La orden de PayPal no contiene un plan válido." };

        var planNivel = detalle.Nivel.Value;
        var ciclo = detalle.Ciclo.Value;

        if (string.Equals(detalle.Status, "APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            var capturado = await _payPalService.CapturarOrdenAsync(dto.OrderId);
            if (!capturado)
            {
                _logger.LogWarning(
                    "No fue posible capturar la orden al confirmar. OrderId {OrderId}, DealerId {DealerId}",
                    dto.OrderId, perfilDealerId);
                return new ConfirmarPagoResult { Mensaje = "No fue posible confirmar el pago. Inténtalo nuevamente." };
            }
        }
        else if (!string.Equals(detalle.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Orden no aprobada al confirmar. OrderId {OrderId}, Status {Status}",
                dto.OrderId, detalle.Status);
            return new ConfirmarPagoResult { Mensaje = "La orden de PayPal no está aprobada." };
        }

        var plan = await _planCatalogoService.ObtenerPlanPorNivelAsync(planNivel)
            ?? throw new InvalidOperationException("El plan seleccionado no está disponible en este momento.");

        var precioEsperadoRd = PagoHelper.ObtenerPrecioEnRD(plan, ciclo);
        var tasa = PagoHelper.ObtenerTasaCambio(_configuration);
        var montoEsperadoUsd = PagoHelper.ConvertirAUSD(precioEsperadoRd, tasa);

        if (precioEsperadoRd <= 0m || Math.Abs(detalle.Monto - montoEsperadoUsd) > 0.01m)
        {
            _logger.LogWarning(
                "Confirmación con monto que no coincide. OrderId {OrderId}, Esperado {Esperado}, Recibido {Recibido}",
                dto.OrderId, montoEsperadoUsd, detalle.Monto);
            return new ConfirmarPagoResult { Mensaje = "El monto del pago no coincide con el plan seleccionado." };
        }

        if (await _suscripcionService.ExistePagoPorOrdenAsync(dto.OrderId))
        {
            _logger.LogInformation("Confirmación duplicada ignorada. OrderId {OrderId}", dto.OrderId);
            return new ConfirmarPagoResult { Exito = true, YaProcesado = true };
        }

        await _suscripcionService.ProcesarPagoSuscripcionAsync(perfilDealerId, planNivel, ciclo);

        string? captureIdPayPal = null;
        try { captureIdPayPal = await _payPalService.ObtenerCaptureIdDeOrdenAsync(dto.OrderId); }
        catch (Exception ex) { _logger.LogWarning(ex, "No se pudo obtener CaptureId. OrderId {OrderId}", dto.OrderId); }

        try
        {
            await _suscripcionService.RegistrarPagoAsync(
                perfilDealerId, planNivel, ciclo, detalle.Monto, detalle.Moneda,
                dto.OrderId, null, captureIdPayPal, detalle.ReferenceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el pago. OrderId {OrderId}", dto.OrderId);
        }

        _logger.LogInformation(
            "Pago confirmado. OrderId {OrderId}, DealerId {DealerId}, Plan {Plan}",
            dto.OrderId, perfilDealerId, planNivel);

        return new ConfirmarPagoResult { Exito = true };
    }

    public async Task<WebhookResult> ProcesarWebhookAsync(string jsonBody, Dictionary<string, string> headers)
    {
        if (string.IsNullOrWhiteSpace(jsonBody))
        {
            _logger.LogWarning("Webhook PayPal rechazado: body vacío.");
            return new WebhookResult { StatusCode = 401, Mensaje = "Unauthorized" };
        }

        if (!headers.TryGetValue("PAYPAL-TRANSMISSION-ID", out var transmissionId) ||
            !headers.TryGetValue("PAYPAL-TRANSMISSION-TIME", out var transmissionTime) ||
            !headers.TryGetValue("PAYPAL-TRANSMISSION-SIG", out var transmissionSig) ||
            !headers.TryGetValue("PAYPAL-CERT-URL", out var certUrl) ||
            !headers.TryGetValue("PAYPAL-AUTH-ALGO", out var authAlgo) ||
            string.IsNullOrWhiteSpace(transmissionId) ||
            string.IsNullOrWhiteSpace(transmissionTime) ||
            string.IsNullOrWhiteSpace(transmissionSig) ||
            string.IsNullOrWhiteSpace(certUrl) ||
            string.IsNullOrWhiteSpace(authAlgo))
        {
            _logger.LogWarning("Webhook PayPal rechazado: headers incompletos.");
            return new WebhookResult { StatusCode = 401, Mensaje = "Unauthorized" };
        }

        var firmaValida = await _payPalService.VerificarFirmaWebhookAsync(
            jsonBody, transmissionId, transmissionTime, transmissionSig, certUrl, authAlgo);

        if (!firmaValida)
        {
            _logger.LogWarning("Webhook PayPal rechazado: firma inválida. TransmissionId {TransmissionId}", transmissionId);
            return new WebhookResult { StatusCode = 401, Mensaje = "Unauthorized" };
        }

        using var document = JsonDocument.Parse(jsonBody);
        var root = document.RootElement;

        var eventId = root.TryGetProperty("id", out var eventIdProp) ? eventIdProp.GetString() : null;
        var eventType = root.TryGetProperty("event_type", out var eventTypeProp) ? eventTypeProp.GetString() : null;

        _logger.LogInformation("Webhook PayPal validado. EventId {EventId}, EventType {EventType}", eventId, eventType);

        if (!string.Equals(eventType, "CHECKOUT.ORDER.APPROVED", StringComparison.OrdinalIgnoreCase))
            return new WebhookResult { StatusCode = 200 };

        if (string.IsNullOrWhiteSpace(eventId))
        {
            _logger.LogWarning("Webhook PayPal sin EventId; descartado para evitar duplicados.");
            return new WebhookResult { StatusCode = 200 };
        }

        if (!root.TryGetProperty("resource", out var resource))
            return new WebhookResult { StatusCode = 200 };

        var orderId = resource.TryGetProperty("id", out var orderIdProp) ? orderIdProp.GetString() : null;
        if (string.IsNullOrWhiteSpace(orderId))
            return new WebhookResult { StatusCode = 200 };

        if (!resource.TryGetProperty("purchase_units", out var purchaseUnits) ||
            purchaseUnits.ValueKind != JsonValueKind.Array ||
            purchaseUnits.GetArrayLength() == 0)
            return new WebhookResult { StatusCode = 200 };

        var firstPurchaseUnit = purchaseUnits[0];
        var monto = 0m;
        var moneda = "USD";

        if (firstPurchaseUnit.TryGetProperty("amount", out var amountEl))
        {
            if (amountEl.TryGetProperty("value", out var valorEl))
                decimal.TryParse(valorEl.GetString(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out monto);
            if (amountEl.TryGetProperty("currency_code", out var monedaEl))
                moneda = monedaEl.GetString() ?? "USD";
        }

        var referenceId = firstPurchaseUnit.TryGetProperty("reference_id", out var referenceIdProp)
            ? referenceIdProp.GetString() : null;

        if (string.IsNullOrWhiteSpace(referenceId))
            return new WebhookResult { StatusCode = 200 };

        var partes = referenceId.Split('-');
        if (partes.Length != 6 ||
            !string.Equals(partes[0], "DEALER", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(partes[2], "PLAN", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(partes[4], "CICLO", StringComparison.OrdinalIgnoreCase) ||
            !int.TryParse(partes[1], out var dealerId))
            return new WebhookResult { StatusCode = 200 };

        if (!Enum.TryParse<PlanNivel>(partes[3], true, out var planEnum) ||
            !Enum.TryParse<CicloFacturacion>(partes[5], true, out var cicloEnum))
            return new WebhookResult { StatusCode = 200 };

        var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(dealerId);
        if (dealer is null || dealer.PerfilDealer is null)
            return new WebhookResult { StatusCode = 200 };

        if (await _suscripcionService.ExistePagoPorEventoAsync(eventId ?? string.Empty))
            return new WebhookResult { StatusCode = 200 };

        var planWebhook = await _planCatalogoService.ObtenerPlanPorNivelAsync(planEnum);
        if (planWebhook is null)
            return new WebhookResult { StatusCode = 200 };

        var precioEsperadoRd = PagoHelper.ObtenerPrecioEnRD(planWebhook, cicloEnum);
        var tasa = PagoHelper.ObtenerTasaCambio(_configuration);
        var montoEsperadoUsd = PagoHelper.ConvertirAUSD(precioEsperadoRd, tasa);

        if (precioEsperadoRd <= 0m || Math.Abs(monto - montoEsperadoUsd) > 0.01m)
        {
            _logger.LogWarning(
                "Webhook con monto que no coincide. EventId {EventId}, OrderId {OrderId}, Esperado {Esperado}, Recibido {Recibido}",
                eventId, orderId, montoEsperadoUsd, monto);
            return new WebhookResult { StatusCode = 200 };
        }

        var cobroExitoso = await _payPalService.CapturarOrdenAsync(orderId);
        if (!cobroExitoso)
            return new WebhookResult { StatusCode = 200 };

        await _suscripcionService.ProcesarPagoSuscripcionAsync(dealerId, planEnum, cicloEnum);

        string? captureIdPayPal = null;
        try { captureIdPayPal = await _payPalService.ObtenerCaptureIdDeOrdenAsync(orderId); }
        catch (Exception ex) { _logger.LogWarning(ex, "No se pudo obtener CaptureId. OrderId {OrderId}", orderId); }

        try
        {
            await _suscripcionService.RegistrarPagoAsync(
                dealerId, planEnum, cicloEnum, monto, moneda,
                orderId, eventId, captureIdPayPal, referenceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el pago. OrderId {OrderId}", orderId);
        }

        _logger.LogInformation(
            "Pago PayPal procesado. EventId {EventId}, OrderId {OrderId}, DealerId {DealerId}",
            eventId, orderId, dealerId);

        return new WebhookResult { StatusCode = 200 };
    }

    public async Task<TransferenciaResult> RegistrarTransferenciaAsync(int usuarioId, string nombrePlan, string ciclo, IFormFile imagen)
    {
        if (imagen is null || imagen.Length == 0)
            return new TransferenciaResult { Mensaje = "Debes adjuntar la captura de la transferencia." };

        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
            return new TransferenciaResult { Mensaje = "Solo se permiten archivos JPG o PNG." };

        if (imagen.Length > 5 * 1024 * 1024)
            return new TransferenciaResult { Mensaje = "La imagen no debe exceder 5MB." };

        var dealer = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId)
            ?? throw new InvalidOperationException("El usuario autenticado no tiene perfil de dealer.");

        var perfilDealerId = dealer.PerfilDealer!.UsuarioId;

        if (!Enum.TryParse<PlanNivel>(nombrePlan, false, out var planNivel) ||
            !Enum.TryParse<CicloFacturacion>(ciclo, false, out var cicloFacturacion))
            throw new InvalidOperationException("El plan o el ciclo seleccionado no es válido.");

        var plan = await _planCatalogoService.ObtenerPlanPorNivelAsync(planNivel)
            ?? throw new InvalidOperationException("El plan seleccionado no está disponible en este momento.");

        var precioRd = PagoHelper.ObtenerPrecioEnRD(plan, cicloFacturacion);
        if (precioRd <= 0m)
            throw new InvalidOperationException("El plan Gratis no requiere pago.");

        using var stream = imagen.OpenReadStream();
        var nombreArchivo = $"transferencias/{perfilDealerId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var urlCaptura = await _almacenadorArchivos.GuardarArchivoAsync(stream, nombreArchivo, imagen.ContentType);

        var pagoId = await _suscripcionService.RegistrarPagoTransferenciaAsync(
            perfilDealerId, planNivel, cicloFacturacion, precioRd, "RD$", urlCaptura);

        _logger.LogInformation(
            "Transferencia registrada. PagoId {PagoId}, DealerId {DealerId}, Plan {Plan}",
            pagoId, perfilDealerId, nombrePlan);

        return new TransferenciaResult
        {
            Exito = true,
            PagoId = pagoId,
            Mensaje = "Tu comprobante fue recibido. Tu suscripción se activará por 1 día mientras el admin confirma el pago."
        };
    }
}
