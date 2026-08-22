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
    Task<(
        IReadOnlyCollection<PerfilDealer> Agencias,
        IReadOnlyDictionary<int, int> AnunciosPorAgencia,
        int TotalRegistros
    )> BuscarAgenciasAsync(
        string? busqueda,
        bool? soloVerificadas,
        string? planNivel,
        int pagina,
        int cantidadPorPagina);
    Task GuardarCambiosAsync();
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<bool> ActualizarContrasenaAsync(string email, string nuevoPasswordHash);

    /// <summary>Marca el usuario para eliminación (cascade de dependientes al guardar).</summary>
    Task EliminarAsync(Usuario usuario);
}