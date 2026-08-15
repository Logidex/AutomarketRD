using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoMarket.Tests.Repositories;

public class TicketRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TicketRepository _repositorio;

    public TicketRepositoryTests()
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(opciones);
        _repositorio = new TicketRepository(_context);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static void Set(object obj, string propiedad, object valor)
    {
        var prop = obj.GetType().GetProperty(
            propiedad,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Propiedad '{propiedad}' no encontrada.");

        prop.SetValue(obj, valor);
    }

    private static Usuario CrearUsuario(int id, string email, string rol = "Dealer")
    {
        var usuario = new Usuario("Ana", "Dealer", email, "hash", null, rol);
        Set(usuario, "UsuarioId", id);
        return usuario;
    }

    private static Ticket CrearTicket(int id, int usuarioId, string asunto, TicketEstado estado, string mensajeInicial)
    {
        var ticket = new Ticket(usuarioId, asunto, TicketCategoria.General, TicketPrioridad.Normal, mensajeInicial);
        Set(ticket, "Id", id);
        Set(ticket, "Estado", estado);
        return ticket;
    }

    private async Task SembrarEscenario()
    {
        _context.Usuarios.AddRange(
            CrearUsuario(1, "usuario1@test.com"),
            CrearUsuario(2, "usuario2@test.com"));

        _context.Tickets.AddRange(
            CrearTicket(1, 1, "Abierto del usuario 1", TicketEstado.Abierto, "Hola, necesito ayuda"),
            CrearTicket(2, 1, "Resuelto del usuario 1", TicketEstado.Resuelto, "Gracias por resolverlo"),
            CrearTicket(3, 2, "En proceso del usuario 2", TicketEstado.EnProceso, "Tengo otra duda"));

        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // 1. El dueño solo ve sus propios tickets
    // =========================================================================
    [Fact]
    public async Task ObtenerPorUsuarioIdAsync_DevuelveSoloTicketsDelUsuario()
    {
        await SembrarEscenario();

        var tickets = await _repositorio.ObtenerPorUsuarioIdAsync(1);

        Assert.Equal(2, tickets.Count);
        Assert.All(tickets, t => Assert.Equal(1, t.UsuarioId));
        Assert.Contains(tickets, t => t.Id == 1);
        Assert.Contains(tickets, t => t.Id == 2);
    }

    // =========================================================================
    // 2. El listado admin trae todos los tickets
    // =========================================================================
    [Fact]
    public async Task ObtenerTodosAdminAsync_DevuelveTodosLosTickets()
    {
        await SembrarEscenario();

        var tickets = await _repositorio.ObtenerTodosAdminAsync();

        Assert.Equal(3, tickets.Count);
    }

    // =========================================================================
    // 3. ContarAbiertos solo suma Abierto y EnProceso
    // =========================================================================
    [Fact]
    public async Task ContarAbiertosAsync_NoCuentaResueltosNiCerrados()
    {
        await SembrarEscenario();

        var abiertos = await _repositorio.ContarAbiertosAsync();

        // Abierto(1) + EnProceso(3) = 2; Resuelto(2) no cuenta.
        Assert.Equal(2, abiertos);
    }

    // =========================================================================
    // 4. El detalle incluye los mensajes
    // =========================================================================
    [Fact]
    public async Task ObtenerPorIdConMensajesAsync_IncluyeMensajes()
    {
        await SembrarEscenario();

        var ticket = await _repositorio.ObtenerPorIdConMensajesAsync(1);

        Assert.NotNull(ticket);
        Assert.Single(ticket!.Mensajes);
        Assert.Equal("Hola, necesito ayuda", ticket.Mensajes.Single().Mensaje);
    }

    // =========================================================================
    // 5. Agregar ticket persiste y genera Id
    // =========================================================================
    [Fact]
    public async Task AgregarAsync_AsignaIdYGuarda()
    {
        var ticket = new Ticket(1, "Nuevo caso", TicketCategoria.Cuenta, TicketPrioridad.Urgente, "Perdí el acceso a mi cuenta");

        await _repositorio.AgregarAsync(ticket);

        Assert.True(ticket.Id > 0);
        Assert.Equal(1, await _context.Tickets.CountAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
