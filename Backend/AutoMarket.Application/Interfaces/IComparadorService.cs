using AutoMarket.Application.DTOs.Anuncio;

namespace AutoMarket.Application.Interfaces;

public interface IComparadorService
{
    Task<IEnumerable<AnuncioComparadorDto>> CompararVehiculosAsync(int[] ids);
}