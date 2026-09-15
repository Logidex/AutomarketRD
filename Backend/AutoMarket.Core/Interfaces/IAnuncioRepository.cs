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
    Task<int> ContarAnunciosActivosVendedorAsync(int usuarioId);
    Task<int> ContarDestacadosPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<Anuncio>> ObtenerTodosParaAdminAsync();
    Task<(IEnumerable<Anuncio> Anuncios, int Total)> ObtenerTodosPaginadosAsync(int pagina, int tamanoPagina);
    void Eliminar(Anuncio anuncio);
    Task<IEnumerable<Anuncio>> ObtenerPorIdsAsync(IEnumerable<int> ids);
    Task<(IEnumerable<Anuncio> Anuncios, int Total)> ObtenerPaginadosAsync(int pagina, int tamanoPagina);
    Task<(IEnumerable<Anuncio> Anuncios, int Total)> ObtenerDestacadosPaginadosAsync(int pagina, int tamanoPagina);
    Task<bool> ExisteFotoAsync(string claveOUrl);
}