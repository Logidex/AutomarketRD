using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Interfaces;

public interface IPlanCatalogoRepository
{
    Task<PlanCatalogo?> ObtenerPorIdAsync(int id);
    Task<PlanCatalogo?> ObtenerPorNivelAsync(PlanNivel nivel);
    Task<List<PlanCatalogo>> ObtenerTodosAsync(bool soloActivos = false);
    Task AgregarAsync(PlanCatalogo plan);
    Task ActualizarAsync(PlanCatalogo plan);
}