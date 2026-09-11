using Xunit;
using Moq;
using MediatR;
using AutoMarket.Application.Features.Leads.Handlers;
using AutoMarket.Application.Features.Leads.Commands;
using AutoMarket.Application.Features.Leads.Queries;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Lead;

namespace AutoMarket.Tests.Features.Leads;

public class LeadHandlerTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly LeadCommandHandler _commandHandler;
    private readonly LeadQueryHandler _queryHandler;

    public LeadHandlerTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _commandHandler = new LeadCommandHandler(_mockLeadService.Object);
        _queryHandler = new LeadQueryHandler(_mockLeadService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_CrearLead_DebeLlamarServicio()
    {
        // Arrange
        var dto = new LeadCreateDto
        {
            AnuncioId = 1,
            NombreContacto = "Juan Perez",
            EmailContacto = "juan@test.com",
            TelefonoContacto = "8091234567",
            Mensaje = "Estoy interesado en este vehículo",
            Canal = CanalContacto.WhatsApp
        };
        var usuarioIdRemitente = 10;
        var command = new CrearLeadCommand(dto, usuarioIdRemitente);
        _mockLeadService
            .Setup(s => s.CrearLeadAsync(dto, usuarioIdRemitente))
            .Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockLeadService.Verify(s => s.CrearLeadAsync(dto, usuarioIdRemitente), Times.Once);
    }

    [Fact]
    public async Task Handle_MarcarLeido_DebeRetornarTrue()
    {
        // Arrange
        var leadId = 1;
        var usuarioId = 10;
        var command = new MarcarLeidoCommand(leadId, usuarioId);
        _mockLeadService.Setup(s => s.MarcarLeidoAsync(leadId, usuarioId)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockLeadService.Verify(s => s.MarcarLeidoAsync(leadId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_MarcarTodosLeidos_DebeRetornarCantidad()
    {
        // Arrange
        var usuarioId = 10;
        var command = new MarcarTodosLeidosCommand(usuarioId);
        _mockLeadService.Setup(s => s.MarcarTodosLeidosAsync(usuarioId)).ReturnsAsync(5);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, resultado);
        _mockLeadService.Verify(s => s.MarcarTodosLeidosAsync(usuarioId), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerLeadsPorAnuncio_DebeRetornarLista()
    {
        // Arrange
        var anuncioId = 1;
        var usuarioId = 10;
        var command = new ObtenerLeadsPorAnuncioQuery(anuncioId, usuarioId);
        var expectedLeads = new List<Lead>
        {
            new Lead(anuncioId, "Juan", "juan@test.com", "8091234567", "Interesado", CanalContacto.WhatsApp),
            new Lead(anuncioId, "Maria", "maria@test.com", "8099876543", "También interesada", CanalContacto.Formulario)
        };
        _mockLeadService
            .Setup(s => s.ObtenerLeadsPorAnuncioAsync(anuncioId, usuarioId))
            .ReturnsAsync(expectedLeads);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockLeadService.Verify(s => s.ObtenerLeadsPorAnuncioAsync(anuncioId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerLeadsPorDealer_DebeRetornarLista()
    {
        // Arrange
        var dealerId = 5;
        var command = new ObtenerLeadsPorDealerQuery(dealerId);
        var expectedLeads = new List<LeadDealerDto>
        {
            new() { Id = 1, AnuncioId = 10, NombreContacto = "Juan" },
            new() { Id = 2, AnuncioId = 20, NombreContacto = "Maria" }
        };
        _mockLeadService
            .Setup(s => s.ObtenerLeadsPorDealerAsync(dealerId))
            .ReturnsAsync(expectedLeads);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockLeadService.Verify(s => s.ObtenerLeadsPorDealerAsync(dealerId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerResumenNoLeidos_DebeRetornarResumen()
    {
        // Arrange
        var usuarioId = 10;
        var command = new ObtenerResumenNoLeidosQuery(usuarioId);
        var expectedResumen = new LeadNoLeidosResumenDto
        {
            CantidadNoLeidos = 3,
            Recientes = new List<LeadDealerDto>
            {
                new() { Id = 1, NombreContacto = "Juan" }
            }
        };
        _mockLeadService
            .Setup(s => s.ObtenerResumenNoLeidosAsync(usuarioId))
            .ReturnsAsync(expectedResumen);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.CantidadNoLeidos);
        Assert.Single(resultado.Recientes);
        _mockLeadService.Verify(s => s.ObtenerResumenNoLeidosAsync(usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerMisContactos_DebeRetornarLista()
    {
        // Arrange
        var usuarioId = 10;
        var command = new ObtenerMisContactosQuery(usuarioId);
        var expectedContactos = new List<LeadContactoUsuarioDto>
        {
            new() { Id = 1, AnuncioId = 10, Marca = "Toyota", Modelo = "Corolla" },
            new() { Id = 2, AnuncioId = 20, Marca = "Honda", Modelo = "Civic" }
        };
        _mockLeadService
            .Setup(s => s.ObtenerMisContactosAsync(usuarioId))
            .ReturnsAsync(expectedContactos);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockLeadService.Verify(s => s.ObtenerMisContactosAsync(usuarioId), Times.Once);
    }
}
