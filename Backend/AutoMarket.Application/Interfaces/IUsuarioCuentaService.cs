using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IUsuarioCuentaService
{
    Task<UsuarioCuentaDto> ObtenerCuentaAsync(int usuarioId);
    Task<UsuarioCuentaDto> ActualizarDatosAsync(int usuarioId, ActualizarDatosDto dto);
    Task<LoginResultDto> AscenderRolAsync(int usuarioId, AscenderRolDto dto);
    Task<UsuarioCuentaDto> CambiarRolAdminAsync(int usuarioId, CambiarRolAdminDto dto);
    Task SolicitarCambioPasswordAsync(int usuarioId, SolicitarCambioPasswordDto dto);
    Task ConfirmarCambioPasswordAsync(int usuarioId, ConfirmarPasswordDto dto);
    Task SolicitarCambioEmailAsync(int usuarioId, SolicitarCambioEmailDto dto);
    Task ConfirmarCambioEmailAsync(int usuarioId, ConfirmarEmailDto dto);
}