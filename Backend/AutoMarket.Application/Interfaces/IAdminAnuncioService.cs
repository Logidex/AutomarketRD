namespace AutoMarket.Application.Interfaces;

public interface IAdminAnuncioService
{
    Task<IEnumerable<object>> ListarAnunciosParaAdminAsync();
    Task<bool> EliminarAnuncioForzosoAsync(int anuncioId);
}
