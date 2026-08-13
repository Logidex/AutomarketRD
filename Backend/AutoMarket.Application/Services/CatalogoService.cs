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

/// <summary>
/// Inicializa una nueva instancia de la clase CatalogoService. Parámetro anuncioRepository (IAnuncioRepository)
/// </summary>
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
            Moneda = a.Moneda,
            Kilometraje = a.Kilometraje,
            FotoPrincipal = AnuncioFotos.ObtenerPrincipal(a.Fotos)
        }).ToList();

        // 4. Empaquetamos todo en nuestra caja maestra
        return new PagedResult<AnuncioCatalogoDto>(items, total, pagina, tamanoPagina);
    }
}
