using AutoMarket.Application.DTOs.Ticket;

namespace AutoMarket.Application.Interfaces;

public interface ITicketService
{
    // ===== Panel del Dealer/Vendedor =====
    Task<int> CrearTicketAsync(TicketCreateDto dto, int usuarioId);
    Task<IReadOnlyCollection<TicketListadoDto>> ObtenerMisTicketsAsync(int usuarioId);
    Task<TicketDetalleDto> ObtenerTicketAsync(int ticketId, int usuarioId);
    Task ResponderTicketAsync(int ticketId, TicketMensajeCreateDto dto, int usuarioId);
    Task CerrarTicketAsync(int ticketId, int usuarioId);

    // ===== Panel del Administrador =====
    Task<IReadOnlyCollection<TicketListadoDto>> ObtenerTicketsAdminAsync();
    Task<TicketDetalleDto> ObtenerTicketAdminAsync(int ticketId);
    Task ResponderTicketAdminAsync(int ticketId, TicketMensajeCreateDto dto, int adminId);
    Task CambiarEstadoAdminAsync(int ticketId, CambiarEstadoTicketDto dto);
    Task<TicketResumenAdminDto> ObtenerResumenAdminAsync();
}
