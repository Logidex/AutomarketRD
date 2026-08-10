using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IHistorialVistaRepository
{
    Task<HistorialVista?> ObtenerAsync(int usuarioId, int anuncioId);
    Task AgregarAsync(HistorialVista historial);
    Task GuardarCambiosAsync();
    Task<IReadOnlyCollection<HistorialVista>> ObtenerRecientesAsync(int usuarioId, int cantidad);
}