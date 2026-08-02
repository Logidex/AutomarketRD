namespace AutoMarket.Application.DTOs.Anuncio;

public class AnuncioComparadorDto
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public decimal Precio { get; set; }
    public int Kilometraje { get; set; }
    public string Transmision { get; set; } = string.Empty;
    public string Combustible { get; set; } = string.Empty;
    public string? ColorExterior { get; set; }
    public string? FotoPrincipal { get; set; }
}