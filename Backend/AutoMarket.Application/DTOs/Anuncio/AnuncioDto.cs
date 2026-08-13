namespace AutoMarket.Application.DTOs;

public class AnuncioDto
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
    public string Moneda { get; set; } = "DOP";
    public decimal? PrecioAnterior { get; set; }
    public int Kilometraje { get; set; }

    public string Condicion { get; set; } = "Usado";

    public bool EnOferta { get; set; }

    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;

    public List<string> Accesorios { get; set; } = new();

    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public List<string> Fotos { get; set; } = new();

    // Datos de contacto del vendedor, expuestos solo en el detalle público.
    public string? NombreVendedor { get; set; }
    public string? WhatsAppContacto { get; set; }

    // true si el dueño del anuncio es cuenta Vendedor (particular);
    // false si es un Dealer (agencia).
    public bool EsVendedorParticular { get; set; }
}