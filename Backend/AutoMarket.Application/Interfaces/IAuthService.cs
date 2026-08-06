using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync(RegistroDto dto);
    Task<LoginResultDto> LoginAsync(LoginDto dto);
}