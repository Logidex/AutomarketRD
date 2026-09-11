using AutoMarket.Application.DTOs.Favorito;
using MediatR;

namespace AutoMarket.Application.Features.Favoritos.Queries;

public record ObtenerFavoritosQuery(int UsuarioId) : IRequest<IEnumerable<AnuncioFavoritoDto>>;
