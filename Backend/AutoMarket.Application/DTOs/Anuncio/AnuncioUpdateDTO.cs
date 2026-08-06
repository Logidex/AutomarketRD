namespace AutoMarket.Application.DTOs;

public class AnuncioUpdateDto
{
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public string TipoVehiculo { get; set; } = string.Empty;
    public string Motor { get; set; } = string.Empty;
    public string Traccion { get; set; } = string.Empty;

    public string ColorExterior { get; set; } = string.Empty;
    public string ColorInterior { get; set; } = string.Empty;

    public int Anio { get; set; }
    public decimal Precio { get; set; }
    public int Kilometraje { get; set; }

    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;

    public List<string> Accesorios { get; set; } = new();

    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}