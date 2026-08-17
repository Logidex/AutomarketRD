namespace AutoMarket.Application.DTOs.Dealer;

public class AgenciaListadoDto
{
    public int Id { get; set; }
    public string NombreAgencia { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string Ubicacion { get; set; } = null!;
    public string TelefonoAgencia { get; set; } = null!;
    public string? WhatsApp { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    // true si tiene correo confirmado y suscripción pagada (no Gratis).
    public bool EsDealerVerificado { get; set; }

    // Nivel de suscripción vigente (Gratis/Basico/Pro/Elite), si existe.
    public string? PlanNivel { get; set; }

    // Anuncios publicados vigentes de la agencia.
    public int CantidadAnuncios { get; set; }
}