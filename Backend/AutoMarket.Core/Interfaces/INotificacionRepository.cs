using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface INotificacionRepository
{
    Task<(IEnumerable<Notificacion> Items, int Total)> ObtenerPorUsuarioAsync(
        int usuarioId, int pagina, int tamanoPagina);
    Task<int> ContarNoLeidasAsync(int usuarioId);
    Task MarcarComoLeidaAsync(int notificacionId, int usuarioId);
    Task MarcarTodasComoLeidasAsync(int usuarioId);
    Task CrearAsync(Notificacion notificacion);
}
