using Xunit;
using Moq;
using AutoMarket.Application.Features.HistorialVista.Handlers;
using AutoMarket.Application.Features.HistorialVista.Commands;
using AutoMarket.Application.Features.HistorialVista.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Historial;

namespace AutoMarket.Tests.Features.HistorialVista;

public class HistorialHandlerTests
{
    private readonly Mock<IHistorialVistaService> _mockService;
    private readonly HistorialCommandHandler _commandHandler;
    private readonly HistorialQueryHandler _queryHandler;

    public HistorialHandlerTests()
    {
        _mockService = new Mock<IHistorialVistaService>();
        _commandHandler = new HistorialCommandHandler(_mockService.Object);
        _queryHandler = new HistorialQueryHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_RegistrarVista_DebeLlamarServicio()
    {
        var command = new RegistrarVistaCommand(UsuarioId: 1, AnuncioId: 10);
        _mockService
            .Setup(s => s.RegistrarVistaAsync(1, 10))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.RegistrarVistaAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerRecientes_DebeRetornarLista()
    {
        var query = new ObtenerRecientesQuery(UsuarioId: 1, Cantidad: 5);
        var expected = new List<AnuncioRecienteDto>
        {
            new() { Id = 1, Marca = "Toyota", Modelo = "Corolla", Anio = 2023, Precio = 1200000, VistoEnUtc = DateTime.UtcNow },
            new() { Id = 2, Marca = "Honda", Modelo = "Civic", Anio = 2022, Precio = 950000, VistoEnUtc = DateTime.UtcNow.AddMinutes(-30) }
        };
        _mockService
            .Setup(s => s.ObtenerRecientesAsync(1, 5))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockService.Verify(s => s.ObtenerRecientesAsync(1, 5), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerRecientes_SinHistorial_DebeRetornarListaVacia()
    {
        var query = new ObtenerRecientesQuery(UsuarioId: 99, Cantidad: 5);
        _mockService
            .Setup(s => s.ObtenerRecientesAsync(99, 5))
            .ReturnsAsync(new List<AnuncioRecienteDto>());

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
        _mockService.Verify(s => s.ObtenerRecientesAsync(99, 5), Times.Once);
    }
}
