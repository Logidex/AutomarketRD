using Xunit;
using Moq;
using AutoMarket.Application.Features.CuentasBancarias.Handlers;
using AutoMarket.Application.Features.CuentasBancarias.Commands;
using AutoMarket.Application.Features.CuentasBancarias.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Features.CuentasBancarias;

public class CuentaBancariaHandlerTests
{
    private readonly Mock<ICuentasBancariasService> _mockService;
    private readonly CuentaBancariaCommandHandler _commandHandler;
    private readonly CuentaBancariaQueryHandler _queryHandler;

    public CuentaBancariaHandlerTests()
    {
        _mockService = new Mock<ICuentasBancariasService>();
        _commandHandler = new CuentaBancariaCommandHandler(_mockService.Object);
        _queryHandler = new CuentaBancariaQueryHandler(_mockService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_CrearCuentaBancaria_DebeRetornarCuenta()
    {
        var command = new CrearCuentaBancariaCommand(
            BancoDestino.Popular, "Juan Perez", "123456789",
            "Ahorro", "001-1234567", "Pago servicio");
        var expected = new CuentaBancaria(
            BancoDestino.Popular, "Juan Perez", "123456789",
            "Ahorro", "001-1234567", "Pago servicio") { Id = 1 };
        _mockService
            .Setup(s => s.CrearCuentaAsync(
                BancoDestino.Popular, "Juan Perez", "123456789",
                "Ahorro", "001-1234567", "Pago servicio"))
            .ReturnsAsync(expected);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(BancoDestino.Popular, result.Banco);
        Assert.Equal("Juan Perez", result.NombreTitular);
        _mockService.Verify(s => s.CrearCuentaAsync(
            BancoDestino.Popular, "Juan Perez", "123456789",
            "Ahorro", "001-1234567", "Pago servicio"), Times.Once);
    }

    [Fact]
    public async Task Handle_ActualizarCuentaBancaria_DebeRetornarCuentaActualizada()
    {
        var command = new ActualizarCuentaBancariaCommand(
            1, "Maria Lopez", "98765321",
            "Corriente", "002-7654321", "Referencia actualizada");
        var expected = new CuentaBancaria(
            BancoDestino.QIK, "Maria Lopez", "98765321",
            "Corriente", "002-7654321", "Referencia actualizada") { Id = 1 };
        _mockService
            .Setup(s => s.ActualizarCuentaAsync(
                1, "Maria Lopez", "98765321",
                "Corriente", "002-7654321", "Referencia actualizada"))
            .ReturnsAsync(expected);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Maria Lopez", result.NombreTitular);
        _mockService.Verify(s => s.ActualizarCuentaAsync(
            1, "Maria Lopez", "98765321",
            "Corriente", "002-7654321", "Referencia actualizada"), Times.Once);
    }

    [Fact]
    public async Task Handle_ToggleCuentaBancaria_DebeRetornarCuentaConToggle()
    {
        var command = new ToggleCuentaBancariaCommand(1);
        var expected = new CuentaBancaria(
            BancoDestino.Popular, "Juan Perez", "123456789",
            "Ahorro", "001-1234567", "Pago servicio") { Id = 1, Activa = false };
        _mockService.Setup(s => s.ToggleCuentaAsync(1)).ReturnsAsync(expected);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Activa);
        _mockService.Verify(s => s.ToggleCuentaAsync(1), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerCuentasActivas_DebeRetornarLista()
    {
        var command = new ObtenerCuentasActivasQuery();
        var expected = new List<CuentaBancaria>
        {
            new(BancoDestino.Popular, "Cuenta 1", "111", "Ahorro", "doc1", "ref1") { Id = 1, Activa = true },
            new(BancoDestino.QIK, "Cuenta 2", "222", "Corriente", "doc2", "ref2") { Id = 2, Activa = true }
        };
        _mockService.Setup(s => s.ObtenerCuentasActivasAsync()).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.True(c.Activa));
        _mockService.Verify(s => s.ObtenerCuentasActivasAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTodasCuentas_DebeRetornarListaCompleta()
    {
        var command = new ObtenerTodasCuentasQuery();
        var expected = new List<CuentaBancaria>
        {
            new(BancoDestino.Popular, "Cuenta 1", "111", "Ahorro", "doc1", "ref1") { Id = 1, Activa = true },
            new(BancoDestino.QIK, "Cuenta 2", "222", "Corriente", "doc2", "ref2") { Id = 2, Activa = false }
        };
        _mockService.Setup(s => s.ObtenerTodasAsync()).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => !c.Activa);
        _mockService.Verify(s => s.ObtenerTodasAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerCuentasActivas_Vacia_DebeRetornarListaVacia()
    {
        var command = new ObtenerCuentasActivasQuery();
        _mockService.Setup(s => s.ObtenerCuentasActivasAsync()).ReturnsAsync(new List<CuentaBancaria>());

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
        _mockService.Verify(s => s.ObtenerCuentasActivasAsync(), Times.Once);
    }
}
