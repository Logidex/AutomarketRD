using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Suscripcion;

public class SuscripcionDealerDto
{
    public int PerfilDealerId { get; set; }
    public PlanNivel Nivel { get; set; }
    public CicloFacturacion Ciclo { get; set; }
    public EstadoSuscripcion Estado { get; set; }
    public int LimiteAnuncios { get; set; }
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaVencimientoUtc { get; set; }
    public int DiasRestantes { get; set; }
    public bool Activa { get; set; }
}