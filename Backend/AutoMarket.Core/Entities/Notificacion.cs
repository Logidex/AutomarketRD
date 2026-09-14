namespace AutoMarket.Core.Entities;

/// <summary>
/// Notificación in-app para un usuario.
/// Se crea cuando ocurre un evento relevante (lead nuevo, suscripción venciendo, etc.)
/// </summary>
public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // "lead", "suscripcion", "transferencia", "general"
    public string? UrlDestino { get; set; }
    public bool Leido { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;

    // Relación
    public Usuario Usuario { get; set; } = null!;
}
