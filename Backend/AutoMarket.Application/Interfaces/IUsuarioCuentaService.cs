using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IUsuarioCuentaService
{
    Task<UsuarioCuentaDto> ObtenerCuentaAsync(int usuarioId);
    Task<UsuarioCuentaDto> ActualizarDatosAsync(int usuarioId, ActualizarDatosDto dto);
    Task CambiarPasswordAsync(int usuarioId, CambiarPasswordDto dto);
    Task SolicitarCambioEmailAsync(int usuarioId, SolicitarCambioEmailDto dto);
    Task ConfirmarCambioEmailAsync(int usuarioId, ConfirmarEmailDto dto);
}