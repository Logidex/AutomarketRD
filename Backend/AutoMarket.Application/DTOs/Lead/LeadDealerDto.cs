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

// Resumen para el campanario de notificaciones del dealer/vendedor
public class LeadNoLeidosResumenDto
{
    public int CantidadNoLeidos { get; set; }
    public List<LeadDealerDto> Recientes { get; set; } = new();
}

// Historial de contactos del comprador: vehículos que contactó
public class LeadContactoUsuarioDto
{
    public int Id { get; set; }
    public int AnuncioId { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string? FotoPrincipal { get; set; }
    public string? NombreVendedor { get; set; }
    public bool EsVendedorParticular { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Canal { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
}