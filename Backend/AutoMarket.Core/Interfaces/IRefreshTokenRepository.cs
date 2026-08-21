using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task AgregarAsync(RefreshToken token);
    Task<RefreshToken?> ObtenerPorHashAsync(string tokenHash);
    Task RevocarActivosDeUsuarioAsync(int usuarioId);
    Task GuardarCambiosAsync();
}
