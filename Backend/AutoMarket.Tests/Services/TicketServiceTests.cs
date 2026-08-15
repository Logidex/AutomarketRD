using Moq;
using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AutoMarket.Tests.Services;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _mockTicketRepo;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
    private readonly Mock<IEmailSenderService> _mockEmailSender;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<TicketService>> _mockLogger;
    private readonly TicketService _servicio;

    public TicketServiceTests()
    {
        _mockTicketRepo = new Mock<ITicketRepository>();
        _mockUsuarioRepo = new Mock<IUsuarioRepository>();
        _mockEmailSender = new Mock<IEmailSenderService>();
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["Admin:Email"]).Returns("admin@automarket.do");
        _mockLogger = new Mock<ILogger<TicketService>>();

        _servicio = new TicketService(
            _mockTicketRepo.Object,
            _mockUsuarioRepo.Object,
            _mockEmailSender.Object,
            _mockConfig.Object,
            _mockLogger.Object);
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

    private static Usuario CrearUsuario(int usuarioId, string email, string rol = "Dealer")
    {
        var usuario = new Usuario("Juan", "Perez", email, "hash", null, rol);
        Set(usuario, "UsuarioId", usuarioId);
        return usuario;
    }

    private static Ticket CrearTicket(
        int id,
        int usuarioId,
        TicketEstado estado = TicketEstado.Abierto,
        string asunto = "Problema con mi plan")
    {
        var usuario = CrearUsuario(usuarioId, $"usuario{usuarioId}@test.com");
        var ticket = new Ticket(usuarioId, asunto, TicketCategoria.Facturacion, TicketPrioridad.Alta, "Mensaje inicial de prueba");

        Set(ticket, "Id", id);
        Set(ticket, "Usuario", usuario);

        foreach (var mensaje in ticket.Mensajes)
            Set(mensaje, "Autor", usuario);

        if (estado != TicketEstado.Abierto)
            Set(ticket, "Estado", estado);

        return ticket;
    }

    // =========================================================================
    // PRUEBA 01: Crear ticket con éxito (guarda y notifica al admin)
    // =========================================================================
    [Fact]
    public async Task CrearTicketAsync_DatosValidos_DebeGuardarYNotificarAlAdmin()
    {
        // Arrange
        var dto = new TicketCreateDto
        {
            Asunto = "No puedo renovar mi plan",
            Categoria = TicketCategoria.Facturacion,
            Prioridad = TicketPrioridad.Alta,
            Mensaje = "Mi pago fue rechazado dos veces."
        };

        var usuario = CrearUsuario(7, "dealer@test.com");
        _mockUsuarioRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        _mockTicketRepo.Setup(r => r.AgregarAsync(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => Set(t, "Id", 42))
            .Returns(Task.CompletedTask);

        // Act
        var id = await _servicio.CrearTicketAsync(dto, 7);

        // Assert
        Assert.Equal(42, id);
        _mockTicketRepo.Verify(r => r.AgregarAsync(It.Is<Ticket>(t =>
            t.UsuarioId == 7 &&
            t.Asunto == "No puedo renovar mi plan" &&
            t.Estado == TicketEstado.Abierto &&
            t.Mensajes.Count == 1
        )), Times.Once);

        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            "admin@automarket.do",
            It.Is<string>(s => s.Contains("Nuevo ticket de soporte")),
            It.IsAny<string>()), Times.Once);
    }

    // =========================================================================
    // PRUEBA 02: Crear ticket con usuario inexistente
    // =========================================================================
    [Fact]
    public async Task CrearTicketAsync_UsuarioNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = new TicketCreateDto
        {
            Asunto = "Ayuda",
            Mensaje = "Necesito ayuda con mi cuenta por favor."
        };

        _mockUsuarioRepo.Setup(r => r.ObtenerPorIdAsync(99)).ReturnsAsync((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.CrearTicketAsync(dto, 99));
        _mockTicketRepo.Verify(r => r.AgregarAsync(It.IsAny<Ticket>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 03: El dueño del ticket puede ver su detalle
    // =========================================================================
    [Fact]
    public async Task ObtenerTicketAsync_DuenoValido_DebeDevolverDetalle()
    {
        // Arrange
        var ticket = CrearTicket(id: 1, usuarioId: 7);
        _mockTicketRepo.Setup(r => r.ObtenerPorIdConMensajesAsync(1)).ReturnsAsync(ticket);

        // Act
        var detalle = await _servicio.ObtenerTicketAsync(1, 7);

        // Assert
        Assert.Equal(1, detalle.Id);
        Assert.Single(detalle.Mensajes);
        Assert.False(detalle.Mensajes[0].EsAdmin);
    }

    // =========================================================================
    // PRUEBA 04: Un usuario NO dueño no puede ver el ticket
    // =========================================================================
    [Fact]
    public async Task ObtenerTicketAsync_NoEsDueno_DebeLanzarUnauthorizedAccessException()
    {
        // Arrange
        var ticket = CrearTicket(id: 1, usuarioId: 7);
        _mockTicketRepo.Setup(r => r.ObtenerPorIdConMensajesAsync(1)).ReturnsAsync(ticket);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _servicio.ObtenerTicketAsync(1, 999));

        Assert.Contains("no pertenece", excepcion.Message);
    }

    // =========================================================================
    // PRUEBA 05: Responder a un ticket cerrado debe fallar
    // =========================================================================
    [Fact]
    public async Task ResponderTicketAsync_TicketCerrado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var ticket = CrearTicket(id: 1, usuarioId: 7, estado: TicketEstado.Cerrado);
        _mockTicketRepo.Setup(r => r.ObtenerPorIdConMensajesAsync(1)).ReturnsAsync(ticket);

        var dto = new TicketMensajeCreateDto { Mensaje = "¿Pueden reabrir mi caso?" };

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.ResponderTicketAsync(1, dto, 7));

        Assert.Contains("cerrado", excepcion.Message);
        _mockTicketRepo.Verify(r => r.GuardarCambiosAsync(), Times.Never);
    }

    // =========================================================================
    // PRUEBA 06: El admin responde y el ticket pasa a En proceso + notifica al cliente
    // =========================================================================
    [Fact]
    public async Task ResponderTicketAdminAsync_DebePasarAEnProcesoYNotificarAlCliente()
    {
        // Arrange
        var ticket = CrearTicket(id: 1, usuarioId: 7);
        _mockTicketRepo.Setup(r => r.ObtenerPorIdConMensajesAsync(1)).ReturnsAsync(ticket);

        var dto = new TicketMensajeCreateDto { Mensaje = "Estamos revisando tu caso." };

        // Act
        await _servicio.ResponderTicketAdminAsync(1, dto, adminId: 1);

        // Assert
        Assert.Equal(TicketEstado.EnProceso, ticket.Estado);
        Assert.Equal(2, ticket.Mensajes.Count);
        Assert.True(ticket.Mensajes.Last().EsAdmin);

        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            "usuario7@test.com",
            It.Is<string>(s => s.Contains("fue respondido")),
            It.IsAny<string>()), Times.Once);
    }

    // =========================================================================
    // PRUEBA 07: Cambiar estado a Resuelto notifica al cliente
    // =========================================================================
    [Fact]
    public async Task CambiarEstadoAdminAsync_Resuelto_DebeNotificarAlCliente()
    {
        // Arrange
        var ticket = CrearTicket(id: 1, usuarioId: 7);
        _mockTicketRepo.Setup(r => r.ObtenerPorIdConMensajesAsync(1)).ReturnsAsync(ticket);

        var dto = new CambiarEstadoTicketDto { NuevoEstado = TicketEstado.Resuelto };

        // Act
        await _servicio.CambiarEstadoAdminAsync(1, dto);

        // Assert
        Assert.Equal(TicketEstado.Resuelto, ticket.Estado);
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            "usuario7@test.com",
            It.Is<string>(s => s.Contains("Resuelto")),
            It.IsAny<string>()), Times.Once);
    }

    // =========================================================================
    // PRUEBA 08: Cerrar dos veces el mismo ticket debe fallar
    // =========================================================================
    [Fact]
    public async Task CerrarTicketAsync_Repetido_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var ticket = CrearTicket(id: 1, usuarioId: 7);
        _mockTicketRepo.Setup(r => r.ObtenerPorIdConMensajesAsync(1)).ReturnsAsync(ticket);

        // Act
        await _servicio.CerrarTicketAsync(1, 7);

        // Assert (el segundo cierre lanza excepción)
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CerrarTicketAsync(1, 7));

        Assert.Contains("ya está cerrado", excepcion.Message);
    }

    // =========================================================================
    // PRUEBA 09: El resumen del admin devuelve la cantidad de abiertos
    // =========================================================================
    [Fact]
    public async Task ObtenerResumenAdminAsync_DebeDevolverCantidadAbiertos()
    {
        // Arrange
        _mockTicketRepo.Setup(r => r.ContarAbiertosAsync()).ReturnsAsync(4);

        // Act
        var resumen = await _servicio.ObtenerResumenAdminAsync();

        // Assert
        Assert.Equal(4, resumen.CantidadAbiertos);
    }

    // =========================================================================
    // PRUEBA 10: Un fallo SMTP no impide crear el ticket
    // =========================================================================
    [Fact]
    public async Task CrearTicketAsync_SMTPSinConexion_IgualGuardaElTicket()
    {
        // Arrange
        var dto = new TicketCreateDto
        {
            Asunto = "Problema técnico",
            Categoria = TicketCategoria.SoporteTecnico,
            Mensaje = "La página no carga mis anuncios."
        };

        _mockUsuarioRepo.Setup(r => r.ObtenerPorIdAsync(7))
            .ReturnsAsync(CrearUsuario(7, "dealer@test.com"));

        _mockEmailSender.Setup(e => e.EnviarCorreoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP Timeout"));

        _mockTicketRepo.Setup(r => r.AgregarAsync(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => Set(t, "Id", 43))
            .Returns(Task.CompletedTask);

        // Act
        var id = await _servicio.CrearTicketAsync(dto, 7);

        // Assert
        Assert.Equal(43, id);
        _mockTicketRepo.Verify(r => r.AgregarAsync(It.IsAny<Ticket>()), Times.Once);
    }
}
