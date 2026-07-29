using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteEmailAsync(string email)
    {
        return await _context.Usuarios
        .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<Usuario> CrearUsuarioAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        var usuarioEncontrado = await _context.Usuarios
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        return usuarioEncontrado;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuarioEncontrado = await _context.Usuarios
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(u => u.UsuarioId == id);
        return usuarioEncontrado;
    }

    public async Task<Usuario?> ObtenerDealerConPerfilPorIdAsync(int usuarioId)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.PerfilDealer)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }
}