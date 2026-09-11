using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Lead;
using MediatR;

namespace AutoMarket.Application.Features.Leads.Commands;

public record CrearLeadCommand(LeadCreateDto Dto, int? UsuarioIdRemitente)
    : IRequest;

public record MarcarLeidoCommand(int LeadId, int UsuarioId)
    : IRequest<bool>;

public record MarcarTodosLeidosCommand(int UsuarioId)
    : IRequest<int>;
