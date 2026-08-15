using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<Ticket?> ObtenerPorIdConMensajesAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Usuario)
                .ThenInclude(u => u.PerfilDealer)
            .Include(t => t.Mensajes)
                .ThenInclude(m => m.Autor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IReadOnlyCollection<Ticket>> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.Tickets
            .Include(t => t.Usuario)
                .ThenInclude(u => u.PerfilDealer)
            .Include(t => t.Mensajes)
            .Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.FechaActualizacionUtc)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Ticket>> ObtenerTodosAdminAsync()
    {
        return await _context.Tickets
            .Include(t => t.Usuario)
                .ThenInclude(u => u.PerfilDealer)
            .Include(t => t.Mensajes)
            .OrderByDescending(t => t.FechaActualizacionUtc)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> ContarAbiertosAsync()
    {
        return await _context.Tickets
            .CountAsync(t => t.Estado == TicketEstado.Abierto || t.Estado == TicketEstado.EnProceso);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
