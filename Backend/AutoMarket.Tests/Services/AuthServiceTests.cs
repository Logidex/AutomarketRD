using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(true);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
        mockRepo.Setup(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
        mockRepo.Setup(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        Assert.True(resultado.Exito);
        Assert.Equal("Usuario registrado exitosamente", resultado.Mensaje);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.Is<Usuario>(u =>
            u.Rol == "Dealer" &&
            u.PerfilDealer != null &&
            u.PerfilDealer.NombreAgencia == "AutoMotors RD"
        )), Times.Once);
        mockSuscripcionService.Verify(s => s.AsignarPlanInicialAsync(
            It.IsAny<int>(),
            PlanNivel.Gratis,
            CicloFacturacion.Mensual
        ), Times.Once);
    }

    [Fact]
    public async Task RegistrarUsuarioAsyncDealerConPlanPagadoSeRegistraConPlanGratis()
    {
        var dto = new RegistroDto
        {
            Nombre = "Pedro",
            Apellido = "Lopez",
            Email = "dealerpremium@test.com",
            Password = "MiPasswordSeguro123",
            Rol = "Dealer",
            NombreAgencia = "Premium Motors",
            AgenciaRNC = "130-11111-1",
            UbicacionAgencia = "Santiago",
            TelefonoAgencia = "809-777-7777",
            PlanInicial = "Pro"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockTokenService = new Mock<ITokenService>();
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
        mockRepo.Setup(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => u);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

        var resultado = await servicio.RegistrarUsuarioAsync(dto);

        Assert.True(resultado.Exito);
        mockRepo.Verify(r => r.CrearUsuarioAsync(It.IsAny<Usuario>()), Times.Once);
        mockSuscripcionService.Verify(s => s.AsignarPlanInicialAsync(
            It.IsAny<int>(),
            PlanNivel.Gratis,
            CicloFacturacion.Mensual
        ), Times.Once);
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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.LoginAsync(dto));

        Assert.Equal("Correo electrónico o contraseña incorrectos.", ex.Message);
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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync(usuarioEnBaseDeDatos);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.LoginAsync(dto));

        Assert.Equal("Correo electrónico o contraseña incorrectos.", ex.Message);
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
        var mockSuscripcionService = new Mock<ISuscripcionService>();

        mockRepo.Setup(r => r.ObtenerPorEmailAsync(dto.Email)).ReturnsAsync(usuarioEnBaseDeDatos);
        mockTokenService.Setup(t => t.GenerarToken(usuarioEnBaseDeDatos)).Returns(tokenFalso);

        var servicio = new AuthService(mockRepo.Object, mockTokenService.Object, mockSuscripcionService.Object, new Mock<IEmailSenderService>().Object, Mock.Of<ILogger<AuthService>>());

        var resultado = await servicio.LoginAsync(dto);

        Assert.True(resultado.Exito);
        Assert.Equal("Inicio de sesión exitoso.", resultado.Mensaje);
        Assert.Equal(tokenFalso, resultado.Token);
        mockTokenService.Verify(t => t.GenerarToken(usuarioEnBaseDeDatos), Times.Once);
    }

    // ==========================================
    // RECUPERACIÓN DE CONTRASEÑA
    // ==========================================

    private class RecuperacionContext
    {
        public Mock<IUsuarioRepository> Repo { get; } = new();
        public Mock<IEmailSenderService> EmailSender { get; } = new();
        public Usuario Usuario { get; } = new Usuario(
            "Maria", "Lopez", "maria@test.com",
            BCrypt.Net.BCrypt.HashPassword("ClaveAnterior123"),
            "8090000000", "Comprador");
        public AuthService Servicio { get; }

        public RecuperacionContext()
        {
            typeof(Usuario).GetProperty("UsuarioId")?.SetValue(Usuario, 12);
            Repo.Setup(r => r.ObtenerPorEmailAsync(Usuario.Email)).ReturnsAsync(Usuario);
            Repo.Setup(r => r.ObtenerPorEmailParaEscrituraAsync(Usuario.Email)).ReturnsAsync(Usuario);
            Servicio = new AuthService(
                Repo.Object,
                new Mock<ITokenService>().Object,
                new Mock<ISuscripcionService>().Object,
                EmailSender.Object,
                Mock.Of<ILogger<AuthService>>());
        }
    }

    [Fact]
    public async Task SolicitarRecuperacionAsync_UsuarioValido_GuardaCodigoYEnviaCorreo()
    {
        var ctx = new RecuperacionContext();

        await ctx.Servicio.SolicitarRecuperacionAsync(ctx.Usuario.Email);

        Assert.NotNull(ctx.Usuario.CodigoRecuperacionHash);
        Assert.NotNull(ctx.Usuario.CodigoRecuperacionExpiracionUtc);
        ctx.Repo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        ctx.EmailSender.Verify(e => e.EnviarCorreoAsync(
            "maria@test.com",
            It.Is<string>(s => s.Contains("recuperar") || s.Contains("Recupera")),
            It.Is<string>(c => c.Contains("código") || c.Contains("codigo"))), Times.Once);
    }

    [Fact]
    public async Task SolicitarRecuperacionAsync_EmailNoExiste_NoEnviaCorreoNiGuarda()
    {
        var ctx = new RecuperacionContext();
        ctx.Repo.Setup(r => r.ObtenerPorEmailParaEscrituraAsync("fantasma@test.com")).ReturnsAsync((Usuario?)null);

        await ctx.Servicio.SolicitarRecuperacionAsync("fantasma@test.com");

        ctx.EmailSender.Verify(e => e.EnviarCorreoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarRecuperacionAsync_CuentaSuspendida_NoEnviaCorreo()
    {
        var ctx = new RecuperacionContext();
        typeof(Usuario).GetMethod("Suspender")?.Invoke(ctx.Usuario, null);

        await ctx.Servicio.SolicitarRecuperacionAsync(ctx.Usuario.Email);

        ctx.EmailSender.Verify(e => e.EnviarCorreoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RestablecerPasswordAsync_CodigoCorrecto_CambiaContraseñaYGuarda()
    {
        var ctx = new RecuperacionContext();
        var codigo = "123456";
        ctx.Usuario.EstablecerCodigoRecuperacion(
            AutoMarket.Application.Helpers.CodigoUtil.HashCodigo(codigo),
            DateTime.UtcNow.AddMinutes(15));

        await ctx.Servicio.RestablecerPasswordAsync(new RestablecerPasswordDto
        {
            Email = ctx.Usuario.Email,
            Codigo = codigo,
            NuevaPassword = "NuevaClave456"
        });

        ctx.Repo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        Assert.True(BCrypt.Net.BCrypt.Verify("NuevaClave456", ctx.Usuario.PasswordHash));
        Assert.Null(ctx.Usuario.CodigoRecuperacionHash);
    }

    [Fact]
    public async Task RestablecerPasswordAsync_CodigoIncorrecto_DebeLanzarBusinessRule()
    {
        var ctx = new RecuperacionContext();
        ctx.Usuario.EstablecerCodigoRecuperacion(
            AutoMarket.Application.Helpers.CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(15));

        await Assert.ThrowsAsync<AutoMarket.Core.Exceptions.BusinessRuleException>(() =>
            ctx.Servicio.RestablecerPasswordAsync(new RestablecerPasswordDto
            {
                Email = ctx.Usuario.Email,
                Codigo = "999999",
                NuevaPassword = "NuevaClave456"
            }));
    }

    [Fact]
    public async Task RestablecerPasswordAsync_ContraseñaCorta_DebeLanzarBusinessRule()
    {
        var ctx = new RecuperacionContext();
        ctx.Usuario.EstablecerCodigoRecuperacion(
            AutoMarket.Application.Helpers.CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(15));

        await Assert.ThrowsAsync<AutoMarket.Core.Exceptions.BusinessRuleException>(() =>
            ctx.Servicio.RestablecerPasswordAsync(new RestablecerPasswordDto
            {
                Email = ctx.Usuario.Email,
                Codigo = "123456",
                NuevaPassword = "123"
            }));
    }

    [Fact]
    public async Task RestablecerPasswordAsync_EmailNoRegistrado_DebeLanzarUnauthorized()
    {
        var ctx = new RecuperacionContext();
        ctx.Repo.Setup(r => r.ObtenerPorEmailParaEscrituraAsync("nadie@test.com")).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            ctx.Servicio.RestablecerPasswordAsync(new RestablecerPasswordDto
            {
                Email = "nadie@test.com",
                Codigo = "123456",
                NuevaPassword = "NuevaClave456"
            }));
    }
}