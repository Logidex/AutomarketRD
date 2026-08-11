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

    public async Task<IReadOnlyCollection<Lead>> ObtenerPorRemitenteIdAsync(int usuarioId)
    {
        return await _context.Leads
            .Include(l => l.Anuncio)
            .Include(l => l.Anuncio!.Usuario)
            .Include(l => l.Anuncio!.Usuario.PerfilDealer)
            .Where(l => l.UsuarioIdRemitente == usuarioId)
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

    public async Task<int> ContarNoLeidosPorUsuarioAsync(int usuarioId)
    {
        return await _context.Leads
            .CountAsync(l => l.Anuncio.UsuarioId == usuarioId && !l.Leido);
    }

    public async Task<IReadOnlyCollection<Lead>> ObtenerRecientesNoLeidosPorUsuarioAsync(int usuarioId, int cantidad)
    {
        return await _context.Leads
            .Include(l => l.Anuncio)
            .Where(l => l.Anuncio.UsuarioId == usuarioId && !l.Leido)
            .OrderByDescending(l => l.FechaCreacionUtc)
            .Take(cantidad)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> MarcarTodosLeidosAsync(int usuarioId)
    {
        var noLeidos = await _context.Leads
            .Where(l => l.Anuncio.UsuarioId == usuarioId && !l.Leido)
            .ToListAsync();

        foreach (var lead in noLeidos)
        {
            lead.MarcarComoLeido();
        }

        await _context.SaveChangesAsync();
        return noLeidos.Count;
    }
}