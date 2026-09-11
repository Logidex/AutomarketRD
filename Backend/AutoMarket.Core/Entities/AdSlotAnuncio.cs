using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Un anuncio publicitario pagado por un dealer para mostrarse en un AdSlot.
/// Se activa inmediatamente al crearse; el admin puede rechazarlo después.
/// </summary>
public class AdSlotAnuncio
{
    public int Id { get; set; }

    public int AdSlotId { get; set; }

    public int PerfilDealerId { get; set; }

    // Contenido
    public string ImagenOriginalUrl { get; set; } = string.Empty;

    public string ImagenRedimensionadaUrl { get; set; } = string.Empty;

    public string? Enlace { get; set; }

    public string? Titulo { get; set; }

    // Fechas
    public DateTime FechaInicioUtc { get; set; }

    public DateTime FechaFinUtc { get; set; }

    // Estado
    public EstadoAdSlot Estado { get; set; } = EstadoAdSlot.Activo;

    // Pagos
    public MetodoPago MetodoPago { get; set; }

    public decimal MontoPagado { get; set; }

    public string Moneda { get; set; } = "RD$";

    public string? OrderIdPayPal { get; set; }

    public string? UrlCapturaTransferencia { get; set; }

    public EstadoTransferencia? EstadoTransferencia { get; set; }

    // Rotación ponderada
    public int Prioridad { get; set; }

    public int Impresiones { get; set; }

    public int Clicks { get; set; }

    // Notificaciones
    public DateTime? FechaRecordatorioEnviadoUtc { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    // Navegación
    public AdSlot AdSlot { get; set; } = null!;

    public PerfilDealer PerfilDealer { get; set; } = null!;

    /// <summary>
    /// Indica si el anuncio está vigente (activo y dentro del rango de fechas).
    /// </summary>
    public bool EstaVigente => Estado == EstadoAdSlot.Activo
                               && DateTime.UtcNow >= FechaInicioUtc
                               && DateTime.UtcNow <= FechaFinUtc;

    /// <summary>
    /// Calcula la prioridad de rotación basándose en el monto pagado.
    /// Un valor más alto significa más rotaciones.
    /// </summary>
    public void CalcularPrioridad(decimal precioBaseSlot)
    {
        if (precioBaseSlot <= 0) { Prioridad = 1; return; }
        Prioridad = Math.Max(1, (int)(MontoPagado / precioBaseSlot * 100));
    }

    /// <summary>
    /// Marca el anuncio como vencido.
    /// </summary>
    public void MarcarVencido()
    {
        Estado = EstadoAdSlot.Vencido;
    }

    /// <summary>
    /// Marca el anuncio como rechazado por el admin.
    /// </summary>
    public void Rechazar()
    {
        Estado = EstadoAdSlot.Rechazado;
    }

    /// <summary>
    /// Registra una impresión del anuncio.
    /// </summary>
    public void RegistrarImpresion()
    {
        Impresiones++;
    }

    /// <summary>
    /// Registra un click en el anuncio.
    /// </summary>
    public void RegistrarClick()
    {
        Clicks++;
    }

    /// <summary>
    /// Registra que se envió el recordatorio de vencimiento.
    /// </summary>
    public void MarcarRecordatorioEnviado()
    {
        FechaRecordatorioEnviadoUtc = DateTime.UtcNow;
    }
}
