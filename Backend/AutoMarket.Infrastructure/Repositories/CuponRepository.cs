using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoMarket.Infrastructure.Repositories;

public class CuponRepository : ICuponRepository
{
    private readonly ApplicationDbContext _context;

    public CuponRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cupon?> ObtenerPorCodigoAsync(string codigo)
    {
        var normalizado = codigo?.Trim().ToUpperInvariant() ?? string.Empty;

        return await _context.Cupones
            .Include(c => c.Redenciones)
            .FirstOrDefaultAsync(c => c.Codigo == normalizado);
    }

    public async Task<Cupon> AgregarAsync(Cupon cupon)
    {
        await _context.Cupones.AddAsync(cupon);
        await _context.SaveChangesAsync();
        return cupon;
    }

    public async Task<bool> ExisteRedencionAsync(int cuponId, int perfilDealerId)
    {
        return await _context.RedencionesCupon
            .AnyAsync(r => r.CuponId == cuponId && r.PerfilDealerId == perfilDealerId);
    }

    public async Task GuardarCanjeAsync(Cupon cupon, CuponRedencion redencion)
    {
        _context.Cupones.Update(cupon);
        await _context.RedencionesCupon.AddAsync(redencion);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Dos canjes compitieron por el último uso disponible.
            throw new BusinessRuleException(
                "El cupón se agotó en este momento. ¡Quedaste fuera del cupo!");
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            // Índice único (CuponId, PerfilDealerId): canje duplicado por carrera.
            throw new BusinessRuleException("Ya canjeaste este cupón con tu cuenta.");
        }
    }

    private static bool EsViolacionDeUnicidad(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException pg
            && pg.SqlState == "23505";
    }
}
