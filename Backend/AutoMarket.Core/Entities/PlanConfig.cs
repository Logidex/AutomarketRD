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
        PlanNivel.Gratis => 1,
        PlanNivel.Basico => 15,
        PlanNivel.Pro => 50,
        PlanNivel.Elite => 150,
        _ => 1
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

    /// <summary>Días de vigencia de un anuncio publicado por plan.</summary>
    public static int DiasVigencia(PlanNivel nivel) => nivel switch
    {
        PlanNivel.Gratis => 30,
        PlanNivel.Basico => 30,
        PlanNivel.Pro => 45,
        PlanNivel.Elite => 60,
        _ => 30
    };

    /// <summary>Máximo de fotos permitidas por anuncio según el plan.</summary>
    public static int MaxFotos(PlanNivel nivel) => nivel switch
    {
        PlanNivel.Gratis => 8,
        PlanNivel.Basico => 8,
        PlanNivel.Pro => 20,
        PlanNivel.Elite => 50,
        _ => 8
    };
}
