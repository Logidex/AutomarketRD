namespace AutoMarket.Application.DTOs.Usuario;

public class SolicitarCambioPasswordDto
{
    public string PasswordActual { get; set; } = null!;
    public string NuevaPassword { get; set; } = null!;
}