using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Cupones;

public class AplicarCuponDto
{
    [Required(ErrorMessage = "El código del cupón es obligatorio.")]
    public string Codigo { get; set; } = string.Empty;
}
