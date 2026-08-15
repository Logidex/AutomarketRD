using AutoMarket.Application.Helpers;
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
            .Where(a => a.Estado == "Publicado" &&
                        (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task ActualizarAsync(Anuncio anuncio)
    {
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
         * VendedorId es un filtro público: devuelve únicamente los anuncios
         * Publicado de ese vendedor, para la página pública del vendedor.
         *
         * Si ninguno está presente, solamente se muestran anuncios publicados.
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
        else if (
            filtro.VendedorId.HasValue &&
            filtro.VendedorId.Value > 0
        )
        {
            var vendedorId = filtro.VendedorId.Value;
            query = query.Where(a =>
                a.UsuarioId == vendedorId &&
                a.Estado == "Publicado" &&
                (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow)
            );
        }
        else
        {
            query = query.Where(a =>
                a.Estado == "Publicado" &&
                (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow)
            );
        }

        // Excluye los destacados vigentes para no duplicarlos cuando ya se
        // muestran en una sección aparte (Home).
        if (filtro.ExcluirDestacadosVigentes)
        {
            query = query.Where(a =>
                !(a.EsDestacado &&
                  a.FechaDestacadoHasta.HasValue &&
                  a.FechaDestacadoHasta.Value > DateTime.UtcNow)
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Marca))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Marca);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Marca,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Modelo))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Modelo);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Modelo,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Version))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Version);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Version,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.TipoVehiculo))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.TipoVehiculo);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.TipoVehiculo,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Motor))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Motor);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Motor,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Traccion))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Traccion);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Traccion,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.ColorExterior))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.ColorExterior);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.ColorExterior,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.ColorInterior))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.ColorInterior);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.ColorInterior,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Transmision))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Transmision);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Transmision,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Combustible))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Combustible);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Combustible,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Ubicacion))
        {
            var termino = NormalizadorTexto.Normalizar(filtro.Ubicacion);
            query = query.Where(a =>
                EF.Functions.ILike(
                    a.Ubicacion,
                    $"%{termino}%"
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filtro.Condicion))
        {
            var condicion = NormalizadorTexto.Normalizar(filtro.Condicion);
            query = query.Where(a =>
                EF.Functions.ILike(a.Condicion, condicion)
            );
        }

        if (filtro.EnOferta == true)
        {
            query = query.Where(a =>
                a.PrecioAnterior.HasValue &&
                a.PrecioAnterior.Value > a.Precio
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

        if (!string.IsNullOrWhiteSpace(filtro.Moneda))
        {
            var moneda = NormalizadorTexto.Normalizar(filtro.Moneda);
            query = query.Where(a =>
                EF.Functions.ILike(a.Moneda, moneda)
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
        // El límite del plan aplica a la vitrina activa (publicado + pausado).
        // Borradores, vendidos, eliminados y anuncios vencidos no ocupan cupo.
        return await _context.Anuncios
            .CountAsync(a =>
                a.UsuarioId == usuarioId &&
                (a.Estado == "Publicado" || a.Estado == "Pausado") &&
                (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow)
            );
    }

    public async Task<int> ContarDestacadosPorUsuarioAsync(int usuarioId)
    {
        return await _context.Anuncios
            .CountAsync(a =>
                a.UsuarioId == usuarioId &&
                a.EsDestacado &&
                a.FechaDestacadoHasta.HasValue &&
                a.FechaDestacadoHasta.Value > DateTime.UtcNow &&
                a.Estado == "Publicado" &&
                (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow)
            );
    }

    public async Task<(IEnumerable<Anuncio> Anuncios, int Total)>
        ObtenerDestacadosPaginadosAsync(int pagina, int tamanoPagina)
    {
        var query = _context.Anuncios
            .Include(a => a.Usuario)
                .ThenInclude(u => u.PerfilDealer)
                    .ThenInclude(p => p!.Suscripcion)
            .Where(a =>
                a.Estado == "Publicado" &&
                a.EsDestacado &&
                a.FechaDestacadoHasta.HasValue &&
                a.FechaDestacadoHasta.Value > DateTime.UtcNow &&
                (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow))
            .AsNoTracking();

        int total = await query.CountAsync();

        var anuncios = await query
            .OrderByDescending(a => a.Usuario!.PerfilDealer!.Suscripcion!.Nivel)
            .ThenByDescending(a => a.FechaDestacadoHasta)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (anuncios, total);
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
            .Where(a => ids.Contains(a.Id) && a.Estado == "Publicado" &&
                        (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow))
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
            .Include(a => a.Usuario)
                .ThenInclude(u => u.PerfilDealer)
                    .ThenInclude(p => p!.Suscripcion)
            .Where(a => a.Estado == "Publicado" &&
                        (a.FechaVencimientoUtc == null || a.FechaVencimientoUtc > DateTime.UtcNow))
            .AsNoTracking();

        int total = await query.CountAsync();

        var anuncios = await query
            .OrderByDescending(a => a.Usuario!.PerfilDealer!.Suscripcion!.Nivel)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return (
            anuncios,
            total
        );
    }
}