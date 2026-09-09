using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Application.Features.Tickets.Commands;
using AutoMarket.Application.Features.Tickets.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Tickets.Handlers;

public class TicketCommandHandler
    : IRequestHandler<CrearTicketCommand, int>,
      IRequestHandler<AgregarMensajeTicketCommand>,
      IRequestHandler<CerrarTicketCommand>,
      IRequestHandler<AgregarMensajeAdminTicketCommand>,
      IRequestHandler<CambiarEstadoTicketCommand>
{
    private readonly ITicketService _ticketService;

    public TicketCommandHandler(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task<int> Handle(CrearTicketCommand request, CancellationToken ct)
    {
        return await _ticketService.CrearTicketAsync(request.Dto, request.UsuarioId);
    }

    public async Task Handle(AgregarMensajeTicketCommand request, CancellationToken ct)
    {
        await _ticketService.ResponderTicketAsync(request.TicketId, request.Dto, request.UsuarioId);
    }

    public async Task Handle(CerrarTicketCommand request, CancellationToken ct)
    {
        await _ticketService.CerrarTicketAsync(request.TicketId, request.UsuarioId);
    }

    public async Task Handle(AgregarMensajeAdminTicketCommand request, CancellationToken ct)
    {
        await _ticketService.ResponderTicketAdminAsync(request.TicketId, request.Dto, request.AdminId);
    }

    public async Task Handle(CambiarEstadoTicketCommand request, CancellationToken ct)
    {
        await _ticketService.CambiarEstadoAdminAsync(request.TicketId, request.Dto);
    }
}

public class TicketQueryHandler
    : IRequestHandler<ObtenerMisTicketsQuery, IReadOnlyCollection<TicketListadoDto>>,
      IRequestHandler<ObtenerTicketQuery, TicketDetalleDto>,
      IRequestHandler<ObtenerTicketsAdminQuery, IReadOnlyCollection<TicketListadoDto>>,
      IRequestHandler<ObtenerTicketAdminQuery, TicketDetalleDto>,
      IRequestHandler<ObtenerResumenAdminQuery, TicketResumenAdminDto>
{
    private readonly ITicketService _ticketService;

    public TicketQueryHandler(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task<IReadOnlyCollection<TicketListadoDto>> Handle(
        ObtenerMisTicketsQuery request, CancellationToken ct)
    {
        return await _ticketService.ObtenerMisTicketsAsync(request.UsuarioId);
    }

    public async Task<TicketDetalleDto> Handle(
        ObtenerTicketQuery request, CancellationToken ct)
    {
        return await _ticketService.ObtenerTicketAsync(request.TicketId, request.UsuarioId);
    }

    public async Task<IReadOnlyCollection<TicketListadoDto>> Handle(
        ObtenerTicketsAdminQuery request, CancellationToken ct)
    {
        return await _ticketService.ObtenerTicketsAdminAsync();
    }

    public async Task<TicketDetalleDto> Handle(
        ObtenerTicketAdminQuery request, CancellationToken ct)
    {
        return await _ticketService.ObtenerTicketAdminAsync(request.TicketId);
    }

    public async Task<TicketResumenAdminDto> Handle(
        ObtenerResumenAdminQuery request, CancellationToken ct)
    {
        return await _ticketService.ObtenerResumenAdminAsync();
    }
}
