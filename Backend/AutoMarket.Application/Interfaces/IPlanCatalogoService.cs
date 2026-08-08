using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Interfaces;

public interface IPlanCatalogoService
{
    /// <summary>Catálogo público de planes activos con precios calculados por ciclo.</summary>
    Task<List<PlanCatalogoDto>> ObtenerCatalogoPublicoAsync();

    /// <summary>Plan público activo según su nivel, o null si no existe o está inactivo.</summary>
    Task<PlanCatalogoDto?> ObtenerPlanPorNivelAsync(PlanNivel nivel);

    /// <summary>Catálogo completo para gestión admin (incluye inactivos).</summary>
    Task<List<PlanCatalogoAdminDto>> ObtenerCatalogoAdminAsync();

    Task<PlanCatalogoAdminDto> CrearPlanAsync(PlanCatalogoCreateDto dto);

    Task<PlanCatalogoAdminDto> ActualizarPlanAsync(int id, PlanCatalogoUpdateDto dto);

    Task EliminarPlanAsync(int id);
}