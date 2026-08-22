using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Reporte de un anuncio hecho por cualquier visitante (anónimo permitido).
/// Un admin lo gestiona desde el panel: lo descarta o resuelve eliminando
/// el anuncio reportado.
/// </summary>
public class ReporteAnuncio
{
    public int Id { get; private set; }

    public int AnuncioId { get; private set; }
    public Anuncio Anuncio { get; private set; } = null!;

    public ReporteMotivo Motivo { get; private set; }
    public string? Detalle { get; private set; }

    // IP del reportante (con ForwardedHeaders activo es la IP real del cliente).
    // Sirve para rate limiting e investigación de abuso de reportes.
    public string IpReportante { get; private set; } = null!;

    public ReporteEstado Estado { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }

    public int? ResueltoPorAdminId { get; private set; }
    public DateTime? FechaResolucionUtc { get; private set; }

    private ReporteAnuncio() { }

    public ReporteAnuncio(
        int anuncioId,
        ReporteMotivo motivo,
        string? detalle,
        string ipReportante)
    {
        if (anuncioId <= 0)
            throw new ArgumentException("El ID del anuncio es inválido.");

        if (!string.IsNullOrWhiteSpace(detalle) && detalle.Length > 500)
            throw new BusinessRuleException("El detalle no puede exceder los 500 caracteres.");

        if (string.IsNullOrWhiteSpace(ipReportante))
            throw new ArgumentException("La IP del reportante es obligatoria.");

        AnuncioId = anuncioId;
        Motivo = motivo;
        Detalle = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim();
        IpReportante = ipReportante.Trim();
        Estado = ReporteEstado.Pendiente;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    /// <summary>El admin revisó el reporte y decidió que no procede.</summary>
    public void Descartar(int adminId)
    {
        if (Estado != ReporteEstado.Pendiente)
            throw new BusinessRuleException("Este reporte ya fue gestionado.");

        Estado = ReporteEstado.Descartado;
        ResueltoPorAdminId = adminId;
        FechaResolucionUtc = DateTime.UtcNow;
    }

    /// <summary>El admin confirmó la infracción y eliminó el anuncio.</summary>
    public void Resolver(int adminId)
    {
        if (Estado != ReporteEstado.Pendiente)
            throw new BusinessRuleException("Este reporte ya fue gestionado.");

        Estado = ReporteEstado.Resuelto;
        ResueltoPorAdminId = adminId;
        FechaResolucionUtc = DateTime.UtcNow;
    }
}
