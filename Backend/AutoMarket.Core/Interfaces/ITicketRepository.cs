using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ITicketRepository
{
    Task AgregarAsync(Ticket ticket);

    // Detalle completo: incluye los mensajes ordenados y el usuario dueño.
    Task<Ticket?> ObtenerPorIdConMensajesAsync(int id);

    // Listado para el dueño del ticket (Dealer/Vendedor).
    Task<IReadOnlyCollection<Ticket>> ObtenerPorUsuarioIdAsync(int usuarioId);

    // Listado para el panel de administración.
    Task<IReadOnlyCollection<Ticket>> ObtenerTodosAdminAsync();

    // Cantidad de tickets abiertos/en proceso (para el badge del admin).
    Task<int> ContarAbiertosAsync();

    Task GuardarCambiosAsync();
}
