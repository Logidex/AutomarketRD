using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly ApplicationDbContext _context;

    public FavoritoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(UsuarioFavorito favorito)
    {
        await _context.Favoritos.AddAsync(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(UsuarioFavorito favorito)
    {
        _context.Favoritos.Remove(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task<UsuarioFavorito?> ObtenerAsync(int usuarioId, int anuncioId)
    {
        return await _context.Favoritos
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.AnuncioId == anuncioId);
    }

    public async Task<IEnumerable<Anuncio>> ObtenerAnunciosFavoritosAsync(int usuarioId)
    {
        return await _context.Favoritos
            .Where(f => f.UsuarioId == usuarioId)
            .Include(f => f.Anuncio) 
            .Select(f => f.Anuncio)
            .ToListAsync();
    }
}