using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Application.Features.ReportesAnuncio.Commands;
using AutoMarket.Application.Features.ReportesAnuncio.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.ReportesAnuncio.Handlers;

public class ReporteCommandHandler
    : IRequestHandler<CrearReporteCommand, int>,
      IRequestHandler<DescartarReporteCommand>,
      IRequestHandler<ResolverReporteCommand>
{
    private readonly IReporteAnuncioService _service;
    public ReporteCommandHandler(IReporteAnuncioService service) => _service = service;

    public async Task<int> Handle(CrearReporteCommand request, CancellationToken ct)
        => await _service.CrearAsync(request.Dto, request.IpReportante);

    public async Task Handle(DescartarReporteCommand request, CancellationToken ct)
        => await _service.DescartarAsync(request.ReporteId, request.AdminId);

    public async Task Handle(ResolverReporteCommand request, CancellationToken ct)
        => await _service.ResolverEliminandoAnuncioAsync(request.ReporteId, request.AdminId);
}

public class ReporteQueryHandler
    : IRequestHandler<ListarReportesPorEstadoQuery, IReadOnlyCollection<ReporteAdminDto>>,
      IRequestHandler<ContarReportesPendientesQuery, int>
{
    private readonly IReporteAnuncioService _service;
    public ReporteQueryHandler(IReporteAnuncioService service) => _service = service;

    public async Task<IReadOnlyCollection<ReporteAdminDto>> Handle(ListarReportesPorEstadoQuery request, CancellationToken ct)
        => await _service.ListarPorEstadoAsync(request.Estado);

    public async Task<int> Handle(ContarReportesPendientesQuery request, CancellationToken ct)
        => await _service.ContarPendientesAsync();
}
