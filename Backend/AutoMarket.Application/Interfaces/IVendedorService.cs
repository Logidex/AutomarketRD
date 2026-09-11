using AutoMarket.Application.DTOs;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Interfaces;

/// <summary>
/// Servicio para operaciones del vendedor (perfil público, anuncios, estadísticas).
/// </summary>
public interface IVendedorService
{
    /// <summary>
    /// Obtiene la información pública del perfil de un vendedor/dealer.
    /// </summary>
    Task<VendedorPerfilPublicoDto?> ObtenerPerfilPublicoAsync(int vendedorId);

    /// <summary>
    /// Obtiene los anuncios publicados de un vendedor.
    /// </summary>
    Task<IReadOnlyCollection<AnuncioListadoDto>> ObtenerAnunciosDelVendedorAsync(int vendedorId);

    /// <summary>
    /// Obtiene la información de suscripción del vendedor autenticado.
    /// </summary>
    Task<VendedorSuscripcionDto?> ObtenerMiSuscripcionAsync(int usuarioId);
}
