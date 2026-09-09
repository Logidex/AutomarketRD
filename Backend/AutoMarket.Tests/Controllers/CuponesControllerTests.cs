using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Application.Features.Cupones.Commands;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class CuponesControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly CuponesController _controller;

    public CuponesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new CuponesController(_mockMediator.Object);
    }

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId),
            new Claim(ClaimTypes.Role, "Dealer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Aplicar_CodigoValido_DebeRetornarOkConDto()
    {
        SimularUsuarioAutenticado("10");
        var dto = new AplicarCuponDto { Codigo = "VERANO2025" };
        var esperado = new CuponAplicadoDto { Nivel = PlanNivel.Pro, Dias = 15 };

        _mockMediator.Setup(s => s.Send(It.IsAny<AplicarCuponCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(esperado);

        var resultado = await _controller.Aplicar(dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(esperado, okResult.Value);
    }

    [Fact]
    public async Task Aplicar_CodigoInvalido_DebeLanzarBusinessRuleException()
    {
        SimularUsuarioAutenticado("10");
        var dto = new AplicarCuponDto { Codigo = "INVALIDO" };

        _mockMediator.Setup(s => s.Send(It.IsAny<AplicarCuponCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Cupón inválido."));

        await Assert.ThrowsAsync<BusinessRuleException>(() => _controller.Aplicar(dto));
    }
}
