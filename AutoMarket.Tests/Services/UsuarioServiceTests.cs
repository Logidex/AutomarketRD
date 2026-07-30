using Moq;
using Xunit;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Core.Entities;

namespace AutoMarket.Tests.Services;

public class UsuarioServiceTests
{
    [Fact]
    public async Task RegistrarUsuarioAsync_SiEmailYaExiste_DebeRetornarFalso()
    {
        // 1. ARRANGE
        var dto = new RegistroDto
        {
            Nombre = "Erick",
            Apellido = "Hipolito",
            Email = "erick@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Comprador"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(true);
        var mockTokenService = new Mock<ITokenService>();

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2. ACT
        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        // 3. ASSERT
        Assert.False(resultado.Exito);
        Assert.Equal("El correo electrónico ya está registrado.", resultado.Mensaje);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DatosValidosComprador_DebeRetornarExito()
    {
        // 1. ARRANGE
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
        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2. ACT
        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        // 3. ASSERT
        Assert.True(resultado.Exito);
        Assert.Equal("Usuario registrado exitosamente", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DealerFaltanDatos_DebeRetornarFalso()
    {
        // 1. ARRANGE
        var dto = new RegistroDto
        {
            Nombre = "Carlos",
            Apellido = "Santana",
            Email = "dealer_falso@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Dealer",
            NombreAgencia = "", 
            AgenciaRNC = null   
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2. ACT
        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        // 3. ASSERT
        Assert.False(resultado.Exito);
        Assert.Equal("Los datos de la agencia y el RNC son obligatorios para cuentas tipo Dealer.", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DatosValidosDealer_DebeRetornarExito()
    {
        // 1. ARRANGE
        var dto = new RegistroDto
        {
            Nombre = "Roberto",
            Apellido = "Gomez",
            Email = "dealer_real@test.com",
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
        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2. ACT
        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        // 3. ASSERT
        Assert.True(resultado.Exito);
        Assert.Equal("Usuario registrado exitosamente", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.Is<Usuario>(u =>
            u.Rol == "Dealer" &&
            u.PerfilDealer != null &&
            u.PerfilDealer.NombreAgencia == "AutoMotors RD"
        )), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_EmailInexistente_DebeLanzarExcepcion()
    {
        // 1. ARRANGE
        var dto = new LoginDto
        {
            Email = "correo_fantasma@test.com",
            Password = "CualquierPassword123"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);
        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2 & 3. ACT & ASSERT
        // Validamos que el método lance la excepción esperada
        var excepcion = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.LoginAsync(dto));
        
        Assert.Equal("Credenciales inválidas.", excepcion.Message);
        
        // Nos aseguramos de que el sistema NUNCA intentó generar un token
        mockTokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_PasswordIncorrecto_DebeLanzarExcepcion()
    {
        // 1. ARRANGE
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
            rol: "Comprador",
            telefonoPersonal: null
        );

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync(usuarioEnBaseDeDatos);
        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2 & 3. ACT & ASSERT
        // Validamos que el método lance la excepción esperada por la clave incorrecta
        var excepcion = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.LoginAsync(dto));
        
        Assert.Equal("Credenciales inválidas.", excepcion.Message);
        
        // Nos aseguramos de que el sistema NUNCA intentó generar un token
        mockTokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_CredencialesCorrectas_DebeRetornarExitoYToken()
    {
        // 1. ARRANGE
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
            rol: "Comprador",
            telefonoPersonal: null
        );

        var tokenFalso = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.UnTokenFalsoParaPruebas.FirmaFalsa";
        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync(usuarioEnBaseDeDatos);
        mockTokenService.Setup(t => t.GenerarToken(It.IsAny<Usuario>())).Returns(tokenFalso);
        
        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object);

        // 2. ACT
        var resultado = await servicio.LoginAsync(dto);

        // 3. ASSERT
        Assert.True(resultado.Exito);
        Assert.Equal("Inicio de sesión exitoso.", resultado.Mensaje);
        Assert.Equal(tokenFalso, resultado.Token);
        mockTokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Once);
    }
}