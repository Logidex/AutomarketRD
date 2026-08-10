namespace AutoMarket.Core.Entities;

public class HistorialVista
{
    // ==========================================
    // 1. LAS LLAVES (Identificadores)
    // ==========================================
    public int UsuarioId { get; private set; }
    public int AnuncioId { get; private set; }

    public DateTime VistoEnUtc { get; private set; }

    // ==========================================
    // 2. LA NAVEGACIÓN (Para Entity Framework)
    // ==========================================
    public virtual Usuario Usuario { get; private set; } = null!;
    public virtual Anuncio Anuncio { get; private set; } = null!;

    private HistorialVista() { }

    // Constructor de dominio
    public HistorialVista(int usuarioId, int anuncioId)
    {
        UsuarioId = usuarioId;
        AnuncioId = anuncioId;
        VistoEnUtc = DateTime.UtcNow;
    }

    // Re-ver el mismo vehículo actualiza su posición en el historial
    public void ActualizarVista() => VistoEnUtc = DateTime.UtcNow;
}