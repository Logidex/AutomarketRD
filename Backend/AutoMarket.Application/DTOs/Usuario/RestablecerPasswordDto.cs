namespace AutoMarket.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class RestablecerPasswordDto
{
    [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
    public string Email { get; set; } = null!;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    public string NuevaPassword { get; set; } = null!;

    public string Codigo { get; set; } = null!;
}