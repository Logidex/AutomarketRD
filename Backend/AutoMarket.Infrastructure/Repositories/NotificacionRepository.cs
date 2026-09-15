using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly ApplicationDbContext _context;

    public NotificacionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Notificacion> Items, int Total)> ObtenerPorUsuarioAsync(
        int usuarioId, int pagina, int tamanoPagina)
    {
        var query = _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .AsNoTracking();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.FechaUtc)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> ContarNoLeidasAsync(int usuarioId)
    {
        return await _context.Notificaciones
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Leido);
    }

    public async Task MarcarComoLeidaAsync(int notificacionId, int usuarioId)
    {
        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);

        if (notificacion != null)
        {
            notificacion.Leido = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarcarTodasComoLeidasAsync(int usuarioId)
    {
        await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.Leido)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leido, true));
    }

    public async Task CrearAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();
    }
}
