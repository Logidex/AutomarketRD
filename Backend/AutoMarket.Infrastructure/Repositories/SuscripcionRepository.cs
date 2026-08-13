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

    public async Task AgregarPagoAsync(PagoSuscripcion pago)
    {
        await _context.PagosSuscripcion.AddAsync(pago);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PagoSuscripcion>> ObtenerHistorialPagosAsync(int perfilDealerId)
    {
        return await _context.PagosSuscripcion
            .Where(p => p.PerfilDealerId == perfilDealerId)
            .OrderByDescending(p => p.FechaUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PagoSuscripcion>> ObtenerTodosLosPagosAsync()
    {
        return await _context.PagosSuscripcion
            .Include(p => p.PerfilDealer)
            .ThenInclude(pd => pd.Usuario)
            .OrderByDescending(p => p.FechaUtc)
            .ToListAsync();
    }

    public async Task<PagoSuscripcion?> ObtenerPagoPorIdAsync(int pagoId)
    {
        return await _context.PagosSuscripcion
            .Include(p => p.PerfilDealer)
            .ThenInclude(pd => pd.Usuario)
            .FirstOrDefaultAsync(p => p.Id == pagoId);
    }

    public async Task ActualizarPagoAsync(PagoSuscripcion pago)
    {
        _context.PagosSuscripcion.Update(pago);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistePagoPorEventoAsync(string eventoId)
    {
        return await _context.PagosSuscripcion
            .AnyAsync(p => p.EventoIdPayPal == eventoId);
    }

    public async Task<bool> ExistePagoPorOrdenAsync(string orderId)
    {
        return await _context.PagosSuscripcion
            .AnyAsync(p => p.OrderIdPayPal == orderId);
    }
}