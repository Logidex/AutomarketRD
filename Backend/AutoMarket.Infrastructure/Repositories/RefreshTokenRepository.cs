using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }

    public Task<RefreshToken?> ObtenerPorHashAsync(string tokenHash)
    {
        return _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task RevocarActivosDeUsuarioAsync(int usuarioId)
    {
        var ahora = DateTime.UtcNow;

        var activos = await _context.RefreshTokens
            .Where(t => t.UsuarioId == usuarioId &&
                        t.RevokedAtUtc == null &&
                        t.ExpiresAtUtc > ahora)
            .ToListAsync();

        foreach (var token in activos)
        {
            token.Revocar();
        }
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }
}
