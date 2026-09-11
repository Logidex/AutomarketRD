using AutoMarket.Application.DTOs;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para operaciones del vendedor (perfil público, anuncios, suscripción).
/// </summary>
public class VendedorService : IVendedorService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAnuncioRepository _anuncioRepository;

    public VendedorService(
        IUsuarioRepository usuarioRepository,
        IAnuncioRepository anuncioRepository)
    {
        _usuarioRepository = usuarioRepository;
        _anuncioRepository = anuncioRepository;
    }

    public async Task<VendedorPerfilPublicoDto?> ObtenerPerfilPublicoAsync(int vendedorId)
    {
        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(vendedorId);

        if (usuario is null)
            return null;

        var esVendedorParticular = string.Equals(
            usuario.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase);

        var esDealerVerificado = AnuncioPlanValidator.EsDealerVerificado(usuario);

        var (anuncios, _) = await _anuncioRepository.BuscarPaginadoAsync(
            new Core.Entities.AnuncioQueryFilter
            {
                UsuarioId = vendedorId,
                PaginaActual = 1,
                CantidadPorPagina = 1
            });

        return new VendedorPerfilPublicoDto
        {
            UsuarioId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            Telefono = usuario.TelefonoPersonal,
            LogoUrl = usuario.PerfilDealer?.LogoUrl,
            NombreAgencia = usuario.PerfilDealer?.NombreAgencia,
            Direccion = usuario.PerfilDealer?.Ubicacion,
            EsVendedorParticular = esVendedorParticular,
            EsDealerVerificado = esDealerVerificado,
            TotalAnuncios = anuncios.Count()
        };
    }

    public async Task<IReadOnlyCollection<AnuncioListadoDto>> ObtenerAnunciosDelVendedorAsync(int vendedorId)
    {
        var (anuncios, _) = await _anuncioRepository.BuscarPaginadoAsync(
            new Core.Entities.AnuncioQueryFilter
            {
                VendedorId = vendedorId,
                PaginaActual = 1,
                CantidadPorPagina = 50
            });

        return anuncios.Select(a => AnuncioMapper.ToListadoDto(a, true)).ToList();
    }

    public async Task<VendedorSuscripcionDto?> ObtenerMiSuscripcionAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);

        if (usuario?.PerfilDealer?.Suscripcion is null)
            return null;

        var suscripcion = usuario.PerfilDealer.Suscripcion;
        var plan = suscripcion.Plan;

        var nivel = suscripcion.Nivel;
        var limiteAnuncios = plan?.LimiteAnunciosEfectivo ?? PlanConfig.LimiteAnuncios(nivel);
        var cuotaDestacados = plan?.CuotaDestacados ?? 0;
        var maxFotos = plan?.MaxFotosEfectivo ?? PlanConfig.MaxFotos(nivel);
        var diasRestantes = Math.Max(0, (suscripcion.FechaVencimientoUtc - DateTime.UtcNow).Days);

        return new VendedorSuscripcionDto
        {
            Nivel = nivel.ToString(),
            Ciclo = suscripcion.Ciclo.ToString(),
            Estado = suscripcion.Estado.ToString(),
            LimiteAnuncios = limiteAnuncios,
            CuotaDestacados = cuotaDestacados,
            MaxFotos = maxFotos,
            FechaInicioUtc = suscripcion.FechaInicioUtc,
            FechaVencimientoUtc = suscripcion.FechaVencimientoUtc,
            DiasRestantes = diasRestantes,
            Activa = suscripcion.Estado == EstadoSuscripcion.Activa
        };
    }
}
