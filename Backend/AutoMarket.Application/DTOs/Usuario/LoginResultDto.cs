namespace AutoMarket.Application.DTOs.Auth;

public class LoginResultDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Token { get; set; }

    /// <summary>Refresh token crudo; solo viaja hacia la cookie HttpOnly.</summary>
    public string? RefreshToken { get; set; }

    public UsuarioAuthDto? Usuario { get; set; }
}