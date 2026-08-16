namespace AutoMarket.Application.DTOs.Auth;

using System.ComponentModel.DataAnnotations;

public class ConfirmarCorreoDto
{
    [Required(ErrorMessage = "El token de confirmación es obligatorio.")]
    public string Token { get; set; } = null!;
}