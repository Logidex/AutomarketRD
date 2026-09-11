using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Define un espacio publicitario disponible en el frontend.
/// Cada slot tiene una ubicación, dimensiones y precios asociados.
/// </summary>
public class AdSlot
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public UbicacionAdSlot Ubicacion { get; set; }

    public int AnchoPx { get; set; }

    public int AltoPx { get; set; }

    public int IntervaloRotacionSeg { get; set; } = 5;

    public int MaxAnunciosSimultaneos { get; set; } = 3;

    public bool Activo { get; set; } = true;

    public int Orden { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public ICollection<AdSlotPrecio> Precios { get; set; } = new List<AdSlotPrecio>();

    public ICollection<AdSlotAnuncio> Anuncios { get; set; } = new List<AdSlotAnuncio>();
}
