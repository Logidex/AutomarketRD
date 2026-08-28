using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class ReporteAnuncioRepository : IReporteAnuncioRepository
{
    private readonly ApplicationDbContext _context;

    public ReporteAnuncioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(ReporteAnuncio reporte)
    {
        await _context.ReportesAnuncios.AddAsync(reporte);
    }

    public Task<ReporteAnuncio?> ObtenerPorIdAsync(int id)
    {
        return _context.ReportesAnuncios.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IReadOnlyCollection<ReporteAnuncio>> ListarPorEstadoAsync(ReporteEstado estado)
    {
        return await _context.ReportesAnuncios
            .Include(r => r.Anuncio)
            .Where(r => r.Estado == estado)
            .OrderByDescending(r => r.FechaCreacionUtc)
            .Take(200)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> ContarPendientesAsync()
    {
        return _context.ReportesAnuncios
            .CountAsync(r => r.Estado == ReporteEstado.Pendiente);
    }

    public Task GuardarCambiosAsync()
    {
        return _context.SaveChangesAsync();
    }
}
