namespace AutoMarket.Application.DTOs;

public class AnuncioListadoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }

    public string NombreAnuncio { get; set; } = string.Empty;

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

    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public List<string> Fotos { get; set; } = new();

    public string BadgeSuscripcion { get; set; } = "Gratis";
}