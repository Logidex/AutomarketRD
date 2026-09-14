using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IUsuarioCuentaService _usuarioCuentaService;

    public AdminUsuarioService(
        IUsuarioRepository usuarioRepository,
        IAnuncioRepository anuncioRepository,
        IAlmacenadorArchivos almacenadorArchivos,
        IUsuarioCuentaService usuarioCuentaService)
    {
        _usuarioRepository = usuarioRepository;
        _anuncioRepository = anuncioRepository;
        _almacenadorArchivos = almacenadorArchivos;
        _usuarioCuentaService = usuarioCuentaService;
    }

    public async Task<IEnumerable<UsuarioAdminListDto>> ListarUsuariosAsync()
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync();
        return usuarios.Select(MapToDto);
    }

    public async Task<(IEnumerable<UsuarioAdminListDto> Items, int Total)> ListarUsuariosPaginadosAsync(int pagina, int tamanoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 100) tamanoPagina = 20;

        var (usuarios, total) = await _usuarioRepository.ObtenerPaginadosAsync(pagina, tamanoPagina);
        return (usuarios.Select(MapToDto), total);
    }

    public async Task<UsuarioAdminListDto?> SuspenderUsuarioAsync(int usuarioId, int adminId)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
        if (usuario is null) return null;

        if (string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No puedes suspender a otro administrador.");

        if (!usuario.IsActivo)
            throw new InvalidOperationException("El usuario ya se encuentra suspendido.");

        usuario.Suspender();
        await _usuarioRepository.GuardarCambiosAsync();

        return MapToDto(usuario);
    }

    public async Task<UsuarioAdminListDto?> ReactivarUsuarioAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
        if (usuario is null) return null;

        if (string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No puedes modificar a otro administrador.");

        if (usuario.IsActivo)
            throw new InvalidOperationException("El usuario ya se encuentra activo.");

        usuario.Reactivar();
        await _usuarioRepository.GuardarCambiosAsync();

        return MapToDto(usuario);
    }

    public async Task<UsuarioAdminListDto?> EliminarUsuarioAsync(int usuarioId, int adminId)
    {
        if (usuarioId == adminId)
            throw new InvalidOperationException("No puedes eliminar tu propia cuenta.");

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
        if (usuario is null) return null;

        if (string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No se puede eliminar una cuenta de administrador.");

        var pagina = 1;
        while (true)
        {
            var (anunciosPagina, _) = await _anuncioRepository.BuscarPaginadoAsync(
                new AnuncioQueryFilter
                {
                    UsuarioId = usuarioId,
                    PaginaActual = pagina,
                    CantidadPorPagina = 50
                });

            if (!anunciosPagina.Any())
                break;

            foreach (var anuncio in anunciosPagina)
                foreach (var foto in anuncio.Fotos)
                    await _almacenadorArchivos.EliminarArchivoAsync(foto);

            pagina++;
        }

        await _usuarioRepository.EliminarAsync(usuario);
        await _usuarioRepository.GuardarCambiosAsync();

        return MapToDto(usuario);
    }

    public async Task<UsuarioAdminListDto?> CambiarRolAsync(int usuarioId, CambiarRolAdminDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
        if (usuario is null) return null;

        if (string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No puedes modificar el rol de otro administrador.");

        var cuenta = await _usuarioCuentaService.CambiarRolAdminAsync(usuarioId, dto);
        return new UsuarioAdminListDto
        {
            UsuarioId = usuarioId,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = cuenta.Email,
            Rol = cuenta.Rol,
            IsActivo = usuario.IsActivo,
            FechaRegistro = usuario.CreatedAt
        };
    }

    private static UsuarioAdminListDto MapToDto(Core.Entities.Usuario u) => new()
    {
        UsuarioId = u.UsuarioId,
        Nombre = u.Nombre,
        Apellido = u.Apellido,
        Email = u.Email,
        Rol = u.Rol,
        IsActivo = u.IsActivo,
        FechaRegistro = u.CreatedAt
    };
}
