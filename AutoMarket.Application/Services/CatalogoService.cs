using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.DTOs.Common;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly IAnuncioRepository _anuncioRepository;

    public CatalogoService(IAnuncioRepository anuncioRepository)
    {
        _anuncioRepository = anuncioRepository;
    }

    public async Task<PagedResult<AnuncioCatalogoDto>> ObtenerCatalogoPaginadoAsync(int pagina, int tamanoPagina)
    {
        // 1. Reglas de seguridad para la paginación
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 50) tamanoPagina = 20; // Máximo 50 vehículos por petición

        // 2. Pedimos los datos al repositorio
        var (anuncios, total) = await _anuncioRepository.ObtenerPaginadosAsync(pagina, tamanoPagina);

        // 3. Mapeamos la respuesta
        var items = anuncios.Select(a => new AnuncioCatalogoDto
        {
            Id = a.Id,
            Marca = a.Marca,
            Modelo = a.Modelo,
            Anio = a.Anio,
            Precio = a.Precio,
            Kilometraje = a.Kilometraje,
            FotoPrincipal = a.Fotos != null && a.Fotos.Any() ? a.Fotos.First() : null
        }).ToList();

        // 4. Empaquetamos todo en nuestra caja maestra
        return new PagedResult<AnuncioCatalogoDto>(items, total, pagina, tamanoPagina);
    }
}