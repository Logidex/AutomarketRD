using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IFavoritoRepository
{
    Task AgregarAsync(UsuarioFavorito favorito);
    Task EliminarAsync(UsuarioFavorito favorito);
    Task<UsuarioFavorito?> ObtenerAsync(int usuarioId, int anuncioId);
    Task<IEnumerable<Anuncio>> ObtenerAnunciosFavoritosAsync(int usuarioId);
}