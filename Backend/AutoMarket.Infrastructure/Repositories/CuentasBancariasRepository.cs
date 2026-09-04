using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class CuentasBancariasRepository : ICuentasBancariasRepository
{
    private readonly ApplicationDbContext _context;

    public CuentasBancariasRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CuentaBancaria>> ObtenerActivasAsync()
    {
        return await _context.CuentasBancarias
            .Where(c => c.Activa)
            .OrderBy(c => c.Banco)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CuentaBancaria>> ObtenerTodasAsync()
    {
        return await _context.CuentasBancarias
            .OrderBy(c => c.Banco)
            .ToListAsync();
    }

    public async Task<CuentaBancaria?> ObtenerPorIdAsync(int id)
    {
        return await _context.CuentasBancarias.FindAsync(id);
    }

    public async Task AgregarAsync(CuentaBancaria cuenta)
    {
        await _context.CuentasBancarias.AddAsync(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(CuentaBancaria cuenta)
    {
        _context.CuentasBancarias.Update(cuenta);
        await _context.SaveChangesAsync();
    }
}
