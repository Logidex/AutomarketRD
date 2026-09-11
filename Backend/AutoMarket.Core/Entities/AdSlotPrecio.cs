namespace AutoMarket.Core.Entities;

/// <summary>
/// Precio configurado para un slot publicitario según la duración contratada.
/// Incluye descuento opcional para dealers con plan Pro o Elite.
/// </summary>
public class AdSlotPrecio
{
    public int Id { get; set; }

    public int AdSlotId { get; set; }

    public int DuracionDias { get; set; }

    public decimal Precio { get; set; }

    public decimal DescuentoProElitePorcentaje { get; set; }

    public bool Activo { get; set; } = true;

    public AdSlot AdSlot { get; set; } = null!;
}
