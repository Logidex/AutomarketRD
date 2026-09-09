using Xunit;
using Moq;
using AutoMarket.Application.Features.Cupones.Handlers;
using AutoMarket.Application.Features.Cupones.Commands;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Features.Cupones;

public class CuponHandlerTests
{
    private readonly Mock<ICuponService> _mockService;
    private readonly CuponCommandHandler _commandHandler;

    public CuponHandlerTests()
    {
        _mockService = new Mock<ICuponService>();
        _commandHandler = new CuponCommandHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_AplicarCupon_DebeRetornarResultado()
    {
        var command = new AplicarCuponCommand(PerfilDealerId: 1, Codigo: "DESCUENTO20");
        var expected = new CuponAplicadoDto
        {
            Nivel = PlanNivel.Pro,
            Dias = 30,
            FechaVencimientoUtc = DateTime.UtcNow.AddDays(30),
            Mensaje = "Cupón aplicado exitosamente"
        };
        _mockService
            .Setup(s => s.AplicarCuponAsync(1, "DESCUENTO20"))
            .ReturnsAsync(expected);

        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(PlanNivel.Pro, resultado.Nivel);
        Assert.Equal(30, resultado.Dias);
        Assert.Equal("Cupón aplicado exitosamente", resultado.Mensaje);
        _mockService.Verify(s => s.AplicarCuponAsync(1, "DESCUENTO20"), Times.Once);
    }

    [Fact]
    public async Task Handle_AplicarCupon_CodigoInvalido_DebeRetornarNull()
    {
        var command = new AplicarCuponCommand(PerfilDealerId: 1, Codigo: "INVALIDO");
        _mockService
            .Setup<Task<CuponAplicadoDto>>(s => s.AplicarCuponAsync(1, "INVALIDO"))
            .ReturnsAsync((CuponAplicadoDto?)null);

        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Null(resultado);
        _mockService.Verify(s => s.AplicarCuponAsync(1, "INVALIDO"), Times.Once);
    }
}
