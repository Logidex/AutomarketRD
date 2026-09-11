using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Anuncios.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Anuncios.Handlers;

public class AnuncioQueryHandler :
    IRequestHandler<ObtenerAnuncioPorIdQuery, AnuncioDto?>,
    IRequestHandler<ObtenerTodosLosAnunciosQuery, IReadOnlyCollection<AnuncioListadoDto>>,
    IRequestHandler<BuscarAnunciosQuery, PagedResult<AnuncioListadoDto>>,
    IRequestHandler<ObtenerDestacadosQuery, PagedResult<AnuncioListadoDto>>
{
    private readonly IAnuncioService _service;

    public AnuncioQueryHandler(IAnuncioService service)
    {
        _service = service;
    }

    public async Task<AnuncioDto?> Handle(ObtenerAnuncioPorIdQuery request, CancellationToken ct)
        => await _service.ObtenerAnuncioPorIdAsync(request.Id, request.UsuarioId);

    public async Task<IReadOnlyCollection<AnuncioListadoDto>> Handle(ObtenerTodosLosAnunciosQuery request, CancellationToken ct)
        => await _service.ObtenerTodosLosAnuncios();

    public async Task<PagedResult<AnuncioListadoDto>> Handle(BuscarAnunciosQuery request, CancellationToken ct)
        => await _service.BuscarAnunciosAsync(request.Dto);

    public async Task<PagedResult<AnuncioListadoDto>> Handle(ObtenerDestacadosQuery request, CancellationToken ct)
        => await _service.ObtenerDestacadosAsync(request.Pagina, request.TamanoPagina);
}
