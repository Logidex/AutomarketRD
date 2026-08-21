using AutoMarket.Core.Entities;

namespace AutoMarket.Application.Interfaces;
public interface ITokenService
{
    string GenerarToken(Usuario usuario);

    /// <summary>Token de renovación aleatorio (crudo, solo para la cookie).</summary>
    string GenerarRefreshToken();

    /// <summary>Hash SHA-256 hex del refresh token (lo único que se persiste).</summary>
    string HashRefreshToken(string token);
}
