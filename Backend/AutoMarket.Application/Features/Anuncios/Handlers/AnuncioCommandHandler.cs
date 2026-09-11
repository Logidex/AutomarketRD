using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Anuncios.Commands;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Anuncios.Handlers;

public class AnuncioCommandHandler :
    IRequestHandler<CrearAnuncioCommand, int>,
    IRequestHandler<ActualizarAnuncioCommand, AnuncioUpdateDto?>,
    IRequestHandler<PublicarAnuncioCommand, bool>,
    IRequestHandler<CambiarEstadoAnuncioCommand, bool>,
    IRequestHandler<EliminarAnuncioCommand, bool>,
    IRequestHandler<RenovarAnuncioGratisCommand, bool>,
    IRequestHandler<MarcarDestacadoCommand, bool>,
    IRequestHandler<QuitarDestacadoCommand, bool>,
    IRequestHandler<RegistrarVistaCommand>
{
    private readonly IAnuncioService _service;

    public AnuncioCommandHandler(IAnuncioService service)
    {
        _service = service;
    }

    public async Task<int> Handle(CrearAnuncioCommand request, CancellationToken ct)
        => await _service.CrearAnuncioAsync(request.Dto);

    public async Task<AnuncioUpdateDto?> Handle(ActualizarAnuncioCommand request, CancellationToken ct)
        => await _service.ActualizarAsync(request.Id, request.UsuarioId, request.Dto);

    public async Task<bool> Handle(PublicarAnuncioCommand request, CancellationToken ct)
        => await _service.PublicarAnuncioAsync(request.Id, request.UsuarioId);

    public async Task<bool> Handle(CambiarEstadoAnuncioCommand request, CancellationToken ct)
        => await _service.CambiarEstadoAsync(request.Id, request.UsuarioId, request.Estado);

    public async Task<bool> Handle(EliminarAnuncioCommand request, CancellationToken ct)
        => await _service.EliminarAnuncioAsync(request.Id, request.UsuarioId);

    public async Task<bool> Handle(RenovarAnuncioGratisCommand request, CancellationToken ct)
        => await _service.RenovarAnuncioGratisAsync(request.Id, request.UsuarioId);

    public async Task<bool> Handle(MarcarDestacadoCommand request, CancellationToken ct)
        => await _service.MarcarComoDestacadoAsync(request.Id, request.UsuarioId);

    public async Task<bool> Handle(QuitarDestacadoCommand request, CancellationToken ct)
        => await _service.QuitarDestacadoAsync(request.Id, request.UsuarioId);

    public async Task Handle(RegistrarVistaCommand request, CancellationToken ct)
        => await _service.RegistrarVistaAsync(request.AnuncioId);
}
