using AutoMarket.API.Helpers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;
    private readonly DefaultHttpContext _httpContext;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();

        _controller = new AuthController(_mockAuthService.Object);

        _httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
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
        var tipo = ok.Value!.GetType();
        var exito = (bool)tipo.GetProperty("exito")!.GetValue(ok.Value)!;
        var mensaje = tipo.GetProperty("mensaje")!.GetValue(ok.Value)!.ToString();

        Assert.True(exito);
        Assert.Equal("Usuario registrado exitosamente", mensaje);

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
        var tipo = badRequest.Value!.GetType();
        var mensaje = tipo.GetProperty("mensaje")!.GetValue(badRequest.Value)!.ToString();

        Assert.Equal("El correo electrónico ya está registrado.", mensaje);

        _mockAuthService.Verify(s => s.RegistrarUsuarioAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Login_CuandoCredencialesSonValidas_DebeEstablecerCookieHttpOnlyYNoDevolverToken()
    {
        var dto = new LoginDto
        {
            Email = "juan@test.com",
            Password = "123456"
        };

        _mockAuthService
            .Setup(s => s.LoginAsync(dto))
            .ReturnsAsync(new LoginResultDto
            {
                Exito = true,
                Mensaje = "Inicio de sesión exitoso.",
                Token = "token-jwt-demo",
                RefreshToken = "refresh-token-demo",
                Usuario = new UsuarioAuthDto
                {
                    UsuarioId = 1,
                    Nombre = "Juan",
                    Email = "juan@test.com",
                    Rol = "Comprador"
                }
            });

        var resultado = await _controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(ok.Value);

        var tipo = ok.Value!.GetType();
        var mensaje = tipo.GetProperty("Mensaje")?.GetValue(ok.Value)?.ToString();

        Assert.Equal("Inicio de sesión exitoso.", mensaje);

        // El token no debe viajar en el cuerpo de la respuesta
        Assert.Null(tipo.GetProperty("Token"));

        // El token viaja como cookie HttpOnly
        var setCookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains(
            AuthCookieHelper.CookieName,
            setCookie,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "HttpOnly",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "SameSite=Lax",
            setCookie,
            StringComparison.OrdinalIgnoreCase);

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
            .ReturnsAsync(new LoginResultDto
            {
                Exito = false,
                Mensaje = "Credenciales incorrectas.",
                Token = null
            });

        var resultado = await _controller.Login(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);

        var propMensaje = badRequest.Value
            ?.GetType()
            .GetProperty("mensaje")
            ?.GetValue(badRequest.Value, null)
            ?.ToString();
        Assert.Equal("Credenciales incorrectas.", propMensaje);

        _mockAuthService.Verify(s => s.LoginAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Logout_DebeEliminarLaCookieDelToken()
    {
        var resultado = await _controller.Logout();

        Assert.IsType<OkObjectResult>(resultado);

        var setCookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains(
            AuthCookieHelper.CookieName,
            setCookie,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "expires=",
            setCookie,
            StringComparison.OrdinalIgnoreCase);
    }
}