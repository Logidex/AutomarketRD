using AutoMarket.Application.DTOs.Admin;

namespace AutoMarket.Application.Interfaces;

public interface IDashboardService
{
    // Para Admin (global)
    Task<DashboardResumenDto> ObtenerResumenAsync();

    // Para Dealer (filtrado por usuario)
    Task<DashboardResumenDto> ObtenerResumenAsync(int dealerUsuarioId);
}