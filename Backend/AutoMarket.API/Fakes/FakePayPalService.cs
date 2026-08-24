using System.Collections.Concurrent;
using System.Net;

using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.API.Fakes;

/// <summary>
/// Implementación falsa de IPayPalService para desarrollo local y E2E:
/// simula el ciclo orden → aprobación → captura → reembolso sin llamar a
/// PayPal. La "approve URL" apunta directo a la página de éxito con el token
/// de la orden falsa, imitando el regreso de PayPal.
///
/// Se registra SOLO cuando el entorno es Development y PayPal:ClientId es
/// "dummy" (docker-compose.e2e.yml / local sin credenciales). Staging y
/// producción siempre usan el PayPalService real: el gate doble (entorno +
/// credencial dummy) lo hace imposible de activar por accidente.
/// </summary>
public class FakePayPalService : IPayPalService
{
    private readonly string _returnUrl;

    // Órdenes vivas del proceso. Singleton: sobrevive entre requests.
    private readonly ConcurrentDictionary<string, OrdenFake> _ordenes = new();

    private sealed record OrdenFake(
        int PerfilDealerId,
        decimal Monto,
        string Nivel,
        string Ciclo,
        string CaptureId);

    public FakePayPalService(IConfiguration configuration)
    {
        _returnUrl = configuration["PayPal:ReturnUrl"]
            ?? throw new ArgumentNullException("Falta PayPal:ReturnUrl.");
    }

    public Task<string> CrearOrdenDeSuscripcionAsync(
        int dealerId,
        decimal monto,
        string nombrePlan,
        string ciclo)
    {
        var orderId = $"FAKE-{Guid.NewGuid():N}";

        _ordenes[orderId] = new OrdenFake(
            dealerId,
            monto,
            nombrePlan,
            ciclo,
            $"FAKECAP-{Guid.NewGuid():N}");

        return Task.FromResult($"{_returnUrl}?token={orderId}");
    }

    public Task<DetalleOrdenPayPalDto> ObtenerDetalleOrdenAsync(string idOrden)
    {
        if (!_ordenes.TryGetValue(idOrden, out var orden))
        {
            throw new HttpRequestException(
                $"[FakePayPal] Orden no encontrada: {idOrden}",
                null,
                HttpStatusCode.NotFound);
        }

        return Task.FromResult(new DetalleOrdenPayPalDto
        {
            OrderId = idOrden,
            Status = "APPROVED",
            ReferenceId =
                $"DEALER-{orden.PerfilDealerId}-PLAN-{orden.Nivel.ToUpperInvariant()}" +
                $"-CICLO-{orden.Ciclo.ToUpperInvariant()}",
            Monto = orden.Monto,
            Moneda = "USD",
            PerfilDealerId = orden.PerfilDealerId,
            Nivel = Enum.Parse<PlanNivel>(orden.Nivel, ignoreCase: true),
            Ciclo = Enum.Parse<CicloFacturacion>(orden.Ciclo, ignoreCase: true),
        });
    }

    public Task<bool> CapturarOrdenAsync(string idOrden)
    {
        return Task.FromResult(_ordenes.ContainsKey(idOrden));
    }

    public Task<string?> ObtenerCaptureIdDeOrdenAsync(string idOrden)
    {
        return Task.FromResult(
            _ordenes.TryGetValue(idOrden, out var orden)
                ? orden.CaptureId
                : null);
    }

    public Task<bool> ReembolsarAsync(string captureId, decimal monto, string moneda)
    {
        return Task.FromResult(_ordenes.Values.Any(o => o.CaptureId == captureId));
    }

    public Task<bool> VerificarFirmaWebhookAsync(
        string jsonBody,
        string transmissionId,
        string transmissionTime,
        string transmissionSig,
        string certUrl,
        string authAlgo)
    {
        // El webhook falso nunca se acepta: la confirmación va por
        // confirmar-pago, como en desarrollo local sin URL pública.
        return Task.FromResult(false);
    }
}
