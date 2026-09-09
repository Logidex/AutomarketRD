using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Core.Entities.Enums;
using MediatR;

namespace AutoMarket.Application.Features.PlanCatalogo.Queries;

public record ObtenerCatalogoPublicoQuery() : IRequest<List<PlanCatalogoDto>>;
public record ObtenerPlanPorNivelQuery(PlanNivel Nivel) : IRequest<PlanCatalogoDto?>;
public record ObtenerCatalogoAdminQuery() : IRequest<List<PlanCatalogoAdminDto>>;
