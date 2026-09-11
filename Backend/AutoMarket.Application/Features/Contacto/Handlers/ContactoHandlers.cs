using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Contacto.Commands;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Contacto.Handlers;

public class ContactoCommandHandler
    : IRequestHandler<ProcesarMensajeContactoCommand, bool>
{
    private readonly IContactoService _service;
    public ContactoCommandHandler(IContactoService service) => _service = service;

    public async Task<bool> Handle(ProcesarMensajeContactoCommand request, CancellationToken ct)
        => await _service.ProcesarMensajeContactoAsync(request.Dto);
}
