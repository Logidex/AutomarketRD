using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Interfaces;

public interface IReporteAnuncioRepository
{
    Task AgregarAsync(ReporteAnuncio reporte);
    Task<ReporteAnuncio?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyCollection<ReporteAnuncio>> ListarPorEstadoAsync(ReporteEstado estado);
    Task<int> ContarPendientesAsync();
    Task GuardarCambiosAsync();
}
