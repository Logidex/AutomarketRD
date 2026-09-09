using Xunit;
using Moq;
using AutoMarket.Application.Features.PerfilDealer.Handlers;
using AutoMarket.Application.Features.PerfilDealer.Commands;
using AutoMarket.Application.Features.PerfilDealer.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Dealer;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Tests.Features.PerfilDealer;

public class PerfilDealerHandlerTests
{
    private readonly Mock<IPerfilDealerService> _mockService;
    private readonly PerfilDealerCommandHandler _commandHandler;
    private readonly PerfilDealerQueryHandler _queryHandler;

    public PerfilDealerHandlerTests()
    {
        _mockService = new Mock<IPerfilDealerService>();
        _commandHandler = new PerfilDealerCommandHandler(_mockService.Object);
        _queryHandler = new PerfilDealerQueryHandler(_mockService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_ActualizarMiPerfil_DebeRetornarPerfilActualizado()
    {
        var dto = new PerfilDealerUpdateDto
        {
            NombreAgencia = "AutoDealer RD",
            Ubicacion = "Santo Domingo",
            TelefonoAgencia = "809-555-1234",
            Descripcion = "Venta de vehiculos usados"
        };
        var command = new ActualizarMiPerfilCommand(10, dto);
        var expected = new PerfilDealerPublicoDto
        {
            Id = 10,
            NombreAgencia = "AutoDealer RD",
            Ubicacion = "Santo Domingo",
            TelefonoAgencia = "809-555-1234"
        };
        _mockService.Setup(s => s.ActualizarMiPerfilAsync(10, dto)).ReturnsAsync(expected);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
        Assert.Equal("AutoDealer RD", result.NombreAgencia);
        _mockService.Verify(s => s.ActualizarMiPerfilAsync(10, dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ActualizarMiPerfil_DealerNoExiste_DebeRetornarNull()
    {
        var dto = new PerfilDealerUpdateDto { NombreAgencia = "No Existe" };
        var command = new ActualizarMiPerfilCommand(999, dto);
        _mockService.Setup(s => s.ActualizarMiPerfilAsync(999, dto)).ReturnsAsync((PerfilDealerPublicoDto?)null);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _mockService.Verify(s => s.ActualizarMiPerfilAsync(999, dto), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerPerfilPublico_DebeRetornarPerfil()
    {
        var command = new ObtenerPerfilPublicoQuery(10);
        var expected = new PerfilDealerPublicoDto
        {
            Id = 10,
            NombreAgencia = "AutoDealer RD",
            Ubicacion = "Santiago",
            EsDealerVerificado = true
        };
        _mockService.Setup(s => s.ObtenerPerfilPublicoAsync(10)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
        Assert.True(result.EsDealerVerificado);
        _mockService.Verify(s => s.ObtenerPerfilPublicoAsync(10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerPerfilPublico_NoExiste_DebeRetornarNull()
    {
        var command = new ObtenerPerfilPublicoQuery(999);
        _mockService.Setup(s => s.ObtenerPerfilPublicoAsync(999)).ReturnsAsync((PerfilDealerPublicoDto?)null);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _mockService.Verify(s => s.ObtenerPerfilPublicoAsync(999), Times.Once);
    }

    [Fact]
    public async Task Handle_ListarAgencias_DebeRetornarPagina()
    {
        var command = new ListarAgenciasQuery("auto", true, "Pro", 1, 10);
        var items = new List<AgenciaListadoDto>
        {
            new() { Id = 1, NombreAgencia = "AutoPro", EsDealerVerificado = true, PlanNivel = "Pro" },
            new() { Id = 2, NombreAgencia = "AutoMax", EsDealerVerificado = true, PlanNivel = "Pro" }
        };
        var expected = new PagedResult<AgenciaListadoDto>(items, 2, 1, 10);
        _mockService
            .Setup(s => s.ListarAgenciasAsync("auto", true, "Pro", 1, 10))
            .ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRegistros);
        Assert.Equal(1, result.PaginaActual);
        Assert.Equal(2, result.Items.Count);
        _mockService.Verify(s => s.ListarAgenciasAsync("auto", true, "Pro", 1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_ListarAgencias_SinFiltros_DebeRetornarPagina()
    {
        var command = new ListarAgenciasQuery(null, null, null, 1, 20);
        var items = new List<AgenciaListadoDto>
        {
            new() { Id = 1, NombreAgencia = "Dealer 1" }
        };
        var expected = new PagedResult<AgenciaListadoDto>(items, 1, 1, 20);
        _mockService
            .Setup(s => s.ListarAgenciasAsync(null, null, null, 1, 20))
            .ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        _mockService.Verify(s => s.ListarAgenciasAsync(null, null, null, 1, 20), Times.Once);
    }

    [Fact]
    public async Task Handle_ListarAgencias_Vacia_DebeRetornarPaginaVacia()
    {
        var command = new ListarAgenciasQuery("xyz", false, null, 1, 10);
        var expected = new PagedResult<AgenciaListadoDto>(
            new List<AgenciaListadoDto>(), 0, 1, 10);
        _mockService
            .Setup(s => s.ListarAgenciasAsync("xyz", false, null, 1, 10))
            .ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalRegistros);
        _mockService.Verify(s => s.ListarAgenciasAsync("xyz", false, null, 1, 10), Times.Once);
    }
}
