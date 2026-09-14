namespace AutoMarket.Core.Entities;

/// <summary>
/// Job de correo electrónico pendiente de envío con reintentos.
/// Se crea cuando un envío falla. Un background service reintenta automáticamente.
/// </summary>
public class EmailJob
{
    public int Id { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string CuerpoHtml { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Enviado, Fallido
    public int Intentos { get; set; }
    public int MaxIntentos { get; set; } = 3;
    public string? UltimoError { get; set; }
    public DateTime? ProximoReintentoUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
}
