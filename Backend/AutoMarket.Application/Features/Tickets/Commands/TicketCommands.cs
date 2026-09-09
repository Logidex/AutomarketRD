using AutoMarket.Application.DTOs.Ticket;
using MediatR;

namespace AutoMarket.Application.Features.Tickets.Commands;

public record CrearTicketCommand(TicketCreateDto Dto, int UsuarioId)
    : IRequest<int>;

public record AgregarMensajeTicketCommand(int TicketId, TicketMensajeCreateDto Dto, int UsuarioId)
    : IRequest;

public record CerrarTicketCommand(int TicketId, int UsuarioId)
    : IRequest;

public record AgregarMensajeAdminTicketCommand(int TicketId, TicketMensajeCreateDto Dto, int AdminId)
    : IRequest;

public record CambiarEstadoTicketCommand(int TicketId, CambiarEstadoTicketDto Dto)
    : IRequest;
