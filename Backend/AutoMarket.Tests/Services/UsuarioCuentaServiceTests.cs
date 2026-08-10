using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class UsuarioCuentaServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepo;
    private readonly Mock<IEmailSenderService> _mockEmailSender;
    private readonly Mock<ILogger<UsuarioCuentaService>> _mockLogger;
    private readonly UsuarioCuentaService _service;

    public UsuarioCuentaServiceTests()
    {
        _mockRepo = new Mock<IUsuarioRepository>();
        _mockEmailSender = new Mock<IEmailSenderService>();
        _mockLogger = new Mock<ILogger<UsuarioCuentaService>>();
        _service = new UsuarioCuentaService(
            _mockRepo.Object,
            _mockEmailSender.Object,
            _mockLogger.Object);
    }

    private Usuario CrearUsuario(string passwordHash = "hashDePrueba")
    {
        var usuario = new Usuario("Juan", "Perez", "juan@test.com", passwordHash, "8090000000", "Comprador");
        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(usuario, 7);
        return usuario;
    }

    [Fact]
    public async Task ObtenerCuentaAsync_DebeMapearDatos()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var cuenta = await _service.ObtenerCuentaAsync(7);

        Assert.Equal("Juan", cuenta.Nombre);
        Assert.Equal("Perez", cuenta.Apellido);
        Assert.Equal("juan@test.com", cuenta.Email);
        Assert.Equal("8090000000", cuenta.TelefonoPersonal);
        Assert.Equal("Comprador", cuenta.Rol);
    }

    [Fact]
    public async Task ObtenerCuentaAsync_UsuarioNoExiste_DebeLanzarKeyNotFound()
    {
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(99)).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObtenerCuentaAsync(99));
    }

    [Fact]
    public async Task ActualizarDatosAsync_DebeActualizarNombreApellidoYTelefono()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ActualizarDatosDto
        {
            Nombre = "Juana",
            Apellido = "Gomez",
            TelefonoPersonal = "8091234567"
        };

        var cuenta = await _service.ActualizarDatosAsync(7, dto);

        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        Assert.Equal("Juana", cuenta.Nombre);
        Assert.Equal("Gomez", cuenta.Apellido);
        Assert.Equal("8091234567", cuenta.TelefonoPersonal);
    }

    [Fact]
    public async Task CambiarPasswordAsync_PasswordActualIncorrecta_DebeLanzarUnauthorized()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new CambiarPasswordDto { PasswordActual = "OtraClave", NuevaPassword = "NuevaClave123" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CambiarPasswordAsync(7, dto));
    }

    [Fact]
    public async Task CambiarPasswordAsync_DatosValidos_DebeActualizarYNotificar()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new CambiarPasswordDto { PasswordActual = "ClaveCorrecta1", NuevaPassword = "NuevaClave123" };

        await _service.CambiarPasswordAsync(7, dto);

        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            "juan@test.com",
            It.Is<string>(s => s.Contains("contraseña")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CambiarPasswordAsync_NuevaPasswordIgualActual_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new CambiarPasswordDto { PasswordActual = "ClaveCorrecta1", NuevaPassword = "ClaveCorrecta1" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CambiarPasswordAsync(7, dto));
    }

    [Fact]
    public async Task SolicitarCambioEmailAsync_PasswordIncorrecta_DebeLanzarUnauthorized()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new SolicitarCambioEmailDto { PasswordActual = "Mala", NuevoEmail = "nuevo@test.com" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SolicitarCambioEmailAsync(7, dto));
    }

    [Fact]
    public async Task SolicitarCambioEmailAsync_EmailYaRegistrado_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);
        _mockRepo.Setup(r => r.ExisteEmailAsync("ocupado@test.com")).ReturnsAsync(true);

        var dto = new SolicitarCambioEmailDto { PasswordActual = "ClaveCorrecta1", NuevoEmail = "ocupado@test.com" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.SolicitarCambioEmailAsync(7, dto));
    }

    [Fact]
    public async Task SolicitarCambioEmailAsync_Valido_GuardaEmailPendienteYEnviaCodigoAlNuevoCorreo()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);
        _mockRepo.Setup(r => r.ExisteEmailAsync("nuevo@test.com")).ReturnsAsync(false);

        var dto = new SolicitarCambioEmailDto { PasswordActual = "ClaveCorrecta1", NuevoEmail = "nuevo@test.com" };

        await _service.SolicitarCambioEmailAsync(7, dto);

        Assert.Equal("nuevo@test.com", usuario.EmailPendiente);
        Assert.NotNull(usuario.CodigoConfirmacionHash);
        Assert.NotNull(usuario.CodigoConfirmacionExpiracionUtc);
        Assert.Equal("juan@test.com", usuario.Email); // El correo aún NO cambia
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            "nuevo@test.com",
            It.IsAny<string>(),
            It.Is<string>(c => c.Contains("código") || c.Contains("codigo"))), Times.Once);
    }

    [Fact]
    public async Task ConfirmarCambioEmailAsync_CodigoIncorrecto_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        usuario.EstablecerCambioEmail(
            "nuevo@test.com",
            CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(15));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ConfirmarEmailDto { Codigo = "999999" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmarCambioEmailAsync(7, dto));
        Assert.Equal("juan@test.com", usuario.Email);
    }

    [Fact]
    public async Task ConfirmarCambioEmailAsync_CodigoExpirado_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        usuario.EstablecerCambioEmail(
            "nuevo@test.com",
            CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(-1));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ConfirmarEmailDto { Codigo = "123456" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmarCambioEmailAsync(7, dto));
        Assert.Equal("juan@test.com", usuario.Email);
    }

    [Fact]
    public async Task ConfirmarCambioEmailAsync_CodigoCorrecto_AplicaEmailYConfirma()
    {
        var usuario = CrearUsuario();
        usuario.EstablecerCambioEmail(
            "nuevo@test.com",
            CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(15));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ConfirmarEmailDto { Codigo = "123456" };

        await _service.ConfirmarCambioEmailAsync(7, dto);

        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        Assert.Equal("nuevo@test.com", usuario.Email);
        Assert.True(usuario.EmailConfirmado);
        Assert.Null(usuario.EmailPendiente);
        Assert.Null(usuario.CodigoConfirmacionHash);
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync("nuevo@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}