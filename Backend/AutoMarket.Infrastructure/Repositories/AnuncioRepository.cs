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

    public async Task<Anuncio?> ObtenerPorIdAsync(int id)
    {
        return await _context.Anuncios
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Anuncio>>
        ObtenerTodosLosAnuncios()
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

    public async Task<(
        IEnumerable<Anuncio> Anuncios,
        int TotalRegistros
    )> BuscarPaginadoAsync(
        AnuncioQueryFilter filtro)
    {
        IQueryable<Anuncio> query =
            _context.Anuncios
                .Include(a => a.Usuario)
                    .ThenInclude(u => u.PerfilDealer)
                        .ThenInclude(p => p!.Suscripcion)
                .AsNoTracking();

        /*
         * Si UsuarioId está presente, se utiliza para consultar
         * los anuncios privados del usuario, incluyendo borradores.
         *
         * Si no está presente, solamente se muestran anuncios publicados.
         */
        if (
            filtro.UsuarioId.HasValue &&
            filtro.UsuarioId.Value > 0
        )
        {
            query = query.Where(a =>
                a.UsuarioId == filtro.UsuarioId.Value
            );
        }
        else
        {
            query = query.Where(a =>
                a.Estado == "Publicado"
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Marca))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Marca,
                    $"%{filtro.Marca}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Modelo))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Modelo,
                    $"%{filtro.Modelo}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Version))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Version,
                    $"%{filtro.Version}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.TipoVehiculo))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.TipoVehiculo,
                    $"%{filtro.TipoVehiculo}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Motor))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Motor,
                    $"%{filtro.Motor}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Traccion))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Traccion,
                    $"%{filtro.Traccion}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.ColorExterior))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.ColorExterior,
                    $"%{filtro.ColorExterior}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.ColorInterior))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.ColorInterior,
                    $"%{filtro.ColorInterior}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Transmision))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Transmision,
                    $"%{filtro.Transmision}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Combustible))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Combustible,
                    $"%{filtro.Combustible}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Ubicacion))
        {
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Ubicacion,
                    $"%{filtro.Ubicacion}%"
                )
            );
        }

        if (filtro.PrecioMinimo.HasValue)
        {
            query = query.Where(a =>
                a.Precio >= filtro.PrecioMinimo.Value
            );
        }

        if (filtro.PrecioMaximo.HasValue)
        {
            query = query.Where(a =>
                a.Precio <= filtro.PrecioMaximo.Value
            );
        }

        if (filtro.AnioDesde.HasValue)
        {
            query = query.Where(a =>
                a.Anio >= filtro.AnioDesde.Value
            );
        }

        if (filtro.AnioHasta.HasValue)
        {
            query = query.Where(a =>
                a.Anio <= filtro.AnioHasta.Value
            );
        }

        if (filtro.KilometrajeMaximo.HasValue)
        {
            query = query.Where(a =>
                a.Kilometraje <=
                filtro.KilometrajeMaximo.Value
            );
        }

        int totalRegistros = await query.CountAsync();

        int pagina = filtro.PaginaActual <= 0
            ? 1
            : filtro.PaginaActual;

        int cantidadPorPagina =
            filtro.CantidadPorPagina <= 0
                ? 10
                : filtro.CantidadPorPagina;

        var anuncios = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pagina - 1) * cantidadPorPagina)
            .Take(cantidadPorPagina)
            .ToListAsync();

        return (
            anuncios,
            totalRegistros
        );
    }

    public async Task<int> ContarAnunciosPorUsuarioAsync(
        int usuarioId)
    {
        return await _context.Anuncios
            .CountAsync(a =>
                a.UsuarioId == usuarioId
            );
    }

    public async Task<IEnumerable<Anuncio>>
        ObtenerTodosParaAdminAsync()
    {
        return await _context.Anuncios
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public void Eliminar(Anuncio anuncio)
    {
        _context.Anuncios.Remove(anuncio);
    }

    public async Task<IEnumerable<Anuncio>> ObtenerPorIdsAsync(
        IEnumerable<int> ids)
    {
        return await _context.Anuncios
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();
    }

    public async Task<(
        IEnumerable<Anuncio> Anuncios,
        int Total
    )> ObtenerPaginadosAsync(
        int pagina,
        int tamanoPagina)
    {
        var query = _context.Anuncios
            .Where(a => a.Estado == "Publicado")
            .AsNoTracking();

        int total = await query.CountAsync();

        var anuncios = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (
            anuncios,
            total
        );
    }
}