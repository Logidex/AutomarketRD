namespace AutoMarket.Application.DTOs.Usuario;

public class RestablecerPasswordDto
{
    public string Email { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string NuevaPassword { get; set; } = null!;
}