using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Usuario;

// Ascenso de rol de la cuenta (Comprador → Vendedor / Dealer, o Vendedor → Dealer)
public class AscenderRolDto
{
    // Valores: "Vendedor" o "Dealer"
    [Required(ErrorMessage = "Debes indicar el rol al que deseas ascender.")]
    public string NuevoRol { get; set; } = null!;

    // ==========================================
    // CAMPOS EXCLUSIVOS PARA ASCENDER A DEALER
    // ==========================================
    public string? NombreAgencia { get; set; }
    public string? AgenciaRNC { get; set; }
    public string? UbicacionAgencia { get; set; }
    public string? TelefonoAgencia { get; set; }
}