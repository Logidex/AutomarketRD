using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Paypal;

public class CrearOrdenDto
{
    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "El nombre del plan es requerido.")]
    [StringLength(50, ErrorMessage = "El nombre del plan no puede exceder 50 caracteres.")]
    public string NombrePlan { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ciclo es requerido.")]
    [RegularExpression("^(Mensual|Trimestral|Anual)$", ErrorMessage = "El ciclo no es válido.")]
    public string Ciclo { get; set; } = string.Empty;
}