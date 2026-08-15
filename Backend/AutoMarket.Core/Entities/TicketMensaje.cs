namespace AutoMarket.Core.Entities;

public class TicketMensaje
{
    public int Id { get; private set; }

    public int TicketId { get; private set; }
    public Ticket Ticket { get; private set; } = null!;

    // Autor del mensaje: el dueño del ticket (Dealer/Vendedor) o un administrador.
    public int AutorId { get; private set; }
    public Usuario Autor { get; private set; } = null!;

    public bool EsAdmin { get; private set; }
    public string Mensaje { get; private set; } = null!;
    public DateTime FechaCreacionUtc { get; private set; }

    private TicketMensaje() { }

    public TicketMensaje(Ticket ticket, int autorId, bool esAdmin, string mensaje)
    {
        Ticket = ticket;
        AutorId = autorId;
        EsAdmin = esAdmin;
        Mensaje = mensaje.Trim();
        FechaCreacionUtc = DateTime.UtcNow;
    }
}
