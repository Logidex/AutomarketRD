using AutoMarket.Core.Entities.Enums;
namespace AutoMarket.Application.DTOs.Admin;

public class DashboardResumenDto
{
    public int TotalUsuarios { get; set; }
    public int TotalAnuncios { get; set; }
    public int TotalLeads { get; set; }
}

public class RenovarSuscripcionDto
{
    public DateTime NuevaFechaVencimiento { get; set; }
}

public class CambiarPlanAdminDto
{
    public PlanNivel NuevoNivel { get; set; }
}