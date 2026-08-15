using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Ticket;

public class TicketListadoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = null!;
    public string UsuarioEmail { get; set; } = null!;
    public string Asunto { get; set; } = null!;
    public TicketCategoria Categoria { get; set; }
    public TicketPrioridad Prioridad { get; set; }
    public TicketEstado Estado { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime FechaActualizacionUtc { get; set; }
    public string UltimoMensaje { get; set; } = null!;
    public int CantidadMensajes { get; set; }
    // Indica si el último mensaje lo escribió el administrador (para el badge "respuesta nueva").
    public bool UltimoMensajeEsAdmin { get; set; }
}

public class TicketMensajeDto
{
    public int Id { get; set; }
    public int AutorId { get; set; }
    public string AutorNombre { get; set; } = null!;
    public bool EsAdmin { get; set; }
    public string Mensaje { get; set; } = null!;
    public DateTime FechaCreacionUtc { get; set; }
}

public class TicketDetalleDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = null!;
    public string UsuarioEmail { get; set; } = null!;
    public string Asunto { get; set; } = null!;
    public TicketCategoria Categoria { get; set; }
    public TicketPrioridad Prioridad { get; set; }
    public TicketEstado Estado { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime FechaActualizacionUtc { get; set; }
    public List<TicketMensajeDto> Mensajes { get; set; } = new();
}

public class TicketResumenAdminDto
{
    public int CantidadAbiertos { get; set; }
}
