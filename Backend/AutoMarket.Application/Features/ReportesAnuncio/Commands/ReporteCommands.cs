using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Core.Entities.Enums;
using MediatR;

namespace AutoMarket.Application.Features.ReportesAnuncio.Commands;

public record CrearReporteCommand(CrearReporteDto Dto, string IpReportante) : IRequest<int>;
public record DescartarReporteCommand(int ReporteId, int AdminId) : IRequest;
public record ResolverReporteCommand(int ReporteId, int AdminId) : IRequest;
