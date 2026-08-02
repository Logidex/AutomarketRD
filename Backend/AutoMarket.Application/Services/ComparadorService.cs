using AutoMarket.Application.DTOs.Anuncio;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class ComparadorService : IComparadorService
{
    private readonly IAnuncioRepository _anuncioRepository;

    public ComparadorService(IAnuncioRepository anuncioRepository)
    {
        _anuncioRepository = anuncioRepository;
    }

    public async Task<IEnumerable<AnuncioComparadorDto>> CompararVehiculosAsync(int[] ids)
    {
        // 1. Reglas de Negocio en su lugar correcto
        if (ids == null || ids.Length < 2 || ids.Length > 4)
        {
            throw new ArgumentException("Debes seleccionar entre 2 y 4 vehículos para comparar.");
        }

        var idsUnicos = ids.Distinct().ToList();
        var anuncios = await _anuncioRepository.ObtenerPorIdsAsync(idsUnicos);

        if (!anuncios.Any())
        {
            throw new KeyNotFoundException("No se encontraron los vehículos solicitados.");
        }

        // 2. Transformación de datos (Mapeo)
        return anuncios.Select(a => new AnuncioComparadorDto
        {
            Id = a.Id,
            Marca = a.Marca,
            Modelo = a.Modelo,
            Anio = a.Anio,
            Precio = a.Precio,
            Kilometraje = a.Kilometraje,
            Transmision = a.Transmision,
            Combustible = a.Combustible,
            ColorExterior = a.ColorExterior,
            FotoPrincipal = a.Fotos != null && a.Fotos.Any() ? a.Fotos.First() : null
        }).ToList();
    }
}