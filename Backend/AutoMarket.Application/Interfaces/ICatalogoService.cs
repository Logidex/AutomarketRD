using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.DTOs.Common;

namespace AutoMarket.Application.Interfaces;

public interface ICatalogoService
{
    Task<PagedResult<AnuncioCatalogoDto>> ObtenerCatalogoPaginadoAsync(int pagina, int tamanoPagina);
}