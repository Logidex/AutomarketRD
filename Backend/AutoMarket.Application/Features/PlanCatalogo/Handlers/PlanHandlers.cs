using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Features.PlanCatalogo.Commands;
using AutoMarket.Application.Features.PlanCatalogo.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.PlanCatalogo.Handlers;

public class PlanCommandHandler
    : IRequestHandler<CrearPlanCommand, PlanCatalogoAdminDto>,
      IRequestHandler<ActualizarPlanCommand, PlanCatalogoAdminDto>,
      IRequestHandler<EliminarPlanCommand>
{
    private readonly IPlanCatalogoService _service;
    public PlanCommandHandler(IPlanCatalogoService service) => _service = service;

    public async Task<PlanCatalogoAdminDto> Handle(CrearPlanCommand request, CancellationToken ct)
        => await _service.CrearPlanAsync(request.Dto);

    public async Task<PlanCatalogoAdminDto> Handle(ActualizarPlanCommand request, CancellationToken ct)
        => await _service.ActualizarPlanAsync(request.Id, request.Dto);

    public async Task Handle(EliminarPlanCommand request, CancellationToken ct)
        => await _service.EliminarPlanAsync(request.Id);
}

public class PlanQueryHandler
    : IRequestHandler<ObtenerCatalogoPublicoQuery, List<PlanCatalogoDto>>,
      IRequestHandler<ObtenerPlanPorNivelQuery, PlanCatalogoDto?>,
      IRequestHandler<ObtenerCatalogoAdminQuery, List<PlanCatalogoAdminDto>>
{
    private readonly IPlanCatalogoService _service;
    public PlanQueryHandler(IPlanCatalogoService service) => _service = service;

    public async Task<List<PlanCatalogoDto>> Handle(ObtenerCatalogoPublicoQuery request, CancellationToken ct)
        => await _service.ObtenerCatalogoPublicoAsync();

    public async Task<PlanCatalogoDto?> Handle(ObtenerPlanPorNivelQuery request, CancellationToken ct)
        => await _service.ObtenerPlanPorNivelAsync(request.Nivel);

    public async Task<List<PlanCatalogoAdminDto>> Handle(ObtenerCatalogoAdminQuery request, CancellationToken ct)
        => await _service.ObtenerCatalogoAdminAsync();
}
