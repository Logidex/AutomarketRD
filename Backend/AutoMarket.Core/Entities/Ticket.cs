using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Core.Entities;

public class Ticket
{
    public int Id { get; private set; }

    // Usuario que abre el ticket (Dealer o Vendedor)
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string Asunto { get; private set; } = null!;
    public TicketCategoria Categoria { get; private set; }
    public TicketPrioridad Prioridad { get; private set; }
    public TicketEstado Estado { get; private set; }

    public DateTime FechaCreacionUtc { get; private set; }
    public DateTime FechaActualizacionUtc { get; private set; }

    private readonly List<TicketMensaje> _mensajes = new();
    public IReadOnlyCollection<TicketMensaje> Mensajes => _mensajes.AsReadOnly();

    private Ticket() { }

    public Ticket(int usuarioId, string asunto, TicketCategoria categoria, TicketPrioridad prioridad, string mensajeInicial)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("El ID del usuario es inválido.");

        if (string.IsNullOrWhiteSpace(asunto))
            throw new ArgumentException("El asunto es obligatorio.");

        if (string.IsNullOrWhiteSpace(mensajeInicial))
            throw new ArgumentException("Debes escribir un mensaje para abrir el ticket.");

        UsuarioId = usuarioId;
        Asunto = asunto.Trim();
        Categoria = categoria;
        Prioridad = prioridad;
        Estado = TicketEstado.Abierto;
        FechaCreacionUtc = DateTime.UtcNow;
        FechaActualizacionUtc = FechaCreacionUtc;

        _mensajes.Add(new TicketMensaje(ticket: this, autorId: usuarioId, esAdmin: false, mensaje: mensajeInicial));
    }

    public void AgregarMensaje(int autorId, bool esAdmin, string mensaje)
    {
        if (Estado == TicketEstado.Cerrado)
            throw new BusinessRuleException("Este ticket está cerrado y no admite más mensajes.");

        if (string.IsNullOrWhiteSpace(mensaje))
            throw new ArgumentException("El mensaje no puede estar vacío.");

        _mensajes.Add(new TicketMensaje(ticket: this, autorId: autorId, esAdmin: esAdmin, mensaje: mensaje));
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    public void CambiarEstado(TicketEstado nuevoEstado)
    {
        if (Estado == TicketEstado.Cerrado)
            throw new BusinessRuleException("Este ticket está cerrado y no se puede modificar.");

        Estado = nuevoEstado;
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    public void Cerrar()
    {
        Estado = TicketEstado.Cerrado;
        FechaActualizacionUtc = DateTime.UtcNow;
    }
}
