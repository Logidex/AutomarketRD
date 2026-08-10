namespace AutoMarket.Application.DTOs.Usuario;

public class ActualizarDatosDto
{
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string? TelefonoPersonal { get; set; }
}