using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Core.Entities.Enums;
using MediatR;

namespace AutoMarket.Application.Features.PlanCatalogo.Commands;

public record CrearPlanCommand(PlanCatalogoCreateDto Dto) : IRequest<PlanCatalogoAdminDto>;
public record ActualizarPlanCommand(int Id, PlanCatalogoUpdateDto Dto) : IRequest<PlanCatalogoAdminDto>;
public record EliminarPlanCommand(int Id) : IRequest;
