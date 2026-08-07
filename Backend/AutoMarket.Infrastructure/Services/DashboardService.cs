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

    // Versión global (AdminController)
    public async Task<DashboardResumenDto> ObtenerResumenAsync()
    {
        var totalUsuarios = await _context.Usuarios.CountAsync();
        var totalAnuncios = await _context.Anuncios.CountAsync();
        var totalLeads = await _context.Leads.CountAsync();

        var anunciosActivos = await _context.Anuncios
            .CountAsync(a => a.Estado == "Publicado");
        var anunciosBorrador = await _context.Anuncios
            .CountAsync(a => a.Estado == "Borrador");
        var anunciosVendidos = await _context.Anuncios
            .CountAsync(a => a.Estado == "Vendido");
        var anunciosPausados = await _context.Anuncios
            .CountAsync(a => a.Estado == "Pausado");

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
            LeadsNoLeidos = 0,
            PlanActual = "N/A",
            DiasRestantesSuscripcion = 0,
            AnunciosMasVistos = anunciosMasVistos
        };
    }

    // Versión filtrada (DealersController)
    public async Task<DashboardResumenDto> ObtenerResumenAsync(int dealerUsuarioId)
    {
        var anunciosDealer = _context.Anuncios
            .Where(a => a.UsuarioId == dealerUsuarioId);

        var totalAnuncios = await anunciosDealer.CountAsync();

        var anunciosActivos = await anunciosDealer
            .CountAsync(a => a.Estado == "Publicado");
        var anunciosBorrador = await anunciosDealer
            .CountAsync(a => a.Estado == "Borrador");
        var anunciosVendidos = await anunciosDealer
            .CountAsync(a => a.Estado == "Vendido");
        var anunciosPausados = await anunciosDealer
            .CountAsync(a => a.Estado == "Pausado");

        var leadsQuery = _context.Leads
            .Where(l => anunciosDealer.Select(a => a.Id).Contains(l.AnuncioId));

        var totalLeads = await leadsQuery.CountAsync();
        var leadsNoLeidos = 0;

        var perfilDealer = await _context.PerfilesDealers
            .Include(p => p.Suscripcion)
            .FirstOrDefaultAsync(p => p.UsuarioId == dealerUsuarioId);

        string planActual = "N/A";
        int diasRestantes = 0;

        if (perfilDealer?.Suscripcion != null)
        {
            planActual = perfilDealer.Suscripcion.Nivel.ToString();
            diasRestantes = Math.Max(
                0,
                (perfilDealer.Suscripcion.FechaVencimientoUtc.Date - DateTime.UtcNow.Date).Days
            );
        }

        var anunciosMasVistos = await anunciosDealer
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
            TotalUsuarios = 1,
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