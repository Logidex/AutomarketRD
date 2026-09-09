using Xunit;
using Moq;
using MediatR;
using AutoMarket.Application.Features.Auth.Handlers;
using AutoMarket.Application.Features.Auth.Commands;
using AutoMarket.Application.Features.Auth.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Tests.Features.Auth;

public class AuthHandlerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IUsuarioCuentaService> _mockCuentaService;
    private readonly AuthCommandHandler _commandHandler;
    private readonly AuthQueryHandler _queryHandler;

    public AuthHandlerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockCuentaService = new Mock<IUsuarioCuentaService>();
        _commandHandler = new AuthCommandHandler(_mockAuthService.Object, _mockCuentaService.Object);
        _queryHandler = new AuthQueryHandler(_mockCuentaService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_RegistrarUsuario_DebeRetornarExito()
    {
        // Arrange
        var registro = new RegistroDto
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Email = "juan@test.com",
            Password = "password123",
            Rol = "Comprador"
        };
        var command = new RegistrarUsuarioCommand(registro);
        _mockAuthService
            .Setup(s => s.RegistrarUsuarioAsync(registro))
            .ReturnsAsync((true, "Usuario registrado exitosamente"));

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal("Usuario registrado exitosamente", resultado.Mensaje);
        _mockAuthService.Verify(s => s.RegistrarUsuarioAsync(registro), Times.Once);
    }

    [Fact]
    public async Task Handle_Login_DebeRetornarLoginResult()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "juan@test.com", Password = "password123" };
        var command = new LoginCommand(loginDto);
        var expectedResult = new LoginResultDto
        {
            Exito = true,
            Token = "jwt-token-123",
            RefreshToken = "refresh-token-456"
        };
        _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(expectedResult);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Exito);
        Assert.Equal("jwt-token-123", resultado.Token);
        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Handle_RefrescarSesion_DebeRetornarLoginResult()
    {
        // Arrange
        var refreshToken = "refresh-token-456";
        var command = new RefrescarSesionCommand(refreshToken);
        var expectedResult = new LoginResultDto
        {
            Exito = true,
            Token = "new-jwt-token-789",
            RefreshToken = "new-refresh-token-012"
        };
        _mockAuthService.Setup(s => s.RefrescarSesionAsync(refreshToken)).ReturnsAsync(expectedResult);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Exito);
        Assert.Equal("new-jwt-token-789", resultado.Token);
        _mockAuthService.Verify(s => s.RefrescarSesionAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task Handle_Logout_DebeLlamarRevocarSesion()
    {
        // Arrange
        var refreshToken = "refresh-token-456";
        var command = new LogoutCommand(refreshToken);
        _mockAuthService.Setup(s => s.RevocarSesionAsync(refreshToken)).Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthService.Verify(s => s.RevocarSesionAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task Handle_SolicitarRecuperacion_DebeLlamarServicio()
    {
        // Arrange
        var email = "juan@test.com";
        var command = new SolicitarRecuperacionCommand(email);
        _mockAuthService.Setup(s => s.SolicitarRecuperacionAsync(email)).Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthService.Verify(s => s.SolicitarRecuperacionAsync(email), Times.Once);
    }

    [Fact]
    public async Task Handle_RestablecerPassword_DebeLlamarServicio()
    {
        // Arrange
        var datos = new RestablecerPasswordDto
        {
            Email = "juan@test.com",
            NuevaPassword = "newpassword123",
            Codigo = "123456"
        };
        var command = new RestablecerPasswordCommand(datos);
        _mockAuthService.Setup(s => s.RestablecerPasswordAsync(datos)).Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthService.Verify(s => s.RestablecerPasswordAsync(datos), Times.Once);
    }

    [Fact]
    public async Task Handle_ConfirmarCorreo_DebeLlamarServicio()
    {
        // Arrange
        var token = "confirmationToken123";
        var command = new ConfirmarCorreoCommand(token);
        _mockAuthService.Setup(s => s.ConfirmarCorreoAsync(token)).Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthService.Verify(s => s.ConfirmarCorreoAsync(token), Times.Once);
    }

    [Fact]
    public async Task Handle_ReenviarConfirmacion_DebeLlamarServicio()
    {
        // Arrange
        var email = "juan@test.com";
        var command = new ReenviarConfirmacionCommand(email);
        _mockAuthService.Setup(s => s.ReenviarConfirmacionCorreoAsync(email)).Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthService.Verify(s => s.ReenviarConfirmacionCorreoAsync(email), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerCuenta_DebeRetornarUsuarioCuentaDto()
    {
        // Arrange
        var usuarioId = 1;
        var command = new ObtenerCuentaQuery(usuarioId);
        var expectedDto = new UsuarioCuentaDto
        {
            UsuarioId = usuarioId,
            Nombre = "Juan",
            Apellido = "Perez",
            Email = "juan@test.com",
            EmailConfirmado = true,
            Rol = "Comprador"
        };
        _mockCuentaService.Setup(s => s.ObtenerCuentaAsync(usuarioId)).ReturnsAsync(expectedDto);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal("Juan", resultado.Nombre);
        Assert.Equal("Perez", resultado.Apellido);
        Assert.Equal("juan@test.com", resultado.Email);
        Assert.True(resultado.EmailConfirmado);
        _mockCuentaService.Verify(s => s.ObtenerCuentaAsync(usuarioId), Times.Once);
    }
}
