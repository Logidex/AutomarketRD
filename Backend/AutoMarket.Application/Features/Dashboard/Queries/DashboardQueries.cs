using AutoMarket.Application.DTOs.Admin;
using MediatR;

namespace AutoMarket.Application.Features.Dashboard.Queries;

public record ObtenerResumenDashboardQuery(int? DealerUsuarioId = null) : IRequest<DashboardResumenDto>;
