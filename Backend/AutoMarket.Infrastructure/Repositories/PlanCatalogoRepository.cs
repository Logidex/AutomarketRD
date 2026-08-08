using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class PlanCatalogoRepository : IPlanCatalogoRepository
{
    private readonly ApplicationDbContext _context;

    public PlanCatalogoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlanCatalogo?> ObtenerPorIdAsync(int id)
    {
        return await _context.PlanesCatalogo.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlanCatalogo?> ObtenerPorNivelAsync(PlanNivel nivel)
    {
        return await _context.PlanesCatalogo.FirstOrDefaultAsync(p => p.Nivel == nivel);
    }

    public async Task<List<PlanCatalogo>> ObtenerTodosAsync(bool soloActivos = false)
    {
        var query = _context.PlanesCatalogo.AsQueryable();

        if (soloActivos)
            query = query.Where(p => p.Activo);

        return await query.OrderBy(p => p.Nivel).ToListAsync();
    }

    public async Task AgregarAsync(PlanCatalogo plan)
    {
        await _context.PlanesCatalogo.AddAsync(plan);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(PlanCatalogo plan)
    {
        _context.PlanesCatalogo.Update(plan);
        await _context.SaveChangesAsync();
    }
}