using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.AdSlots;

public class AdSlotDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public UbicacionAdSlot Ubicacion { get; set; }
    public int AnchoPx { get; set; }
    public int AltoPx { get; set; }
    public int IntervaloRotacionSeg { get; set; }
    public int MaxAnunciosSimultaneos { get; set; }
    public bool Activo { get; set; }
    public int Orden { get; set; }
    public List<AdSlotPrecioDto> Precios { get; set; } = new();
    public int AnunciosActivosCount { get; set; }
}

public class AdSlotPrecioDto
{
    public int Id { get; set; }
    public int DuracionDias { get; set; }
    public decimal Precio { get; set; }
    public decimal DescuentoProElitePorcentaje { get; set; }
    public bool Activo { get; set; }
}

public class AdSlotPublicoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public UbicacionAdSlot Ubicacion { get; set; }
    public int AnchoPx { get; set; }
    public int AltoPx { get; set; }
    public int IntervaloRotacionSeg { get; set; }
    public List<AdSlotPrecioDto> Precios { get; set; } = new();
    public List<AdSlotAnuncioPublicoDto> Anuncios { get; set; } = new();
}

public class AdSlotAnuncioPublicoDto
{
    public int Id { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string? Enlace { get; set; }
    public string? Titulo { get; set; }
    public string NombreDealer { get; set; } = string.Empty;
    public int Prioridad { get; set; }
}

public class CrearAdSlotAnuncioDto
{
    public int AdSlotId { get; set; }
    public int DuracionDias { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public string? Enlace { get; set; }
    public string? Titulo { get; set; }
}

public class CrearAdSlotAdminDto
{
    public string Titulo { get; set; } = string.Empty;
    public UbicacionAdSlot Ubicacion { get; set; }
    public int AnchoPx { get; set; }
    public int AltoPx { get; set; }
    public int IntervaloRotacionSeg { get; set; } = 5;
    public int MaxAnunciosSimultaneos { get; set; } = 3;
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
    public List<CrearAdSlotPrecioDto> Precios { get; set; } = new();
}

public class ActualizarAdSlotAdminDto
{
    public string? Titulo { get; set; }
    public UbicacionAdSlot? Ubicacion { get; set; }
    public int? AnchoPx { get; set; }
    public int? AltoPx { get; set; }
    public int? IntervaloRotacionSeg { get; set; }
    public int? MaxAnunciosSimultaneos { get; set; }
    public bool? Activo { get; set; }
    public int? Orden { get; set; }
}

public class CrearAdSlotPrecioDto
{
    public int DuracionDias { get; set; }
    public decimal Precio { get; set; }
    public decimal DescuentoProElitePorcentaje { get; set; }
}

public class AdSlotAnuncioAdminDto
{
    public int Id { get; set; }
    public int AdSlotId { get; set; }
    public string TituloSlot { get; set; } = string.Empty;
    public UbicacionAdSlot Ubicacion { get; set; }
    public int PerfilDealerId { get; set; }
    public string NombreDealer { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public string? Enlace { get; set; }
    public string? Titulo { get; set; }
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public EstadoAdSlot Estado { get; set; }
    public decimal MontoPagado { get; set; }
    public int Impresiones { get; set; }
    public int Clicks { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}

public class AdSlotStatsDto
{
    public int TotalImpresiones { get; set; }
    public int TotalClicks { get; set; }
    public double CtrPromedio { get; set; }
    public List<AdSlotAnuncioStatsDto> TopAnuncios { get; set; } = new();
}

public class AdSlotAnuncioStatsDto
{
    public int Id { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string TituloSlot { get; set; } = string.Empty;
    public int Impresiones { get; set; }
    public int Clicks { get; set; }
    public double Ctr { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public bool EstaVigente { get; set; }
}
