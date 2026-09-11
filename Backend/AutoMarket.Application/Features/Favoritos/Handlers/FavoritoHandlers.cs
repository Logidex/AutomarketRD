using AutoMarket.Application.DTOs.Favorito;
using AutoMarket.Application.Features.Favoritos.Commands;
using AutoMarket.Application.Features.Favoritos.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Favoritos.Handlers;

public class FavoritoCommandHandler
    : IRequestHandler<AgregarFavoritoCommand>,
      IRequestHandler<QuitarFavoritoCommand>
{
    private readonly IFavoritoService _service;
    public FavoritoCommandHandler(IFavoritoService service) => _service = service;

    public async Task Handle(AgregarFavoritoCommand request, CancellationToken ct)
        => await _service.AgregarFavoritoAsync(request.UsuarioId, request.AnuncioId);

    public async Task Handle(QuitarFavoritoCommand request, CancellationToken ct)
        => await _service.QuitarFavoritoAsync(request.UsuarioId, request.AnuncioId);
}

public class FavoritoQueryHandler
    : IRequestHandler<ObtenerFavoritosQuery, IEnumerable<AnuncioFavoritoDto>>
{
    private readonly IFavoritoService _service;
    public FavoritoQueryHandler(IFavoritoService service) => _service = service;

    public async Task<IEnumerable<AnuncioFavoritoDto>> Handle(ObtenerFavoritosQuery request, CancellationToken ct)
        => await _service.ObtenerFavoritosAsync(request.UsuarioId);
}
