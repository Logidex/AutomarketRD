using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Catálogo central de planes de suscripción. Define el precio base
/// mensual y los descuentos porcentuales por cada ciclo de facturación
/// (mes, trimestre, año). Los límites de anuncios provienen del enum
/// <see cref="PlanNivel"/> para evitar duplicar la fuente de verdad.
/// </summary>
public class PlanCatalogo
{
    public int Id { get; set; }

    public PlanNivel Nivel { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    /// <summary>Precio base mensual en pesos dominicanos (RD$).</summary>
    public decimal PrecioMensual { get; set; }

    /// <summary>Descuento aplicado al costo acumulado del ciclo trimestral. 7 = −7%.</summary>
    public decimal DescuentoTrimestralPorcentaje { get; set; }

    /// <summary>Descuento aplicado al costo acumulado del ciclo anual. 15 = −15%.</summary>
    public decimal DescuentoAnualPorcentaje { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>Límite de anuncios del plan, derivado del enum.</summary>
    public int LimiteAnuncios => (int)Nivel;
}