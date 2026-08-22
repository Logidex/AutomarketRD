using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Application.DTOs.Reportes;

namespace AutoMarket.Application.Interfaces;

public interface IReporteAnuncioService
{
    Task<int> CrearAsync(CrearReporteDto dto, string ipReportante);
    Task<IReadOnlyCollection<ReporteAdminDto>> ListarPorEstadoAsync(ReporteEstado estado);
    Task<int> ContarPendientesAsync();
    Task DescartarAsync(int reporteId, int adminId);
    /// <summary>Marca el reporte como resuelto y elimina el anuncio reportado con sus fotos.</summary>
    Task ResolverEliminandoAnuncioAsync(int reporteId, int adminId);
}
