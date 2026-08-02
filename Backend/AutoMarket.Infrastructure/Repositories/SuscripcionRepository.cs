using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class SuscripcionRepository : ISuscripcionRepository
{
    private readonly ApplicationDbContext _context;

    public SuscripcionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SuscripcionDealer?> ObtenerPorDealerIdAsync(int perfilDealerId)
    {
        return await _context.SuscripcionDealers
            .FirstOrDefaultAsync(s => s.PerfilDealerId == perfilDealerId);
    }

    public async Task AgregarAsync(SuscripcionDealer suscripcion)
    {
        await _context.SuscripcionDealers.AddAsync(suscripcion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(SuscripcionDealer suscripcion)
    {
        _context.SuscripcionDealers.Update(suscripcion);
        await _context.SaveChangesAsync();
    }
}