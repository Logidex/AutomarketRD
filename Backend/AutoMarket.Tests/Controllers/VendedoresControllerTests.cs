using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Vendedores.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class VendedoresControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly VendedoresController _controller;

    public VendedoresControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new VendedoresController(_mockMediator.Object);
    }

    private void SimularVendedorAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId),
            new Claim(ClaimTypes.Role, "Vendedor")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SimularUsuarioNoVendedor(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId),
            new Claim(ClaimTypes.Role, "Comprador")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task ObtenerMiSuscripcion_Vendedor_DebeRetornarOk()
    {
        SimularVendedorAutenticado("15");
        var suscripcion = new VendedorSuscripcionDto { Nivel = "Pro", Activa = true };
        _mockMediator.Setup(s => s.Send(It.IsAny<ObtenerMiSuscripcionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(suscripcion);

        var resultado = await _controller.ObtenerMiSuscripcion();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(suscripcion, okResult.Value);
    }

    [Fact]
    public async Task ObtenerMiSuscripcion_NoVendedor_DebeRetornarForbidden()
    {
        SimularUsuarioNoVendedor("15");

        var resultado = await _controller.ObtenerMiSuscripcion();

        Assert.IsType<ForbidResult>(resultado);
    }

    [Fact]
    public async Task ObtenerPerfilPublico_Existe_DebeRetornarOk()
    {
        var perfil = new VendedorPerfilPublicoDto { UsuarioId = 10, Nombre = "Juan" };
        _mockMediator.Setup(s => s.Send(It.IsAny<ObtenerPerfilPublicoVendedorQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(perfil);

        var resultado = await _controller.ObtenerPerfilPublico(10);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(perfil, okResult.Value);
    }

    [Fact]
    public async Task ObtenerPerfilPublico_NoExiste_DebeRetornarNotFound()
    {
        _mockMediator.Setup(s => s.Send(It.IsAny<ObtenerPerfilPublicoVendedorQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendedorPerfilPublicoDto?)null);

        var resultado = await _controller.ObtenerPerfilPublico(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
