using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Suscripcion;

public class PagoSuscripcionDto
{
    public int Id { get; set; }
    public int PerfilDealerId { get; set; }
    public PlanNivel Nivel { get; set; }
    public CicloFacturacion Ciclo { get; set; }
    public EstadoPago Estado { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? OrdenIdPayPal { get; set; }
    public string? Referencia { get; set; }
    public DateTime FechaUtc { get; set; }
}