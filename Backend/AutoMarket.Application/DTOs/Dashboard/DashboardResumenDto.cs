using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Admin;

public class DashboardResumenDto
{
    // Ya tenías estos
    public int TotalUsuarios { get; set; }
    public int TotalAnuncios { get; set; }
    public int TotalLeads { get; set; }

    // Nuevos: anuncios por estado
    public int AnunciosActivos { get; set; }
    public int AnunciosBorrador { get; set; }
    public int AnunciosVendidos { get; set; }
    public int AnunciosPausados { get; set; }

    // Nuevos: leads
    public int LeadsNoLeidos { get; set; }

    // Nuevos: suscripción
    public string PlanActual { get; set; } = "N/A";
    public int DiasRestantesSuscripcion { get; set; }

    // Nuevos: anuncios más vistos
    public List<AnuncioMasVistoDto> AnunciosMasVistos { get; set; } = new();
}

public class AnuncioMasVistoDto
{
    public int Id { get; set; }
    public string NombreAnuncio { get; set; } = string.Empty;
    public int Vistas { get; set; }
}

public class RenovarSuscripcionDto
{
    public DateTime NuevaFechaVencimiento { get; set; }
}

public class CambiarPlanAdminDto
{
    public PlanNivel NuevoNivel { get; set; }
}