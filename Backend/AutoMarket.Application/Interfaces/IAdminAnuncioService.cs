namespace AutoMarket.Application.Interfaces;

public interface IAdminAnuncioService
{
    Task<IEnumerable<object>> ListarAnunciosParaAdminAsync();
    Task<(IEnumerable<object> Items, int Total)> ListarAnunciosPaginadosAsync(int pagina, int tamanoPagina);
    Task<bool> EliminarAnuncioForzosoAsync(int anuncioId);
}
