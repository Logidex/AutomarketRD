using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync(RegistroDto dto);
    Task<LoginResultDto> LoginAsync(LoginDto dto);

    /// <summary>
    /// Renueva la sesión a partir de un refresh token crudo. Rota el token
    /// (revoca el viejo, emite uno nuevo) y devuelve nuevo JWT. Si el token
    /// fue revocado (reuso = robo), revoca toda la familia del usuario.
    /// </summary>
    Task<LoginResultDto> RefrescarSesionAsync(string refreshTokenCrudo);

    /// <summary>Revoca el refresh token crudo indicado (logout).</summary>
    Task RevocarSesionAsync(string? refreshTokenCrudo);

    Task SolicitarRecuperacionAsync(string email);
    Task RestablecerPasswordAsync(RestablecerPasswordDto dto);
    Task ConfirmarCorreoAsync(string token);
    Task ReenviarConfirmacionCorreoAsync(string email);
}