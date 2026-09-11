using AutoMarket.Application.DTOs;
using MediatR;

namespace AutoMarket.Application.Features.Anuncios.Queries;

public record ObtenerAnuncioPorIdQuery : IRequest<AnuncioDto?>
{
    public int Id { get; init; }
    public int? UsuarioId { get; init; }
}

public record ObtenerTodosLosAnunciosQuery : IRequest<IReadOnlyCollection<AnuncioListadoDto>>;

public record BuscarAnunciosQuery : IRequest<PagedResult<AnuncioListadoDto>>
{
    public AnuncioSearchDto Dto { get; init; } = default!;
}

public record ObtenerDestacadosQuery : IRequest<PagedResult<AnuncioListadoDto>>
{
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
}
