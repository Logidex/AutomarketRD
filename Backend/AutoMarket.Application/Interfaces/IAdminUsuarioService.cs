using AutoMarket.Application.DTOs.Admin;

namespace AutoMarket.Application.Interfaces;

public interface IAdminUsuarioService
{
    Task<IEnumerable<UsuarioAdminListDto>> ListarUsuariosAsync();
    Task<(IEnumerable<UsuarioAdminListDto> Items, int Total)> ListarUsuariosPaginadosAsync(int pagina, int tamanoPagina);
    Task<UsuarioAdminListDto?> SuspenderUsuarioAsync(int usuarioId, int adminId);
    Task<UsuarioAdminListDto?> ReactivarUsuarioAsync(int usuarioId);
    Task<UsuarioAdminListDto?> EliminarUsuarioAsync(int usuarioId, int adminId);
    Task<UsuarioAdminListDto?> CambiarRolAsync(int usuarioId, CambiarRolAdminDto dto);
}
