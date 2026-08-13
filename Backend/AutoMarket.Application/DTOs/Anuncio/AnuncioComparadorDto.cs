namespace AutoMarket.Application.DTOs.Anuncio;

public class AnuncioComparadorDto
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string TipoVehiculo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "DOP";
    public decimal? PrecioAnterior { get; set; }
    public bool EnOferta { get; set; }
    public int Kilometraje { get; set; }
    public string Condicion { get; set; } = string.Empty;
    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;
    public string Motor { get; set; } = string.Empty;
    public string Traccion { get; set; } = string.Empty;
    public string? ColorExterior { get; set; }
    public string? ColorInterior { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string? FotoPrincipal { get; set; }
}
