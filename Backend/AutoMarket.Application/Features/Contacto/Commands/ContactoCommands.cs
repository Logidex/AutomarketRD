using AutoMarket.Application.DTOs;
using MediatR;

namespace AutoMarket.Application.Features.Contacto.Commands;

public record ProcesarMensajeContactoCommand(ContactoCreateDto Dto) : IRequest<bool>;
