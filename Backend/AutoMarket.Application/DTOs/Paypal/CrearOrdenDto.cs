using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Paypal;

public class CrearOrdenDto
{
    [Required(ErrorMessage = "El nombre del plan es requerido.")]
    [RegularExpression("^(Gratis|Basico|Pro|Elite)$", ErrorMessage = "El plan no es válido.")]
    public string NombrePlan { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ciclo es requerido.")]
    [RegularExpression("^(Mensual|Trimestral|Anual)$", ErrorMessage = "El ciclo no es válido.")]
    public string Ciclo { get; set; } = string.Empty;
}