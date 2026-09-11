using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Dealer;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.PerfilDealer.Commands;
using AutoMarket.Application.Features.PerfilDealer.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.PerfilDealer.Handlers;

public class PerfilDealerCommandHandler
    : IRequestHandler<ActualizarMiPerfilCommand, PerfilDealerPublicoDto?>
{
    private readonly IPerfilDealerService _service;
    public PerfilDealerCommandHandler(IPerfilDealerService service) => _service = service;

    public async Task<PerfilDealerPublicoDto?> Handle(ActualizarMiPerfilCommand request, CancellationToken ct)
        => await _service.ActualizarMiPerfilAsync(request.DealerId, request.Dto);
}

public class PerfilDealerQueryHandler
    : IRequestHandler<ObtenerPerfilPublicoQuery, PerfilDealerPublicoDto?>,
      IRequestHandler<ListarAgenciasQuery, PagedResult<AgenciaListadoDto>>
{
    private readonly IPerfilDealerService _service;
    public PerfilDealerQueryHandler(IPerfilDealerService service) => _service = service;

    public async Task<PerfilDealerPublicoDto?> Handle(ObtenerPerfilPublicoQuery request, CancellationToken ct)
        => await _service.ObtenerPerfilPublicoAsync(request.DealerId);

    public async Task<PagedResult<AgenciaListadoDto>> Handle(ListarAgenciasQuery request, CancellationToken ct)
        => await _service.ListarAgenciasAsync(request.Busqueda, request.SoloVerificadas, request.PlanNivel, request.Pagina, request.CantidadPorPagina);
}
