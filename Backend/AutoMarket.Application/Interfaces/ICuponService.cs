using AutoMarket.Application.DTOs.Cupones;

namespace AutoMarket.Application.Interfaces;

public interface ICuponService
{
    /// <summary>
    /// Canjea un cupón para el dealer autenticado: valida código, tope global
    /// y canje único por dealer; activa/renueva la suscripción al nivel del
    /// cupón por los días configurados.
    /// </summary>
    Task<CuponAplicadoDto> AplicarCuponAsync(int perfilDealerId, string codigo);
}
