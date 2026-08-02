using AutoMarket.Application.DTOs.Favorito;

namespace AutoMarket.Application.Interfaces;

public interface IFavoritoService
{
    Task AgregarFavoritoAsync(int usuarioId, int anuncioId);
    Task QuitarFavoritoAsync(int usuarioId, int anuncioId);
    Task<IEnumerable<AnuncioFavoritoDto>> ObtenerFavoritosAsync(int usuarioId);
}