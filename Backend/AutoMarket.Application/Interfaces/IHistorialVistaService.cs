using AutoMarket.Application.DTOs.Historial;

namespace AutoMarket.Application.Interfaces;

public interface IHistorialVistaService
{
    Task RegistrarVistaAsync(int usuarioId, int anuncioId);
    Task<IReadOnlyCollection<AnuncioRecienteDto>> ObtenerRecientesAsync(int usuarioId, int cantidad);
}