using AutoMarket.Application.DTOs.Lead;
using AutoMarket.Application.Features.Leads.Commands;
using AutoMarket.Application.Features.Leads.Queries;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Leads.Handlers;

public class LeadCommandHandler
    : IRequestHandler<CrearLeadCommand>,
      IRequestHandler<MarcarLeidoCommand, bool>,
      IRequestHandler<MarcarTodosLeidosCommand, int>
{
    private readonly ILeadService _leadService;

    public LeadCommandHandler(ILeadService leadService)
    {
        _leadService = leadService;
    }

    public async Task Handle(CrearLeadCommand request, CancellationToken ct)
    {
        await _leadService.CrearLeadAsync(request.Dto, request.UsuarioIdRemitente);
    }

    public async Task<bool> Handle(MarcarLeidoCommand request, CancellationToken ct)
    {
        return await _leadService.MarcarLeidoAsync(request.LeadId, request.UsuarioId);
    }

    public async Task<int> Handle(MarcarTodosLeidosCommand request, CancellationToken ct)
    {
        return await _leadService.MarcarTodosLeidosAsync(request.UsuarioId);
    }
}

public class LeadQueryHandler
    : IRequestHandler<ObtenerLeadsPorAnuncioQuery, IReadOnlyCollection<Lead>>,
      IRequestHandler<ObtenerLeadsPorDealerQuery, IReadOnlyCollection<LeadDealerDto>>,
      IRequestHandler<ObtenerResumenNoLeidosQuery, LeadNoLeidosResumenDto>,
      IRequestHandler<ObtenerMisContactosQuery, IReadOnlyCollection<LeadContactoUsuarioDto>>
{
    private readonly ILeadService _leadService;

    public LeadQueryHandler(ILeadService leadService)
    {
        _leadService = leadService;
    }

    public async Task<IReadOnlyCollection<Lead>> Handle(
        ObtenerLeadsPorAnuncioQuery request, CancellationToken ct)
    {
        return await _leadService.ObtenerLeadsPorAnuncioAsync(request.AnuncioId, request.UsuarioId);
    }

    public async Task<IReadOnlyCollection<LeadDealerDto>> Handle(
        ObtenerLeadsPorDealerQuery request, CancellationToken ct)
    {
        return await _leadService.ObtenerLeadsPorDealerAsync(request.DealerId);
    }

    public async Task<LeadNoLeidosResumenDto> Handle(
        ObtenerResumenNoLeidosQuery request, CancellationToken ct)
    {
        return await _leadService.ObtenerResumenNoLeidosAsync(request.UsuarioId);
    }

    public async Task<IReadOnlyCollection<LeadContactoUsuarioDto>> Handle(
        ObtenerMisContactosQuery request, CancellationToken ct)
    {
        return await _leadService.ObtenerMisContactosAsync(request.UsuarioId);
    }
}
