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

    public async Task<Usuario?> ObtenerPorEmailParaEscrituraAsync(string email)
    {
        var usuarioEncontrado = await _context.Usuarios
                                              .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        return usuarioEncontrado;
    }

    public async Task<Usuario?> ObtenerPorCodigoConfirmacionEmailAsync(string codigoHash)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.CodigoConfirmacionEmailHash == codigoHash);
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var usuarioEncontrado = await _context.Usuarios
                                              .FirstOrDefaultAsync(u => u.UsuarioId == id);
        return usuarioEncontrado;
    }

    public async Task<Usuario?>
    ObtenerDealerConPerfilPorIdAsync(int usuarioId)
    {
        return await _context.Usuarios
            .Include(u => u.PerfilDealer)
                .ThenInclude(p => p!.Suscripcion)
                    .ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(u =>
                u.UsuarioId == usuarioId
            );
    }

    public async Task EliminarPerfilDealerAsync(int usuarioId)
    {
        var perfil = await _context.PerfilesDealers
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

        if (perfil != null)
            _context.PerfilesDealers.Remove(perfil);
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

    public async Task<bool> ActualizarContrasenaAsync(string email, string nuevoPasswordHash)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        if (usuario == null)
            return false;

        usuario.CambiarPassword(nuevoPasswordHash);
        await _context.SaveChangesAsync();
        return true;
    }
}