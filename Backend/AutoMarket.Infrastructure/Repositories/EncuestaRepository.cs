using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoMarket.Infrastructure.Repositories;

public class EncuestaRepository : IEncuestaRepository
{
    private readonly ApplicationDbContext _context;

    public EncuestaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Encuesta?> ObtenerActivaAsync()
    {
        return await _context.Encuestas
            .Include(e => e.Preguntas.OrderBy(p => p.Orden))
            .FirstOrDefaultAsync(e => e.Activa);
    }

    public async Task<Encuesta?> ObtenerPorIdAsync(int encuestaId)
    {
        return await _context.Encuestas
            .Include(e => e.Preguntas.OrderBy(p => p.Orden))
            .FirstOrDefaultAsync(e => e.Id == encuestaId);
    }

    public async Task<bool> YaRespondioAsync(int encuestaId, int usuarioId)
    {
        return await _context.EncuestasRespuestas
            .AnyAsync(r => r.EncuestaId == encuestaId && r.UsuarioId == usuarioId);
    }

    public async Task<List<EncuestaRespuesta>> ObtenerRespuestasAsync(int encuestaId)
    {
        return await _context.EncuestasRespuestas
            .Where(r => r.EncuestaId == encuestaId)
            .ToListAsync();
    }

    public async Task AgregarRespuestasAsync(IReadOnlyList<EncuestaRespuesta> respuestas)
    {
        if (respuestas.Count == 0) return;

        await _context.EncuestasRespuestas.AddRangeAsync(respuestas);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505")
        {
            // Índice único: el usuario ya respondió (carrera de doble envío).
            throw new BusinessRuleException("Ya respondiste esta encuesta. ¡Gracias!");
        }
    }

    public async Task<Encuesta> AgregarAsync(Encuesta encuesta)
    {
        await _context.Encuestas.AddAsync(encuesta);
        await _context.SaveChangesAsync();
        return encuesta;
    }
}
