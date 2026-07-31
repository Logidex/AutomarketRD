using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Interfaces;

public interface ISuscripcionService
{
    Task AsignarPlanInicialAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo);
    Task CambiarPlanAsync(int dealerId, PlanNivel nuevoPlan, CicloFacturacion ciclo);
    Task RenovarManualAsync(int perfilDealerId, DateTime nuevaFechaVencimiento);
}