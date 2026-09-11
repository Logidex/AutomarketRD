using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.Auth.Commands;
using AutoMarket.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class UsuarioControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly UsuarioController _controller;

    public UsuarioControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new UsuarioController(_mockMediator.Object);
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
    public async Task ObtenerCuenta_DebeRetornarOkConDatos()
    {
        SimularUsuarioAutenticado("15");
        var cuenta = new UsuarioCuentaDto { UsuarioId = 15, Nombre = "Juan", Email = "juan@test.com" };
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerCuentaQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cuenta);

        var resultado = await _controller.ObtenerCuenta();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(cuenta, okResult.Value);
    }

    [Fact]
    public async Task ActualizarDatos_DatosValidos_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ActualizarDatosDto { Nombre = "Juan Actualizado", Apellido = "Perez" };
        var cuenta = new UsuarioCuentaDto { UsuarioId = 15, Nombre = "Juan Actualizado", Apellido = "Perez" };
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ActualizarDatosCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cuenta);

        var resultado = await _controller.ActualizarDatos(dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(cuenta, okResult.Value);
    }

    [Fact]
    public async Task AscenderRol_DatosValidos_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        var dto = new AscenderRolDto
        {
            NuevoRol = "Dealer",
            NombreAgencia = "Mi Agencia",
            AgenciaRNC = "123456789",
            UbicacionAgencia = "SD",
            TelefonoAgencia = "8090000000"
        };
        var loginResult = new LoginResultDto { Exito = true, Token = "new-jwt-token" };
        _mockMediator
            .Setup(s => s.Send(It.IsAny<AscenderRolCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        var resultado = await _controller.AscenderRol(dto);

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public async Task AscenderRol_ReglaViolada_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new AscenderRolDto { NuevoRol = "Dealer" };
        _mockMediator
            .Setup(s => s.Send(It.IsAny<AscenderRolCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Core.Exceptions.BusinessRuleException("No puedes ascender."));

        var resultado = await _controller.AscenderRol(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task SolicitarCambioPassword_DatosValidos_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        var dto = new SolicitarCambioPasswordDto { PasswordActual = "old123", NuevaPassword = "new456" };
        _mockMediator
            .Setup(s => s.Send(It.IsAny<SolicitarCambioPasswordCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _controller.SolicitarCambioPassword(dto);

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public async Task SolicitarCambioPassword_PasswordIncorrecta_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new SolicitarCambioPasswordDto { PasswordActual = "wrong", NuevaPassword = "new456" };
        _mockMediator
            .Setup(s => s.Send(It.IsAny<SolicitarCambioPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Password incorrecta."));

        var resultado = await _controller.SolicitarCambioPassword(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }
}
