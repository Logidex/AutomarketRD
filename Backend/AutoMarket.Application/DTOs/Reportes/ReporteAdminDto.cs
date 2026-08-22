using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Reportes;

/// <summary>Vista de un reporte para el panel de administración.</summary>
public class ReporteAdminDto
{
    public int Id { get; set; }
    public ReporteMotivo Motivo { get; set; }
    public string? Detalle { get; set; }
    public ReporteEstado Estado { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public string IpReportante { get; set; } = string.Empty;

    public int AnuncioId { get; set; }
    public string AnuncioTitulo { get; set; } = string.Empty;
    public string AnuncioEstado { get; set; } = string.Empty;
    public string? AnuncioFotoPrincipal { get; set; }
    public decimal AnuncioPrecio { get; set; }
    public string AnuncioMoneda { get; set; } = "DOP";
}
