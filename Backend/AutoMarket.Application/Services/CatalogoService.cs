using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Catalogo.
/// </summary>
public class CatalogoService : ICatalogoService
{
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly ICacheService _cache;

    public CatalogoService(IAnuncioRepository anuncioRepository, ICacheService cache)
    {
        _anuncioRepository = anuncioRepository;
        _cache = cache;
    }

    public async Task<PagedResult<AnuncioCatalogoDto>> ObtenerCatalogoPaginadoAsync(int pagina, int tamanoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 50) tamanoPagina = 20;

        // Caché solo para la primera página (la más visitada)
        if (pagina == 1 && tamanoPagina == 20)
        {
            var cached = await _cache.GetAsync<PagedResult<AnuncioCatalogoDto>>("catalogo:pagina:1");
            if (cached is not null) return cached;
        }

        var (anuncios, total) = await _anuncioRepository.ObtenerPaginadosAsync(pagina, tamanoPagina);

        var items = anuncios.Select(a => new AnuncioCatalogoDto
        {
            Id = a.Id,
            Marca = a.Marca,
            Modelo = a.Modelo,
            Anio = a.Anio,
            Precio = a.Precio,
            Moneda = a.Moneda,
            Kilometraje = a.Kilometraje,
            FotoPrincipal = AnuncioFotos.ObtenerPrincipal(a.Fotos),
            PlanNivel = a.Usuario?.PerfilDealer?.Suscripcion?.Nivel.ToString(),
            CreatedAt = a.CreatedAt
        }).ToList();

        var result = new PagedResult<AnuncioCatalogoDto>(items, total, pagina, tamanoPagina);

        if (pagina == 1 && tamanoPagina == 20)
            await _cache.SetAsync("catalogo:pagina:1", result, TimeSpan.FromMinutes(3));

        return result;
    }
}
