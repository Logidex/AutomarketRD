using AutoMarket.Core.Entities;
namespace AutoMarket.Core.Interfaces;

public interface IAnuncioRepository
{
    Task AgregarAsync(Anuncio anuncio);
    Task<Anuncio?> ObtenerPorIdAsync(int id);
    Task GuardarCambiosAsync();
    Task ActualizarAsync(Anuncio anuncio);
    Task<IReadOnlyCollection<Anuncio>> ObtenerTodosLosAnuncios();
    Task<(IEnumerable<Anuncio> Anuncios, int TotalRegistros)> BuscarPaginadoAsync(AnuncioQueryFilter filtro);
    Task<int> ContarAnunciosPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<Anuncio>> ObtenerTodosParaAdminAsync();
    void Eliminar(Anuncio anuncio);
}