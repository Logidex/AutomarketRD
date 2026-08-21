namespace AutoMarket.Core.Entities;

/// <summary>
/// Token de renovación de sesión. Se guarda hasheado (SHA-256); el valor
/// crudo solo lo conoce el navegador vía cookie HttpOnly. La rotación marca
/// el token viejo como revocado apuntando a su reemplazo; reutilizar un
/// token ya rotado indica robo y revoca toda la familia del usuario.
/// </summary>
public class RefreshToken
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    private RefreshToken() { }

    public RefreshToken(
        int usuarioId,
        string tokenHash,
        DateTime creadoUtc,
        DateTime expiraUtc)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("El ID de usuario es inválido.");
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("El hash del token es obligatorio.");

        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        CreatedAtUtc = creadoUtc;
        ExpiresAtUtc = expiraUtc;
    }

    /// <summary>Vigente: sin revocar y no expirado.</summary>
    public bool EstaActivo =>
        RevokedAtUtc == null && ExpiresAtUtc > DateTime.UtcNow;

    /// <summary>Ya fue usado y reemplazado por otro token.</summary>
    public bool FueRotado => RevokedAtUtc != null;

    public void Revocar(string? reemplazadoPorTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = reemplazadoPorTokenHash;
    }
}
