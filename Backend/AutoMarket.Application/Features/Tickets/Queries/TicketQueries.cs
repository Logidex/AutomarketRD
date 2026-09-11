using AutoMarket.Application.DTOs.Ticket;
using MediatR;

namespace AutoMarket.Application.Features.Tickets.Queries;

public record ObtenerMisTicketsQuery(int UsuarioId)
    : IRequest<IReadOnlyCollection<TicketListadoDto>>;

public record ObtenerTicketQuery(int TicketId, int UsuarioId)
    : IRequest<TicketDetalleDto>;

public record ObtenerTicketsAdminQuery()
    : IRequest<IReadOnlyCollection<TicketListadoDto>>;

public record ObtenerTicketAdminQuery(int TicketId)
    : IRequest<TicketDetalleDto>;

public record ObtenerResumenAdminQuery()
    : IRequest<TicketResumenAdminDto>;
