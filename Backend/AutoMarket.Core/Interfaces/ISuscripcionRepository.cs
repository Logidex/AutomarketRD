using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ISuscripcionRepository
{
    Task<SuscripcionDealer?> ObtenerPorDealerIdAsync(int perfilDealerId);
    Task AgregarAsync(SuscripcionDealer suscripcion);
    Task ActualizarAsync(SuscripcionDealer suscripcion);
    Task AgregarPagoAsync(PagoSuscripcion pago);
    Task<IReadOnlyList<PagoSuscripcion>> ObtenerHistorialPagosAsync(int perfilDealerId);
    Task<IReadOnlyList<PagoSuscripcion>> ObtenerTodosLosPagosAsync();
    Task<PagoSuscripcion?> ObtenerPagoPorIdAsync(int pagoId);
    Task ActualizarPagoAsync(PagoSuscripcion pago);
    Task<bool> ExistePagoPorEventoAsync(string eventoId);
    Task<bool> ExistePagoPorOrdenAsync(string orderId);
}