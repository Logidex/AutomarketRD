using AutoMarket.Application.DTOs.Suscripcion;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Interfaces;

public interface ISuscripcionService
{
    Task AsignarPlanInicialAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo);
    Task CambiarPlanAsync(int dealerId, PlanNivel nuevoPlan, CicloFacturacion ciclo);
    Task RenovarManualAsync(int perfilDealerId, DateTime nuevaFechaVencimiento);
    Task ProcesarPagoSuscripcionAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo);
    Task RegistrarPagoAsync(
        int perfilDealerId,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        decimal monto,
        string moneda,
        string? orderIdPayPal,
        string? eventoIdPayPal,
        string? referencia);
    Task<IReadOnlyList<PagoSuscripcionDto>> ObtenerHistorialPagosAsync(int perfilDealerId);
    Task<SuscripcionDealerDto?> ObtenerSuscripcionAsync(int perfilDealerId);
    Task CancelarSuscripcionAsync(int perfilDealerId);
    Task<bool> ExistePagoPorEventoAsync(string eventoId);
    Task<bool> ExistePagoPorOrdenAsync(string orderId);
}