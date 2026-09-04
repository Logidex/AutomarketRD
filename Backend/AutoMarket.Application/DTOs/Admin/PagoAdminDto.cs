using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Admin;

public class PagoAdminDto
{
    public int Id { get; set; }
    public int PerfilDealerId { get; set; }
    public string DealerNombreAgencia { get; set; } = string.Empty;
    public string DealerEmail { get; set; } = string.Empty;
    public PlanNivel Nivel { get; set; }
    public CicloFacturacion Ciclo { get; set; }
    public EstadoPago Estado { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? OrdenIdPayPal { get; set; }
    public string? CaptureIdPayPal { get; set; }
    public MetodoPago Metodo { get; set; }
    public EstadoTransferencia? EstadoTransferencia { get; set; }
    public string? UrlCapturaTransferencia { get; set; }
    public string? NotasAdmin { get; set; }
    public DateTime? FechaConfirmacionUtc { get; set; }
    public DateTime FechaUtc { get; set; }
}