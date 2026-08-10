namespace AutoMarket.Application.DTOs.Usuario;

public class UsuarioCuentaDto
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmado { get; set; }
    public string? TelefonoPersonal { get; set; }
    public string Rol { get; set; } = string.Empty;
}