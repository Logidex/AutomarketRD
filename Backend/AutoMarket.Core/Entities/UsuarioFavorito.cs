namespace AutoMarket.Core.Entities;

public class UsuarioFavorito
{
    // ==========================================
    // 1. LAS LLAVES (Identificadores)
    // ==========================================
    public int UsuarioId { get; private set; }
    public int AnuncioId { get; private set; }
    
    public DateTime FechaAgregadoUtc { get; private set; }

    // ==========================================
    // 2. LA NAVEGACIÓN (Para Entity Framework)
    // ==========================================
    public virtual Usuario Usuario { get; private set; } = null!;
    public virtual Anuncio Anuncio { get; private set; } = null!;

    private UsuarioFavorito() { } 

    // Constructor de dominio
    public UsuarioFavorito(int usuarioId, int anuncioId)
    {
        UsuarioId = usuarioId;
        AnuncioId = anuncioId;
        FechaAgregadoUtc = DateTime.UtcNow;
    }
}