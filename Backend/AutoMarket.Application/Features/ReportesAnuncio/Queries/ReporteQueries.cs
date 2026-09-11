using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Core.Entities.Enums;
using MediatR;

namespace AutoMarket.Application.Features.ReportesAnuncio.Queries;

public record ListarReportesPorEstadoQuery(ReporteEstado Estado) : IRequest<IReadOnlyCollection<ReporteAdminDto>>;
public record ContarReportesPendientesQuery() : IRequest<int>;
