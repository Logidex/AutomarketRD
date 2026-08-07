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
        // Totales
        var totalUsuarios = await _context.Usuarios.CountAsync();
        var totalAnuncios = await _context.Anuncios.CountAsync();
        var totalLeads = await _context.Leads.CountAsync();

        // Anuncios por estado (usando tus strings)
        var anunciosActivos = await _context.Anuncios
            .CountAsync(a => a.Estado == "Publicado");

        var anunciosBorrador = await _context.Anuncios
            .CountAsync(a => a.Estado == "Borrador");

        var anunciosVendidos = await _context.Anuncios
            .CountAsync(a => a.Estado == "Vendido");

        var anunciosPausados = await _context.Anuncios
            .CountAsync(a => a.Estado == "Pausado");

        // Leads no leídos: por ahora 0 hasta que agregues un flag
        var leadsNoLeidos = 0;

        // Suscripción: tomamos la suscripción activa más reciente (simple)
        var suscripcion = await _context.SuscripcionDealers
            .OrderByDescending(s => s.FechaVencimientoUtc)
            .FirstOrDefaultAsync();

        var planActual = suscripcion?.Nivel.ToString() ?? "Básico";
        var diasRestantes = suscripcion is null
            ? 0
            : Math.Max(0, (suscripcion.FechaVencimientoUtc.Date - DateTime.UtcNow.Date).Days);

        // Anuncios más vistos (ahora usando la nueva propiedad Vistas)
        var anunciosMasVistos = await _context.Anuncios
            .OrderByDescending(a => a.Vistas)
            .ThenByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new AnuncioMasVistoDto
            {
                Id = a.Id,
                NombreAnuncio = a.NombreAnuncio,
                Vistas = a.Vistas
            })
            .ToListAsync();

        return new DashboardResumenDto
        {
            TotalUsuarios = totalUsuarios,
            TotalAnuncios = totalAnuncios,
            TotalLeads = totalLeads,
            AnunciosActivos = anunciosActivos,
            AnunciosBorrador = anunciosBorrador,
            AnunciosVendidos = anunciosVendidos,
            AnunciosPausados = anunciosPausados,
            LeadsNoLeidos = leadsNoLeidos,
            PlanActual = planActual,
            DiasRestantesSuscripcion = diasRestantes,
            AnunciosMasVistos = anunciosMasVistos
        };
    }
}