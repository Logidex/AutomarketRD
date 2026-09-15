namespace AutoMarket.Core.Entities;

/// <summary>
/// Registro de auditoría para acciones administrativas.
/// Captura quién hizo qué cambio, en qué entidad, y el estado antes/después.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string? JsonAntes { get; set; }
    public string? JsonDespues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
}
