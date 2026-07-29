using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResumenDto> ObtenerResumenAsync()
    {
        // Ejecutamos los conteos en paralelo para que sea súper rápido
        var tareas = new List<Task>
        {
            _context.Usuarios.CountAsync(),
            _context.Anuncios.CountAsync(),
            _context.Leads.CountAsync() 
        };

        return new DashboardResumenDto
        {
            TotalUsuarios = await _context.Usuarios.CountAsync(),
            TotalAnuncios = await _context.Anuncios.CountAsync(),
            TotalLeads = await _context.Leads.CountAsync()
        };
    }
}