using AutoMarket.Application.DTOs.Admin;

namespace AutoMarket.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenAsync();
}