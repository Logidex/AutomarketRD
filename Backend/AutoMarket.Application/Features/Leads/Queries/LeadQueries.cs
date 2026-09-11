using AutoMarket.Application.DTOs.Lead;
using AutoMarket.Core.Entities;
using MediatR;

namespace AutoMarket.Application.Features.Leads.Queries;

public record ObtenerLeadsPorAnuncioQuery(int AnuncioId, int UsuarioId)
    : IRequest<IReadOnlyCollection<Lead>>;

public record ObtenerLeadsPorDealerQuery(int DealerId)
    : IRequest<IReadOnlyCollection<LeadDealerDto>>;

public record ObtenerResumenNoLeidosQuery(int UsuarioId)
    : IRequest<LeadNoLeidosResumenDto>;

public record ObtenerMisContactosQuery(int UsuarioId)
    : IRequest<IReadOnlyCollection<LeadContactoUsuarioDto>>;
