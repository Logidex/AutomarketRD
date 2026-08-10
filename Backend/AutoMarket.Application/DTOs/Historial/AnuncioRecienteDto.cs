namespace AutoMarket.Application.DTOs.Historial;

public class AnuncioRecienteDto
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public decimal Precio { get; set; }
    public string? FotoPrincipal { get; set; }
    public DateTime VistoEnUtc { get; set; }
}