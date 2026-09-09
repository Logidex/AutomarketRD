using Xunit;
using Moq;
using AutoMarket.Application.Features.ReportesAnuncio.Handlers;
using AutoMarket.Application.Features.ReportesAnuncio.Commands;
using AutoMarket.Application.Features.ReportesAnuncio.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Features.ReportesAnuncio;

public class ReporteHandlerTests
{
    private readonly Mock<IReporteAnuncioService> _mockService;
    private readonly ReporteCommandHandler _commandHandler;
    private readonly ReporteQueryHandler _queryHandler;

    public ReporteHandlerTests()
    {
        _mockService = new Mock<IReporteAnuncioService>();
        _commandHandler = new ReporteCommandHandler(_mockService.Object);
        _queryHandler = new ReporteQueryHandler(_mockService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_CrearReporte_DebeRetornarId()
    {
        var dto = new CrearReporteDto
        {
            AnuncioId = 42,
            Motivo = ReporteMotivo.ContenidoInapropiado,
            Detalle = "Contenido ofensivo"
        };
        var ip = "192.168.1.1";
        var command = new CrearReporteCommand(dto, ip);
        _mockService.Setup(s => s.CrearAsync(dto, ip)).ReturnsAsync(10);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Equal(10, result);
        _mockService.Verify(s => s.CrearAsync(dto, ip), Times.Once);
    }

    [Fact]
    public async Task Handle_DescartarReporte_DebeLlamarServicio()
    {
        var command = new DescartarReporteCommand(5, 100);
        _mockService.Setup(s => s.DescartarAsync(5, 100)).Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.DescartarAsync(5, 100), Times.Once);
    }

    [Fact]
    public async Task Handle_ResolverReporte_DebeLlamarServicio()
    {
        var command = new ResolverReporteCommand(7, 100);
        _mockService.Setup(s => s.ResolverEliminandoAnuncioAsync(7, 100)).Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.ResolverEliminandoAnuncioAsync(7, 100), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ListarReportesPorEstado_DebeRetornarLista()
    {
        var command = new ListarReportesPorEstadoQuery(ReporteEstado.Pendiente);
        var expected = new List<ReporteAdminDto>
        {
            new() { Id = 1, Motivo = ReporteMotivo.FraudeEstafa, Estado = ReporteEstado.Pendiente },
            new() { Id = 2, Motivo = ReporteMotivo.Duplicado, Estado = ReporteEstado.Pendiente }
        };
        _mockService.Setup(s => s.ListarPorEstadoAsync(ReporteEstado.Pendiente)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(ReporteEstado.Pendiente, r.Estado));
        _mockService.Verify(s => s.ListarPorEstadoAsync(ReporteEstado.Pendiente), Times.Once);
    }

    [Fact]
    public async Task Handle_ListarReportesPorEstado_Resueltos_DebeRetornarLista()
    {
        var command = new ListarReportesPorEstadoQuery(ReporteEstado.Resuelto);
        var expected = new List<ReporteAdminDto>
        {
            new() { Id = 3, Motivo = ReporteMotivo.InformacionFalsa, Estado = ReporteEstado.Resuelto }
        };
        _mockService.Setup(s => s.ListarPorEstadoAsync(ReporteEstado.Resuelto)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        _mockService.Verify(s => s.ListarPorEstadoAsync(ReporteEstado.Resuelto), Times.Once);
    }

    [Fact]
    public async Task Handle_ContarReportesPendientes_DebeRetornarCantidad()
    {
        var command = new ContarReportesPendientesQuery();
        _mockService.Setup(s => s.ContarPendientesAsync()).ReturnsAsync(15);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.Equal(15, result);
        _mockService.Verify(s => s.ContarPendientesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ContarReportesPendientes_Cero_DebeRetornarCero()
    {
        var command = new ContarReportesPendientesQuery();
        _mockService.Setup(s => s.ContarPendientesAsync()).ReturnsAsync(0);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.Equal(0, result);
        _mockService.Verify(s => s.ContarPendientesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ListarReportesPorEstado_Vacia_DebeRetornarColeccionVacia()
    {
        var command = new ListarReportesPorEstadoQuery(ReporteEstado.Descartado);
        _mockService
            .Setup(s => s.ListarPorEstadoAsync(ReporteEstado.Descartado))
            .ReturnsAsync(new List<ReporteAdminDto>());

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
        _mockService.Verify(s => s.ListarPorEstadoAsync(ReporteEstado.Descartado), Times.Once);
    }
}
