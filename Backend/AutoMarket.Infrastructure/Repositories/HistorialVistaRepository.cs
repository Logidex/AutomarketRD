using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class HistorialVistaRepository : IHistorialVistaRepository
{
    private readonly ApplicationDbContext _context;

    public HistorialVistaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HistorialVista?> ObtenerAsync(int usuarioId, int anuncioId)
    {
        return await _context.HistorialVistas
            .FirstOrDefaultAsync(h => h.UsuarioId == usuarioId && h.AnuncioId == anuncioId);
    }

    public async Task AgregarAsync(HistorialVista historial)
    {
        await _context.HistorialVistas.AddAsync(historial);
        await _context.SaveChangesAsync();
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<HistorialVista>> ObtenerRecientesAsync(int usuarioId, int cantidad)
    {
        return await _context.HistorialVistas
            .Where(h => h.UsuarioId == usuarioId)
            .OrderByDescending(h => h.VistoEnUtc)
            .Take(cantidad)
            .Include(h => h.Anuncio)
            .ToListAsync();
    }
}