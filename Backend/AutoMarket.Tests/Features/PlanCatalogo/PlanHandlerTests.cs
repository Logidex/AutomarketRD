using Xunit;
using Moq;
using AutoMarket.Application.Features.PlanCatalogo.Handlers;
using AutoMarket.Application.Features.PlanCatalogo.Commands;
using AutoMarket.Application.Features.PlanCatalogo.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Features.PlanCatalogo;

public class PlanHandlerTests
{
    private readonly Mock<IPlanCatalogoService> _mockService;
    private readonly PlanCommandHandler _commandHandler;
    private readonly PlanQueryHandler _queryHandler;

    public PlanHandlerTests()
    {
        _mockService = new Mock<IPlanCatalogoService>();
        _commandHandler = new PlanCommandHandler(_mockService.Object);
        _queryHandler = new PlanQueryHandler(_mockService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_CrearPlan_DebeRetornarPlanAdmin()
    {
        var dto = new PlanCatalogoCreateDto
        {
            Nivel = PlanNivel.Basico,
            Nombre = "Basico",
            PrecioMensual = 500,
            LimiteAnuncios = 10,
            MaxFotos = 5,
            DiasVigencia = 30,
            CuotaDestacados = 2
        };
        var command = new CrearPlanCommand(dto);
        var expected = new PlanCatalogoAdminDto
        {
            Id = 1,
            Nivel = PlanNivel.Basico,
            Nombre = "Basico",
            Activo = true
        };
        _mockService.Setup(s => s.CrearPlanAsync(dto)).ReturnsAsync(expected);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Nombre, result.Nombre);
        Assert.True(result.Activo);
        _mockService.Verify(s => s.CrearPlanAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ActualizarPlan_DebeRetornarPlanActualizado()
    {
        var dto = new PlanCatalogoUpdateDto
        {
            Nombre = "Basico Plus",
            PrecioMensual = 750,
            LimiteAnuncios = 15,
            MaxFotos = 8,
            DiasVigencia = 30,
            CuotaDestacados = 3
        };
        var command = new ActualizarPlanCommand(1, dto);
        var expected = new PlanCatalogoAdminDto
        {
            Id = 1,
            Nivel = PlanNivel.Basico,
            Nombre = "Basico Plus",
            Activo = true
        };
        _mockService.Setup(s => s.ActualizarPlanAsync(1, dto)).ReturnsAsync(expected);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Equal("Basico Plus", result.Nombre);
        _mockService.Verify(s => s.ActualizarPlanAsync(1, dto), Times.Once);
    }

    [Fact]
    public async Task Handle_EliminarPlan_DebeLlamarServicio()
    {
        var command = new EliminarPlanCommand(5);
        _mockService.Setup(s => s.EliminarPlanAsync(5)).Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.EliminarPlanAsync(5), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerCatalogoPublico_DebeRetornarLista()
    {
        var command = new ObtenerCatalogoPublicoQuery();
        var expected = new List<PlanCatalogoDto>
        {
            new() { Nivel = PlanNivel.Gratis, Nombre = "Gratis", PrecioMensual = 0 },
            new() { Nivel = PlanNivel.Basico, Nombre = "Basico", PrecioMensual = 500 }
        };
        _mockService.Setup(s => s.ObtenerCatalogoPublicoAsync()).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _mockService.Verify(s => s.ObtenerCatalogoPublicoAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerPlanPorNivel_DebeRetornarPlan()
    {
        var command = new ObtenerPlanPorNivelQuery(PlanNivel.Pro);
        var expected = new PlanCatalogoDto
        {
            Nivel = PlanNivel.Pro,
            Nombre = "Pro",
            PrecioMensual = 1500
        };
        _mockService.Setup(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Pro)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(PlanNivel.Pro, result!.Nivel);
        _mockService.Verify(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Pro), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerPlanPorNivel_NoExiste_DebeRetornarNull()
    {
        var command = new ObtenerPlanPorNivelQuery(PlanNivel.Elite);
        _mockService.Setup(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Elite)).ReturnsAsync((PlanCatalogoDto?)null);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _mockService.Verify(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Elite), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerCatalogoAdmin_DebeRetornarListaCompleta()
    {
        var command = new ObtenerCatalogoAdminQuery();
        var expected = new List<PlanCatalogoAdminDto>
        {
            new() { Id = 1, Nivel = PlanNivel.Gratis, Nombre = "Gratis", Activo = true },
            new() { Id = 2, Nivel = PlanNivel.Basico, Nombre = "Basico", Activo = true },
            new() { Id = 3, Nivel = PlanNivel.Pro, Nombre = "Pro", Activo = false }
        };
        _mockService.Setup(s => s.ObtenerCatalogoAdminAsync()).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => !p.Activo);
        _mockService.Verify(s => s.ObtenerCatalogoAdminAsync(), Times.Once);
    }
}
