using Moq;
using Xunit;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Core.Entities;

namespace AutoMarket.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegistrarUsuarioAsyncSiEmailYaExisteDebeRetornarFalso()
    {
        var dto = new RegistroDto
        {
            Nombre = "Erick",
            Apellido = "Hipolito",
            Email = "erick@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Comprador"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(true);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        Assert.False(resultado.Exito);
        Assert.Equal("El correo electrónico ya está registrado.", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarUsuarioAsyncDatosValidosCompradorDebeRetornarExito()
    {
        var dto = new RegistroDto
        {
            Nombre = "Juan",
            Apellido = "Perez",
            Email = "nuevo@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Comprador"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
        mockRepo.Setup(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        Assert.True(resultado.Exito);
        Assert.Equal("Usuario registrado exitosamente", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.Is<Usuario>(u =>
            u.Email == dto.Email.ToLowerInvariant() &&
            u.Rol == "Comprador"
        )), Times.Once);
    }

    [Fact]
    public async Task RegistrarUsuarioAsyncDealerFaltanDatosDebeRetornarFalso()
    {
        var dto = new RegistroDto
        {
            Nombre = "Carlos",
            Apellido = "Santana",
            Email = "dealerfalso@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Dealer",
            NombreAgencia = "",
            AgenciaRNC = null
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        Assert.False(resultado.Exito);
        Assert.Equal("Los datos de la agencia y el RNC son obligatorios para cuentas tipo Dealer.", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarUsuarioAsyncDatosValidosDealerDebeRetornarExito()
    {
        var dto = new RegistroDto
        {
            Nombre = "Roberto",
            Apellido = "Gomez",
            Email = "dealerreal@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Dealer",
            NombreAgencia = "AutoMotors RD",
            AgenciaRNC = "130-456789-1",
            UbicacionAgencia = "Santo Domingo",
            TelefonoAgencia = "809-555-5555"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
        mockRepo.Setup(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        Assert.True(resultado.Exito);
        Assert.Equal("Usuario registrado exitosamente", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.Is<Usuario>(u =>
            u.Rol == "Dealer" &&
            u.PerfilDealer != null &&
            u.PerfilDealer.NombreAgencia == "AutoMotors RD"
        )), Times.Once);
    }

    [Fact]
    public async Task LoginAsyncEmailInexistenteDebeLanzarExcepcion()
    {
        var dto = new LoginDto
        {
            Email = "correofantasma@test.com",
            Password = "CualquierPassword123"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.LoginAsync(dto));

        Assert.Equal("Credenciales inválidas.", ex.Message);
        mockTokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsyncPasswordIncorrectoDebeLanzarExcepcion()
    {
        var dto = new LoginDto
        {
            Email = "erick@test.com",
            Password = "ClaveEquivocada"
        };

        var passwordReal = "ClaveVerdadera123";

        var usuarioEnBaseDeDatos = new Usuario(
            nombre: "Erick",
            apellido: "Hipolito",
            email: dto.Email,
            passwordHash: BCrypt.Net.BCrypt.HashPassword(passwordReal),
            telefonoPersonal: "8090000000",
            rol: "Comprador",
            emailConfirmado: true
        );

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync(usuarioEnBaseDeDatos);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.LoginAsync(dto));

        Assert.Equal("Credenciales inválidas.", ex.Message);
        mockTokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsyncCredencialesCorrectasDebeRetornarExitoYToken()
    {
        var passwordCrudo = "ClaveSecreta123";

        var dto = new LoginDto
        {
            Email = "erick@test.com",
            Password = passwordCrudo
        };

        var usuarioEnBaseDeDatos = new Usuario(
            nombre: "Erick",
            apellido: "Hipolito",
            email: dto.Email,
            passwordHash: BCrypt.Net.BCrypt.HashPassword(passwordCrudo),
            telefonoPersonal: "8090000000",
            rol: "Comprador",
            emailConfirmado: true
        );

        var tokenFalso = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.UnTokenFalsoParaPruebas.FirmaFalsa";

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync(usuarioEnBaseDeDatos);
        mockTokenService.Setup(t => t.GenerarToken(usuarioEnBaseDeDatos)).Returns(tokenFalso);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        var resultado = await servicio.LoginAsync(dto);

        Assert.True(resultado.Exito);
        Assert.Equal("Inicio de sesión exitoso.", resultado.Mensaje);
        Assert.Equal(tokenFalso, resultado.Token);
        mockTokenService.Verify(t => t.GenerarToken(usuarioEnBaseDeDatos), Times.Once);
    }
}