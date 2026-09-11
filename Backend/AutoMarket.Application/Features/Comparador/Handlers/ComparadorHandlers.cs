using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.Features.Comparador.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Comparador.Handlers;

public class ComparadorQueryHandler
    : IRequestHandler<CompararVehiculosQuery, IEnumerable<AnuncioComparadorDto>>
{
    private readonly IComparadorService _service;
    public ComparadorQueryHandler(IComparadorService service) => _service = service;

    public async Task<IEnumerable<AnuncioComparadorDto>> Handle(CompararVehiculosQuery request, CancellationToken ct)
        => await _service.CompararVehiculosAsync(request.Ids);
}
