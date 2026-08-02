namespace AutoMarket.Application.DTOs;

public class RegistroDto
{
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Rol { get; set; } = null!; // "Comprador", "Vendedor" o "Dealer"
    public string? TelefonoPersonal { get; set; }

    // ==========================================
    // CAMPOS EXCLUSIVOS PARA DEALERS (Opcionales)
    // ==========================================
    public string? NombreAgencia { get; set; }
    public string? AgenciaRNC { get; set; }
    public string UbicacionAgencia { get; set; } = null!;
    public string TelefonoAgencia { get; set; } = null!;
}