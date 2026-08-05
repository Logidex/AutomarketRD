using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class AnuncioRepository : IAnuncioRepository
{
    private readonly ApplicationDbContext _context;

    public AnuncioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Anuncio anuncio)
    {
        await _context.Anuncios.AddAsync(anuncio);
    }

    public async Task<Anuncio?> ObtenerPorIdAsync(int Id)
    {
        var vehiculoEncontrado = await _context.Anuncios
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync(a => a.Id == Id);
        return vehiculoEncontrado;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Anuncio>> ObtenerTodosLosAnuncios()
    {
        return await _context.Anuncios
              .Include(a => a.Usuario)
                  .ThenInclude(u => u.PerfilDealer)
                      .ThenInclude(p => p!.Suscripcion)
              .Where(a => a.Estado == "Publicado")
              .AsNoTracking()
              .ToListAsync();
    }

    public async Task ActualizarAsync(Anuncio anuncio)
    {
        var entry = _context.Entry(anuncio);

        entry.Property("_fotos").IsModified = true;

        _context.Anuncios.Update(anuncio);
        await _context.SaveChangesAsync();

    }

    public async Task<(IEnumerable<Anuncio> Anuncios, int TotalRegistros)> BuscarPaginadoAsync(AnuncioQueryFilter filtro)
    {
        IQueryable<Anuncio> query = _context.Anuncios
        .Include(a => a.Usuario)
            .ThenInclude(u => u.PerfilDealer)
                .ThenInclude(p => p!.Suscripcion)
        .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Marca))
        {
            query = query.Where(a => a.Marca.ToLower().Contains(filtro.Marca.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Modelo))
        {
            query = query.Where(a => a.Modelo.ToLower().Contains(filtro.Modelo.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Transmision))
        {
            query = query.Where(a => a.Transmision.ToLower().Contains(filtro.Transmision.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Combustible))
        {
            query = query.Where(a => a.Combustible.ToLower().Contains(filtro.Combustible.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Ubicacion))
        {
            query = query.Where(a => a.Ubicacion.ToLower().Contains(filtro.Ubicacion.ToLower()));
        }

        // Para los rangos numéricos evaluamos si tienen valor (.HasValue)
        if (filtro.PrecioMinimo.HasValue)
        {
            query = query.Where(a => a.Precio >= filtro.PrecioMinimo.Value);
        }

        if (filtro.PrecioMaximo.HasValue)
        {
            query = query.Where(a => a.Precio <= filtro.PrecioMaximo.Value);
        }

        if (filtro.AnioDesde.HasValue)
        {
            query = query.Where(a => a.Anio >= filtro.AnioDesde.Value);
        }

        if (filtro.AnioHasta.HasValue)
        {
            query = query.Where(a => a.Anio <= filtro.AnioHasta.Value);
        }

        if (filtro.KilometrajeMaximo.HasValue)
        {
            query = query.Where(a => a.Kilometraje <= filtro.KilometrajeMaximo.Value);
        }

        if (filtro.UsuarioId.HasValue && filtro.UsuarioId.Value > 0)
        {
            query = query.Where(a => a.UsuarioId == filtro.UsuarioId.Value);
        }
        else
        {
            query = query.Where(a => a.Estado == "Publicado");
        }

        int totalRegistros = await query.CountAsync();

        var anuncios = await query
            .OrderByDescending(a => a.Id)
            .Skip((filtro.PaginaActual - 1) * filtro.CantidadPorPagina)
            .Take(filtro.CantidadPorPagina)
            .ToListAsync();

        return (anuncios, totalRegistros);
    }

    public async Task<int> ContarAnunciosPorUsuarioAsync(int usuarioId)
    {
        return await _context.Anuncios.CountAsync(a => a.UsuarioId == usuarioId);
    }

    public async Task<IEnumerable<Anuncio>> ObtenerTodosParaAdminAsync()
    {
        return await _context.Anuncios
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public void Eliminar(Anuncio anuncio)
    {
        _context.Anuncios.Remove(anuncio);
    }

    public async Task<IEnumerable<Anuncio>> ObtenerPorIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Anuncios
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();
    }

    public async Task<(IEnumerable<Anuncio> Anuncios, int Total)> ObtenerPaginadosAsync(int pagina, int tamanoPagina)
    {
        // 1. Armamos la consulta base (solo anuncios públicos)
        // AÑADIMOS EL WHERE AQUÍ PARA QUE SEA CIERTO
        var query = _context.Anuncios.Where(a => a.Estado == "Publicado").AsQueryable();

        // 2. Contamos cuántos hay en total (antes de paginar)
        var total = await query.CountAsync();

        // 3. Paginamos usando Skip y Take, ordenados por los más recientes
        var anuncios = await query
            .OrderByDescending(a => a.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (anuncios, total);
    }
}