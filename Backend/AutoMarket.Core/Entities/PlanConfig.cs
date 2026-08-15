using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Configuración central de los planes de suscripción.
/// Los valores del enum <see cref="PlanNivel"/> se mantienen como
/// identificadores estables (no cambian los límites almacenados en BD);
/// los límites y cuotas reales viven aquí.
/// </summary>
public static class PlanConfig
{
    /// <summary>Límite de anuncios publicados permitidos por plan.</summary>
    public static int LimiteAnuncios(PlanNivel nivel) => nivel switch
    {
        PlanNivel.Gratis => 3,
        PlanNivel.Basico => 15,
        PlanNivel.Pro => 50,
        PlanNivel.Elite => 150,
        _ => 3
    };

    /// <summary>Cuota de anuncios destacados en la portada por plan.</summary>
    public static int CuotaDestacados(PlanNivel nivel) => nivel switch
    {
        PlanNivel.Gratis => 0,
        PlanNivel.Basico => 0,
        PlanNivel.Pro => 5,
        PlanNivel.Elite => 20,
        _ => 0
    };
}
