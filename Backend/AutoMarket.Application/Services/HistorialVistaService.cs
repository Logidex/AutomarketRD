using AutoMarket.Application.DTOs.Historial;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class HistorialVistaService : IHistorialVistaService
{
    private const int CANTIDAD_MAXIMA = 50;
    private const int CANTIDAD_DEFECTO = 12;

    private readonly IHistorialVistaRepository _historialRepository;
    private readonly IAnuncioRepository _anuncioRepository;

    public HistorialVistaService(
        IHistorialVistaRepository historialRepository,
        IAnuncioRepository anuncioRepository)
    {
        _historialRepository = historialRepository;
        _anuncioRepository = anuncioRepository;
    }

    public async Task RegistrarVistaAsync(int usuarioId, int anuncioId)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(anuncioId);

        // Solo se guarda el historial de anuncios visibles en la vitrina.
        if (anuncio == null || anuncio.Estado != "Publicado")
            return;

        var existente = await _historialRepository.ObtenerAsync(usuarioId, anuncioId);

        if (existente != null)
        {
            existente.ActualizarVista();
            await _historialRepository.GuardarCambiosAsync();
            return;
        }

        await _historialRepository.AgregarAsync(new HistorialVista(usuarioId, anuncioId));
    }

    public async Task<IReadOnlyCollection<AnuncioRecienteDto>> ObtenerRecientesAsync(int usuarioId, int cantidad)
    {
        if (cantidad <= 0 || cantidad > CANTIDAD_MAXIMA)
            cantidad = CANTIDAD_DEFECTO;

        var vistas = await _historialRepository.ObtenerRecientesAsync(usuarioId, cantidad);

        return vistas
            .Select(v => new AnuncioRecienteDto
            {
                Id = v.Anuncio.Id,
                Marca = v.Anuncio.Marca,
                Modelo = v.Anuncio.Modelo,
                Anio = v.Anuncio.Anio,
                Precio = v.Anuncio.Precio,
                FotoPrincipal = AnuncioFotos.ObtenerPrincipal(v.Anuncio.Fotos),
                VistoEnUtc = v.VistoEnUtc
            })
            .ToList();
    }
}