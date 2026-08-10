namespace AutoMarket.Application.DTOs.Usuario;

public class SolicitarCambioEmailDto
{
    public string PasswordActual { get; set; } = null!;
    public string NuevoEmail { get; set; } = null!;
}