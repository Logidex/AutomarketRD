using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IUsuarioRepository
{
    Task<bool> ExisteEmailAsync(string email);
    Task<Usuario> CrearUsuarioAsync(Usuario usuario);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<Usuario?> ObtenerPorEmailParaEscrituraAsync(string email);
    Task<Usuario?> ObtenerPorCodigoConfirmacionEmailAsync(string codigoHash);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerDealerConPerfilPorIdAsync(int usuarioId);
    Task EliminarPerfilDealerAsync(int usuarioId);
    Task GuardarCambiosAsync();
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<bool> ActualizarContrasenaAsync(string email, string nuevoPasswordHash);

}