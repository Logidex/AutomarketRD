using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Planes;

/// <summary>Plan de suscripción visible en el catálogo público.</summary>
public class PlanCatalogoDto
{
    public PlanNivel Nivel { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int LimiteAnuncios { get; set; }

    /// <summary>Precio base mensual en RD$.</summary>
    public decimal PrecioMensual { get; set; }

    /// <summary>Precio total del ciclo trimestral en RD$ (con descuento aplicado).</summary>
    public decimal PrecioTrimestral { get; set; }

    /// <summary>Precio total del ciclo anual en RD$ (con descuento aplicado).</summary>
    public decimal PrecioAnual { get; set; }

    public decimal DescuentoTrimestralPorcentaje { get; set; }
    public decimal DescuentoAnualPorcentaje { get; set; }
}

/// <summary>Plan completo para la gestión admin (incluye Activo).</summary>
public class PlanCatalogoAdminDto : PlanCatalogoDto
{
    public int Id { get; set; }
    public bool Activo { get; set; }
}

public class PlanCatalogoCreateDto
{
    public PlanNivel Nivel { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public decimal DescuentoTrimestralPorcentaje { get; set; }
    public decimal DescuentoAnualPorcentaje { get; set; }
    public bool Activo { get; set; } = true;
}

public class PlanCatalogoUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public decimal DescuentoTrimestralPorcentaje { get; set; }
    public decimal DescuentoAnualPorcentaje { get; set; }
    public bool Activo { get; set; }
}