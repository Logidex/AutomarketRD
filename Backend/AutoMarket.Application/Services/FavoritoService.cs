using AutoMarket.Application.DTOs.Favorito;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class FavoritoService : IFavoritoService
{
    private readonly IFavoritoRepository _favoritoRepository;
    private readonly IAnuncioRepository _anuncioRepository;

    public FavoritoService(IFavoritoRepository favoritoRepository, IAnuncioRepository anuncioRepository)
    {
        _favoritoRepository = favoritoRepository;
        _anuncioRepository = anuncioRepository;
    }

    public async Task AgregarFavoritoAsync(int usuarioId, int anuncioId)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(anuncioId);
        if (anuncio == null) 
            throw new KeyNotFoundException("El anuncio no existe.");

        var existente = await _favoritoRepository.ObtenerAsync(usuarioId, anuncioId);
        if (existente != null) 
            throw new InvalidOperationException("El vehículo ya está en tus favoritos.");

        var nuevoFavorito = new UsuarioFavorito(usuarioId, anuncioId);
        await _favoritoRepository.AgregarAsync(nuevoFavorito);
    }

    public async Task QuitarFavoritoAsync(int usuarioId, int anuncioId)
    {
        var existente = await _favoritoRepository.ObtenerAsync(usuarioId, anuncioId);
        if (existente == null) 
            throw new KeyNotFoundException("El vehículo no estaba en tus favoritos.");

        await _favoritoRepository.EliminarAsync(existente);
    }

    public async Task<IEnumerable<AnuncioFavoritoDto>> ObtenerFavoritosAsync(int usuarioId)
    {
        var anuncios = await _favoritoRepository.ObtenerAnunciosFavoritosAsync(usuarioId);

        // Mapeo limpio y tipado
        return anuncios.Select(a => new AnuncioFavoritoDto
        {
            Id = a.Id,
            Marca = a.Marca,
            Modelo = a.Modelo,
            Anio = a.Anio,
            Precio = a.Precio,
            FotoPrincipal = a.Fotos != null && a.Fotos.Any() ? a.Fotos.First() : null
        }).ToList();
    }
}