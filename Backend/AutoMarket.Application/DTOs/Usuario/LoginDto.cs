namespace AutoMarket.Application.DTOs.Usuario
{
using System.ComponentModel.DataAnnotations;
public class LoginDto
{
    [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
    public string Email { get; set; } = null!;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = null!;
}
}