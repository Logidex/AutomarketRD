namespace AutoMarket.Application.DTOs.Lead;

public class LeadAnuncioResumenDto
{
    public int Id { get; set; }
    public string NombreAnuncio { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
}

public class LeadDealerDto
{
    public int Id { get; set; }
    public int AnuncioId { get; set; }
    public LeadAnuncioResumenDto? Anuncio { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string EmailContacto { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Canal { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
    public bool Leido { get; set; }
}