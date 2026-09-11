using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Features.Dashboard.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Dashboard.Handlers;

public class DashboardQueryHandler
    : IRequestHandler<ObtenerResumenDashboardQuery, DashboardResumenDto>
{
    private readonly IDashboardService _service;
    public DashboardQueryHandler(IDashboardService service) => _service = service;

    public async Task<DashboardResumenDto> Handle(ObtenerResumenDashboardQuery request, CancellationToken ct)
    {
        return request.DealerUsuarioId.HasValue
            ? await _service.ObtenerResumenAsync(request.DealerUsuarioId.Value)
            : await _service.ObtenerResumenAsync();
    }
}
