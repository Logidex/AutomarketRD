using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Registrar_CuandoRegistroEsExitoso_DebeRetornarOkConMensaje()
    {
        var dto = new RegistroDto
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Email = "juan@test.com",
            Password = "123456",
            Rol = "Particular",
            TelefonoPersonal = "8090000000"
        };

        _mockAuthService
            .Setup(s => s.RegistrarUsuarioAsync(dto))
            .ReturnsAsync((true, "Usuario registrado exitosamente"));

        var resultado = await _controller.Registrar(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Usuario registrado exitosamente", ok.Value);

        _mockAuthService.Verify(s => s.RegistrarUsuarioAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Registrar_CuandoRegistroFalla_DebeRetornarBadRequestConMensaje()
    {
        var dto = new RegistroDto
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Email = "juan@test.com",
            Password = "123456",
            Rol = "Particular",
            TelefonoPersonal = "8090000000"
        };

        _mockAuthService
            .Setup(s => s.RegistrarUsuarioAsync(dto))
            .ReturnsAsync((false, "El correo electrónico ya está registrado."));

        var resultado = await _controller.Registrar(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("El correo electrónico ya está registrado.", badRequest.Value);

        _mockAuthService.Verify(s => s.RegistrarUsuarioAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Login_CuandoCredencialesSonValidas_DebeRetornarOkConMensajeYToken()
    {
        var dto = new LoginDto
        {
            Email = "juan@test.com",
            Password = "123456"
        };

        _mockAuthService
            .Setup(s => s.LoginAsync(dto))
            .ReturnsAsync((true, "Inicio de sesión exitoso.", "token-jwt-demo"));

        var resultado = await _controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(ok.Value);

        var tipo = ok.Value!.GetType();
        var mensaje = tipo.GetProperty("Mensaje")?.GetValue(ok.Value)?.ToString();
        var token = tipo.GetProperty("Token")?.GetValue(ok.Value)?.ToString();

        Assert.Equal("Inicio de sesión exitoso.", mensaje);
        Assert.Equal("token-jwt-demo", token);

        _mockAuthService.Verify(s => s.LoginAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Login_CuandoLoginFalla_DebeRetornarBadRequestConMensaje()
    {
        var dto = new LoginDto
        {
            Email = "juan@test.com",
            Password = "incorrecta"
        };

        _mockAuthService
            .Setup(s => s.LoginAsync(dto))
            .ReturnsAsync((false, "Credenciales incorrectas.", null));

        var resultado = await _controller.Login(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Credenciales incorrectas.", badRequest.Value);

        _mockAuthService.Verify(s => s.LoginAsync(dto), Times.Once);
    }
}