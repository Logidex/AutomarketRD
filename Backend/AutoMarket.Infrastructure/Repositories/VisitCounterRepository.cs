using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class VisitCounterRepository : IVisitCounterRepository
{
    private readonly ApplicationDbContext _context;

    public VisitCounterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VisitCounter> ObtenerOCrearAsync()
    {
        var counter = await _context.VisitCounters.FindAsync(1);

        if (counter is null)
        {
            counter = new VisitCounter
            {
                Id = 1,
                TotalVisitas = 0,
                UltimaVisita = DateTime.UtcNow
            };
            _context.VisitCounters.Add(counter);
            await _context.SaveChangesAsync();
        }

        return counter;
    }

    public async Task IncrementarAsync()
    {
        await _context.VisitCounters
            .Where(c => c.Id == 1)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.TotalVisitas, c => c.TotalVisitas + 1)
                .SetProperty(c => c.UltimaVisita, DateTime.UtcNow));
    }
}
