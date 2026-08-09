using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Paypal;

/// <summary>
/// Detalle de la orden devuelto por PayPal (GET /v2/checkout/orders/{id}).
/// </summary>
public class DetalleOrdenPayPalDto
{
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Estado HTTP de la orden (APPROVED, COMPLETED, CREATED, VOIDED...).</summary>
    public string Status { get; set; } = string.Empty;

    public string? ReferenceId { get; set; }

    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";

    /// <summary>Perfil de dealer extraído del reference_id.</summary>
    public int? PerfilDealerId { get; set; }

    /// <summary>Plan extraído del reference_id.</summary>
    public PlanNivel? Nivel { get; set; }

    /// <summary>Ciclo extraído del reference_id.</summary>
    public CicloFacturacion? Ciclo { get; set; }
}