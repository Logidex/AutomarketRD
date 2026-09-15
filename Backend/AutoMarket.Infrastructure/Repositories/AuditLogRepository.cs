using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<AuditLog> Items, int Total)> ObtenerPaginadosAsync(
        int pagina, int tamanoPagina, string? entidad = null, int? usuarioId = null)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(a => a.Entidad == entidad);

        if (usuarioId.HasValue)
            query = query.Where(a => a.UsuarioId == usuarioId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.FechaUtc)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }
}
