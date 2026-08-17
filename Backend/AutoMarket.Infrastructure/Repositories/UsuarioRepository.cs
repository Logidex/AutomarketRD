using AutoMarket.Application.Helpers;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
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

    public async Task<(
        IReadOnlyCollection<PerfilDealer> Agencias,
        IReadOnlyDictionary<int, int> AnunciosPorAgencia,
        int TotalRegistros
    )> BuscarAgenciasAsync(
        string? busqueda,
        bool? soloVerificadas,
        string? planNivel,
        int pagina,
        int cantidadPorPagina)
    {
        IQueryable<PerfilDealer> query = _context.PerfilesDealers
            .Include(p => p.Usuario)
            .Include(p => p.Suscripcion)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = NormalizadorTexto.Normalizar(busqueda);
            query = query.Where(p =>
                EF.Functions.ILike(
                    p.NombreAgencia,
                    $"%{termino}%"
                )
            );
        }

        if (soloVerificadas == true)
        {
            query = query.Where(p =>
                p.Usuario.EmailConfirmado &&
                p.Suscripcion != null &&
                p.Suscripcion.Nivel != PlanNivel.Gratis
            );
        }

        if (!string.IsNullOrWhiteSpace(planNivel) &&
            Enum.TryParse<PlanNivel>(planNivel, ignoreCase: true, out var nivel))
        {
            query = query.Where(p =>
                p.Suscripcion != null &&
                p.Suscripcion.Nivel == nivel
            );
        }

        int totalRegistros = await query.CountAsync();

        int paginaSegura = pagina <= 0 ? 1 : pagina;
        int cantidadSegura = cantidadPorPagina <= 0 ? 12 : cantidadPorPagina;

        var agencias = await query
            .OrderByDescending(p =>
                p.Usuario.EmailConfirmado &&
                p.Suscripcion != null &&
                p.Suscripcion.Nivel != PlanNivel.Gratis)
            .ThenByDescending(p =>
                p.Suscripcion != null ? p.Suscripcion.Nivel : PlanNivel.Gratis)
            .ThenBy(p => p.NombreAgencia)
            .Skip((paginaSegura - 1) * cantidadSegura)
            .Take(cantidadSegura)
            .ToListAsync();

        var anunciosPorAgencia = await _context.Anuncios
            .Where(a =>
                a.Estado == "Publicado" &&
                (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow))
            .GroupBy(a => a.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Cantidad);

        return (agencias, anunciosPorAgencia, totalRegistros);
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