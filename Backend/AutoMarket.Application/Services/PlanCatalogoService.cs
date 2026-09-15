using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar PlanCatalogo.
/// </summary>
public class PlanCatalogoService : IPlanCatalogoService
{
    private readonly IPlanCatalogoRepository _repository;
    private readonly ICacheService _cache;

    private const string CACHE_CATALOGO = "planes:catalogo";
    private const string CACHE_ADMIN = "planes:admin";
    private static readonly TimeSpan TTL = TimeSpan.FromMinutes(30);

/// <summary>
/// Inicializa una nueva instancia de la clase PlanCatalogoService. Parámetro repository (IPlanCatalogoRepository)
/// </summary>
    public PlanCatalogoService(IPlanCatalogoRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<PlanCatalogoDto>> ObtenerCatalogoPublicoAsync()
    {
        var cached = await _cache.GetAsync<List<PlanCatalogoDto>>(CACHE_CATALOGO);
        if (cached is not null) return cached;

        var planes = await _repository.ObtenerTodosAsync(soloActivos: true);
        var result = planes.Select(p => MapearPublico(p)).ToList();

        await _cache.SetAsync(CACHE_CATALOGO, result, TTL);
        return result;
    }

    public async Task<PlanCatalogoDto?> ObtenerPlanPorNivelAsync(Core.Entities.Enums.PlanNivel nivel)
    {
        var plan = await _repository.ObtenerPorNivelAsync(nivel);

        if (plan is null || !plan.Activo)
            return null;

        return MapearPublico(plan);
    }

    public async Task<List<PlanCatalogoAdminDto>> ObtenerCatalogoAdminAsync()
    {
        var planes = await _repository.ObtenerTodosAsync();
        return planes.Select(p => MapearAdmin(p)).ToList();
    }

    public async Task<PlanCatalogoAdminDto> CrearPlanAsync(PlanCatalogoCreateDto dto)
    {
        var existente = await _repository.ObtenerPorNivelAsync(dto.Nivel);
        if (existente != null)
            throw new BusinessRuleException($"Ya existe un plan registrado para el nivel {dto.Nivel}.");

        if (dto.PrecioMensual < 0)
            throw new BusinessRuleException("El precio mensual no puede ser negativo.");

        if (dto.CuotaDestacados < 0)
            throw new BusinessRuleException("La cuota de destacados no puede ser negativa.");

        if (dto.LimiteAnuncios < 0 || dto.MaxFotos < 0 || dto.DiasVigencia < 0)
            throw new BusinessRuleException("Los límites del plan no pueden ser negativos.");

        var plan = new PlanCatalogo
        {
            Nivel = dto.Nivel,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            PrecioMensual = dto.PrecioMensual,
            DescuentoTrimestralPorcentaje = dto.DescuentoTrimestralPorcentaje,
            DescuentoAnualPorcentaje = dto.DescuentoAnualPorcentaje,
            LimiteAnuncios = dto.LimiteAnuncios > 0 ? dto.LimiteAnuncios : PlanConfig.LimiteAnuncios(dto.Nivel),
            MaxFotos = dto.MaxFotos > 0 ? dto.MaxFotos : PlanConfig.MaxFotos(dto.Nivel),
            DiasVigencia = dto.DiasVigencia > 0 ? dto.DiasVigencia : PlanConfig.DiasVigencia(dto.Nivel),
            CuotaDestacados = dto.CuotaDestacados,
            Activo = dto.Activo
        };

        await _repository.AgregarAsync(plan);
        await InvalidarCachePlanesAsync();
        return MapearAdmin(plan);
    }

    public async Task<PlanCatalogoAdminDto> ActualizarPlanAsync(int id, PlanCatalogoUpdateDto dto)
    {
        var plan = await _repository.ObtenerPorIdAsync(id)
            ?? throw new KeyNotFoundException($"No se encontró un plan con id {id}.");

        if (dto.PrecioMensual < 0)
            throw new BusinessRuleException("El precio mensual no puede ser negativo.");

        if (dto.CuotaDestacados < 0)
            throw new BusinessRuleException("La cuota de destacados no puede ser negativa.");

        if (dto.LimiteAnuncios < 0 || dto.MaxFotos < 0 || dto.DiasVigencia < 0)
            throw new BusinessRuleException("Los límites del plan no pueden ser negativos.");

        plan.Nombre = dto.Nombre;
        plan.Descripcion = dto.Descripcion;
        plan.PrecioMensual = dto.PrecioMensual;
        plan.DescuentoTrimestralPorcentaje = dto.DescuentoTrimestralPorcentaje;
        plan.DescuentoAnualPorcentaje = dto.DescuentoAnualPorcentaje;
        plan.LimiteAnuncios = dto.LimiteAnuncios > 0 ? dto.LimiteAnuncios : PlanConfig.LimiteAnuncios(plan.Nivel);
        plan.MaxFotos = dto.MaxFotos > 0 ? dto.MaxFotos : PlanConfig.MaxFotos(plan.Nivel);
        plan.DiasVigencia = dto.DiasVigencia > 0 ? dto.DiasVigencia : PlanConfig.DiasVigencia(plan.Nivel);
        plan.CuotaDestacados = dto.CuotaDestacados;
        plan.Activo = dto.Activo;

        await _repository.ActualizarAsync(plan);
        await InvalidarCachePlanesAsync();
        return MapearAdmin(plan);
    }

/// <summary>
/// EliminarPlanAsync Eliminar plan async. Parámetros: Parámetro id (int). Retorna: Task.
/// </summary>
    public async Task EliminarPlanAsync(int id)
    {
        // Borrado lógico: cambia el plan a inactivo sin perder historial.
        var plan = await _repository.ObtenerPorIdAsync(id)
            ?? throw new KeyNotFoundException($"No se encontró un plan con id {id}.");

        if (plan.Nivel == Core.Entities.Enums.PlanNivel.Gratis)
            throw new BusinessRuleException("El plan Gratis no puede ser desactivado, siempre debe estar disponible.");

        plan.Activo = false;
        await _repository.ActualizarAsync(plan);
        await InvalidarCachePlanesAsync();
    }

    private async Task InvalidarCachePlanesAsync()
    {
        await _cache.RemoveAsync(CACHE_CATALOGO);
        await _cache.RemoveAsync(CACHE_ADMIN);
    }

private static PlanCatalogoDto MapearPublico(PlanCatalogo plan)
    {
        var dto = new PlanCatalogoDto
        {
            Nivel = plan.Nivel,
            Nombre = plan.Nombre,
            Descripcion = plan.Descripcion,
            LimiteAnuncios = plan.LimiteAnunciosEfectivo,
            CuotaDestacados = plan.CuotaDestacados,
            MaxFotos = plan.MaxFotosEfectivo,
            DiasVigencia = plan.DiasVigenciaEfectivo,
            PrecioMensual = plan.PrecioMensual,
            DescuentoTrimestralPorcentaje = plan.DescuentoTrimestralPorcentaje,
            DescuentoAnualPorcentaje = plan.DescuentoAnualPorcentaje
        };
        CalcularPreciosPorCiclo(plan, dto);
        return dto;
    }

    private static PlanCatalogoAdminDto MapearAdmin(PlanCatalogo plan)
    {
        var dto = new PlanCatalogoAdminDto
        {
            Id = plan.Id,
            Nivel = plan.Nivel,
            Nombre = plan.Nombre,
            Descripcion = plan.Descripcion,
            LimiteAnuncios = plan.LimiteAnunciosEfectivo,
            CuotaDestacados = plan.CuotaDestacados,
            MaxFotos = plan.MaxFotosEfectivo,
            DiasVigencia = plan.DiasVigenciaEfectivo,
            PrecioMensual = plan.PrecioMensual,
            DescuentoTrimestralPorcentaje = plan.DescuentoTrimestralPorcentaje,
            DescuentoAnualPorcentaje = plan.DescuentoAnualPorcentaje,
            Activo = plan.Activo
        };
        CalcularPreciosPorCiclo(plan, dto);
        return dto;
    }

    /// <summary>
    /// Calcula el precio total por ciclo aplicando el descuento porcentual
    /// sobre el acumulado mensual:
    ///   trimestral = PrecioMensual * 3 * (1 - desc%/100)
    ///   anual      = PrecioMensual * 12 * (1 - desc%/100)
    /// </summary>
    private static void CalcularPreciosPorCiclo(PlanCatalogo plan, PlanCatalogoDto dto)
    {
        var descT = Math.Clamp(plan.DescuentoTrimestralPorcentaje, 0, 100) / 100m;
        var descA = Math.Clamp(plan.DescuentoAnualPorcentaje, 0, 100) / 100m;

        dto.PrecioTrimestral = Math.Round(plan.PrecioMensual * 3 * (1 - descT), 2);
        dto.PrecioAnual = Math.Round(plan.PrecioMensual * 12 * (1 - descA), 2);
    }

    private static void CalcularPreciosPorCiclo(PlanCatalogo plan, PlanCatalogoAdminDto dto)
    {
        var baseDto = new PlanCatalogoDto
        {
            Nivel = plan.Nivel,
            Nombre = plan.Nombre,
            Descripcion = plan.Descripcion,
            LimiteAnuncios = plan.LimiteAnuncios,
            PrecioMensual = plan.PrecioMensual,
            DescuentoTrimestralPorcentaje = plan.DescuentoTrimestralPorcentaje,
            DescuentoAnualPorcentaje = plan.DescuentoAnualPorcentaje
        };
        CalcularPreciosPorCiclo(plan, baseDto);
        dto.PrecioTrimestral = baseDto.PrecioTrimestral;
        dto.PrecioAnual = baseDto.PrecioAnual;
    }
}
