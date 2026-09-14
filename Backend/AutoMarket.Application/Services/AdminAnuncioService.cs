using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class AdminAnuncioService : IAdminAnuncioService
{
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;

    public AdminAnuncioService(IAnuncioRepository anuncioRepository, IAlmacenadorArchivos almacenadorArchivos)
    {
        _anuncioRepository = anuncioRepository;
        _almacenadorArchivos = almacenadorArchivos;
    }

    public async Task<IEnumerable<object>> ListarAnunciosParaAdminAsync()
    {
        var anuncios = await _anuncioRepository.ObtenerTodosParaAdminAsync();
        return anuncios.Select(a => new
        {
            a.Id,
            a.Marca,
            a.Modelo,
            a.Precio,
            a.Moneda,
            a.UsuarioId
        });
    }

    public async Task<(IEnumerable<object> Items, int Total)> ListarAnunciosPaginadosAsync(int pagina, int tamanoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 100) tamanoPagina = 20;

        var (anuncios, total) = await _anuncioRepository.ObtenerTodosPaginadosAsync(pagina, tamanoPagina);
        var items = anuncios.Select(a => new
        {
            a.Id,
            a.Marca,
            a.Modelo,
            a.Precio,
            a.Moneda,
            a.UsuarioId
        });
        return (items, total);
    }

    public async Task<bool> EliminarAnuncioForzosoAsync(int anuncioId)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(anuncioId);
        if (anuncio is null) return false;

        if (anuncio.Fotos is not null)
        {
            foreach (var urlFoto in anuncio.Fotos)
                await _almacenadorArchivos.EliminarArchivoAsync(urlFoto);
        }

        _anuncioRepository.Eliminar(anuncio);
        await _anuncioRepository.GuardarCambiosAsync();
        return true;
    }
}
