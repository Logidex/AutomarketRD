namespace AutoMarket.Application.DTOs.Usuario;

public class CambiarPasswordDto
{
    public string PasswordActual { get; set; } = null!;
    public string NuevaPassword { get; set; } = null!;
}