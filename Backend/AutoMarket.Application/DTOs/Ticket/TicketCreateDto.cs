using System.ComponentModel.DataAnnotations;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Ticket;

public class TicketCreateDto
{
    [Required(ErrorMessage = "El asunto es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El asunto debe tener entre 3 y 150 caracteres.")]
    public string Asunto { get; set; } = null!;

    public TicketCategoria Categoria { get; set; } = TicketCategoria.General;

    public TicketPrioridad Prioridad { get; set; } = TicketPrioridad.Normal;

    [Required(ErrorMessage = "Debes escribir un mensaje para abrir el ticket.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 2000 caracteres.")]
    public string Mensaje { get; set; } = null!;
}

public class TicketMensajeCreateDto
{
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "El mensaje no puede superar los 2000 caracteres.")]
    public string Mensaje { get; set; } = null!;
}

public class CambiarEstadoTicketDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
    public TicketEstado NuevoEstado { get; set; }
}
