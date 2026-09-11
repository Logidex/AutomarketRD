using Xunit;
using Moq;
using MediatR;
using AutoMarket.Application.Features.Tickets.Handlers;
using AutoMarket.Application.Features.Tickets.Commands;
using AutoMarket.Application.Features.Tickets.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Features.Tickets;

public class TicketHandlerTests
{
    private readonly Mock<ITicketService> _mockTicketService;
    private readonly TicketCommandHandler _commandHandler;
    private readonly TicketQueryHandler _queryHandler;

    public TicketHandlerTests()
    {
        _mockTicketService = new Mock<ITicketService>();
        _commandHandler = new TicketCommandHandler(_mockTicketService.Object);
        _queryHandler = new TicketQueryHandler(_mockTicketService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_CrearTicket_DebeRetornarId()
    {
        // Arrange
        var dto = new TicketCreateDto
        {
            Asunto = "Problema con mi anuncio",
            Categoria = TicketCategoria.General,
            Prioridad = TicketPrioridad.Normal,
            Mensaje = "No puedo publicar mi anuncio"
        };
        var usuarioId = 10;
        var command = new CrearTicketCommand(dto, usuarioId);
        _mockTicketService.Setup(s => s.CrearTicketAsync(dto, usuarioId)).ReturnsAsync(1);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, resultado);
        _mockTicketService.Verify(s => s.CrearTicketAsync(dto, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_AgregarMensajeTicket_DebeLlamarServicio()
    {
        // Arrange
        var ticketId = 1;
        var dto = new TicketMensajeCreateDto { Mensaje = "Mi respuesta al ticket" };
        var usuarioId = 10;
        var command = new AgregarMensajeTicketCommand(ticketId, dto, usuarioId);
        _mockTicketService
            .Setup(s => s.ResponderTicketAsync(ticketId, dto, usuarioId))
            .Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockTicketService.Verify(s => s.ResponderTicketAsync(ticketId, dto, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_CerrarTicket_DebeLlamarServicio()
    {
        // Arrange
        var ticketId = 1;
        var usuarioId = 10;
        var command = new CerrarTicketCommand(ticketId, usuarioId);
        _mockTicketService
            .Setup(s => s.CerrarTicketAsync(ticketId, usuarioId))
            .Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockTicketService.Verify(s => s.CerrarTicketAsync(ticketId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_AgregarMensajeAdminTicket_DebeLlamarServicio()
    {
        // Arrange
        var ticketId = 1;
        var dto = new TicketMensajeCreateDto { Mensaje = "Respuesta del administrador" };
        var adminId = 100;
        var command = new AgregarMensajeAdminTicketCommand(ticketId, dto, adminId);
        _mockTicketService
            .Setup(s => s.ResponderTicketAdminAsync(ticketId, dto, adminId))
            .Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockTicketService.Verify(s => s.ResponderTicketAdminAsync(ticketId, dto, adminId), Times.Once);
    }

    [Fact]
    public async Task Handle_CambiarEstadoTicket_DebeLlamarServicio()
    {
        // Arrange
        var ticketId = 1;
        var dto = new CambiarEstadoTicketDto { NuevoEstado = TicketEstado.Cerrado };
        var command = new CambiarEstadoTicketCommand(ticketId, dto);
        _mockTicketService
            .Setup(s => s.CambiarEstadoAdminAsync(ticketId, dto))
            .Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockTicketService.Verify(s => s.CambiarEstadoAdminAsync(ticketId, dto), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerMisTickets_DebeRetornarLista()
    {
        // Arrange
        var usuarioId = 10;
        var command = new ObtenerMisTicketsQuery(usuarioId);
        var expectedTickets = new List<TicketListadoDto>
        {
            new() { Id = 1, Asunto = "Ticket 1", Estado = TicketEstado.Abierto },
            new() { Id = 2, Asunto = "Ticket 2", Estado = TicketEstado.Cerrado }
        };
        _mockTicketService
            .Setup(s => s.ObtenerMisTicketsAsync(usuarioId))
            .ReturnsAsync(expectedTickets);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockTicketService.Verify(s => s.ObtenerMisTicketsAsync(usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTicket_DebeRetornarDetalle()
    {
        // Arrange
        var ticketId = 1;
        var usuarioId = 10;
        var command = new ObtenerTicketQuery(ticketId, usuarioId);
        var expectedDetalle = new TicketDetalleDto
        {
            Id = ticketId,
            Asunto = "Ticket de prueba",
            Estado = TicketEstado.Abierto,
            Mensajes = new List<TicketMensajeDto>
            {
                new() { Id = 1, Mensaje = "Mensaje de prueba", AutorNombre = "Juan" }
            }
        };
        _mockTicketService
            .Setup(s => s.ObtenerTicketAsync(ticketId, usuarioId))
            .ReturnsAsync(expectedDetalle);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(ticketId, resultado.Id);
        Assert.Single(resultado.Mensajes);
        _mockTicketService.Verify(s => s.ObtenerTicketAsync(ticketId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTicketsAdmin_DebeRetornarLista()
    {
        // Arrange
        var command = new ObtenerTicketsAdminQuery();
        var expectedTickets = new List<TicketListadoDto>
        {
            new() { Id = 1, Asunto = "Ticket Admin 1", Estado = TicketEstado.Abierto },
            new() { Id = 2, Asunto = "Ticket Admin 2", Estado = TicketEstado.EnProceso }
        };
        _mockTicketService
            .Setup(s => s.ObtenerTicketsAdminAsync())
            .ReturnsAsync(expectedTickets);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockTicketService.Verify(s => s.ObtenerTicketsAdminAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTicketAdmin_DebeRetornarDetalle()
    {
        // Arrange
        var ticketId = 1;
        var command = new ObtenerTicketAdminQuery(ticketId);
        var expectedDetalle = new TicketDetalleDto
        {
            Id = ticketId,
            Asunto = "Ticket Admin",
            Estado = TicketEstado.Abierto
        };
        _mockTicketService
            .Setup(s => s.ObtenerTicketAdminAsync(ticketId))
            .ReturnsAsync(expectedDetalle);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(ticketId, resultado.Id);
        _mockTicketService.Verify(s => s.ObtenerTicketAdminAsync(ticketId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerResumenAdmin_DebeRetornarResumen()
    {
        // Arrange
        var command = new ObtenerResumenAdminQuery();
        var expectedResumen = new TicketResumenAdminDto { CantidadAbiertos = 5 };
        _mockTicketService
            .Setup(s => s.ObtenerResumenAdminAsync())
            .ReturnsAsync(expectedResumen);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.CantidadAbiertos);
        _mockTicketService.Verify(s => s.ObtenerResumenAdminAsync(), Times.Once);
    }
}
