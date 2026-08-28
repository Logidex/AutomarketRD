using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Cupones;

/// <summary>Resultado de un canje de cupón exitoso.</summary>
public class CuponAplicadoDto
{
    public PlanNivel Nivel { get; set; }
    public int Dias { get; set; }
    public DateTime FechaVencimientoUtc { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
