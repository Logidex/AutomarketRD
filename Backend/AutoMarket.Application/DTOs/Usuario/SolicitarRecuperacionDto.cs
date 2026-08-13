namespace AutoMarket.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class SolicitarRecuperacionDto
{
    [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
    public string Email { get; set; } = null!;
}