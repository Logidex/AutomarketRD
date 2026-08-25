namespace AutoMarket.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class RegistroDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
    public string Apellido { get; set; } = null!;

    [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
    [StringLength(254, ErrorMessage = "El correo no puede tener más de 254 caracteres.")]
    public string Email { get; set; } = null!;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
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

    // ==========================================
    // PLAN INICIAL PARA DEALERS
    // Valores: "Gratis", "Basico", "Pro", "Elite".
    // La cuenta siempre se crea con el plan "Gratis";
    // si el dealer elige un plan de pago, el pago se
    // procesa después del registro (flujo PayPal).
    // ==========================================
    public string? PlanInicial { get; set; }

/// <summary>Consentimiento obligatorio de Términos y Condiciones y Política de Privacidad.</summary>
public bool AceptaTerminos { get; set; }
}