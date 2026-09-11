using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.Features.Catalogo.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Catalogo.Handlers;

public class CatalogoQueryHandler
    : IRequestHandler<ObtenerCatalogoPaginadoQuery, PagedResult<AnuncioCatalogoDto>>
{
    private readonly ICatalogoService _service;
    public CatalogoQueryHandler(ICatalogoService service) => _service = service;

    public async Task<PagedResult<AnuncioCatalogoDto>> Handle(ObtenerCatalogoPaginadoQuery request, CancellationToken ct)
        => await _service.ObtenerCatalogoPaginadoAsync(request.Pagina, request.TamanoPagina);
}
