namespace AutoMarket.Application.DTOs.Auth;

using System.ComponentModel.DataAnnotations;

public class ReenviarConfirmacionDto
{
    [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
    public string Email { get; set; } = null!;
}