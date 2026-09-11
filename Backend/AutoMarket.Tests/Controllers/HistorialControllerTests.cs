using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Historial;
using AutoMarket.Application.Features.HistorialVista.Commands;
using AutoMarket.Application.Features.HistorialVista.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class HistorialControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly HistorialController _controller;

    public HistorialControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new HistorialController(_mockMediator.Object);
    }

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task RegistrarVista_DatosValidos_DebeLlamarAlServicio()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator.Setup(s => s.Send(It.IsAny<RegistrarVistaCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _controller.RegistrarVista(5);

        Assert.IsType<OkObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(It.IsAny<RegistrarVistaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerRecientes_DebeRetornarOkConLista()
    {
        SimularUsuarioAutenticado("15");
        var recientes = new List<AnuncioRecienteDto>
        {
            new() { Id = 1, Marca = "Toyota", Modelo = "Corolla" }
        };
        _mockMediator.Setup(s => s.Send(It.IsAny<ObtenerRecientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recientes);

        var resultado = await _controller.ObtenerRecientes();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IReadOnlyCollection<AnuncioRecienteDto>>(okResult.Value);
        Assert.Single(lista);
    }
}
