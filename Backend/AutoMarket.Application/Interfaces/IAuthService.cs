using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync(RegistroDto dto);
    Task<(bool Exito, string Mensaje, string? Token)> LoginAsync(LoginDto dto);
}