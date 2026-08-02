using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly ApplicationDbContext _context;

    public LeadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Lead lead)
    {
        await _context.Leads.AddAsync(lead);
        await _context.SaveChangesAsync();
    }

    public async Task<Lead?> ObtenerPorIdAsync(int id)
    {
        return await _context.Leads
            .Include(l => l.Anuncio)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IReadOnlyCollection<Lead>> ObtenerPorAnuncioIdAsync(int anuncioId)
    {
        return await _context.Leads
            .Where(l => l.AnuncioId == anuncioId)
            .OrderByDescending(l => l.FechaCreacionUtc)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Lead>> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.Leads
            .Include(l => l.Anuncio)
            .Where(l => l.Anuncio.UsuarioId == usuarioId)
            .OrderByDescending(l => l.FechaCreacionUtc)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> ContarLeadsPorUsuarioAsync(int usuarioId)
    {
        // Útil para el Dashboard rápido del Dealer
        return await _context.Leads
            .CountAsync(l => l.Anuncio.UsuarioId == usuarioId);
    }
}