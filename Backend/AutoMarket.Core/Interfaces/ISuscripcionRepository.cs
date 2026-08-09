using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ISuscripcionRepository
{
    Task<SuscripcionDealer?> ObtenerPorDealerIdAsync(int perfilDealerId);
    Task AgregarAsync(SuscripcionDealer suscripcion);
    Task ActualizarAsync(SuscripcionDealer suscripcion);
    Task AgregarPagoAsync(PagoSuscripcion pago);
    Task<IReadOnlyList<PagoSuscripcion>> ObtenerHistorialPagosAsync(int perfilDealerId);
    Task<bool> ExistePagoPorEventoAsync(string eventoId);
}