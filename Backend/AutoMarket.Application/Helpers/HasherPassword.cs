namespace AutoMarket.Application.Helpers;

/// <summary>
/// Política central de hash de contraseñas (BCrypt). Work factor explícito
/// para no depender del default de la librería al actualizarla.
/// </summary>
public static class HasherPassword
{
    public const int WorkFactor = 12;

    public static string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public static bool Verificar(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
