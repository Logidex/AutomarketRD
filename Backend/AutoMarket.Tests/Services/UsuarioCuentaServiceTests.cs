using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
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
    private readonly Mock<ISuscripcionService> _mockSuscripcion;
    private readonly Mock<ITokenService> _mockToken;
    private readonly Mock<IEmailSenderService> _mockEmailSender;
    private readonly Mock<ILogger<UsuarioCuentaService>> _mockLogger;
    private readonly UsuarioCuentaService _service;

    public UsuarioCuentaServiceTests()
    {
        _mockRepo = new Mock<IUsuarioRepository>();
        _mockSuscripcion = new Mock<ISuscripcionService>();
        _mockToken = new Mock<ITokenService>();
        _mockEmailSender = new Mock<IEmailSenderService>();
        _mockLogger = new Mock<ILogger<UsuarioCuentaService>>();
        _service = new UsuarioCuentaService(
            _mockRepo.Object,
            _mockSuscripcion.Object,
            _mockToken.Object,
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
    public async Task SolicitarCambioPasswordAsync_PasswordActualIncorrecta_DebeLanzarUnauthorized()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new SolicitarCambioPasswordDto { PasswordActual = "OtraClave", NuevaPassword = "NuevaClave123" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SolicitarCambioPasswordAsync(7, dto));
    }

    [Fact]
    public async Task SolicitarCambioPasswordAsync_DatosValidos_GuardaPasswordPendienteYEnviaCodigo()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new SolicitarCambioPasswordDto { PasswordActual = "ClaveCorrecta1", NuevaPassword = "NuevaClave123" };

        await _service.SolicitarCambioPasswordAsync(7, dto);

        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        Assert.NotNull(usuario.PasswordPendienteHash);
        Assert.NotNull(usuario.CodigoPasswordHash);
        Assert.NotNull(usuario.CodigoPasswordExpiracionUtc);
        // La contraseña NO cambia hasta confirmar
        Assert.True(BCrypt.Net.BCrypt.Verify("ClaveCorrecta1", usuario.PasswordHash));
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            "juan@test.com",
            It.Is<string>(s => s.Contains("contraseña")),
            It.Is<string>(c => c.Contains("código") || c.Contains("codigo"))), Times.Once);
    }

    [Fact]
    public async Task SolicitarCambioPasswordAsync_NuevaPasswordIgualActual_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario(passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta1"));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new SolicitarCambioPasswordDto { PasswordActual = "ClaveCorrecta1", NuevaPassword = "ClaveCorrecta1" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.SolicitarCambioPasswordAsync(7, dto));
    }

    [Fact]
    public async Task ConfirmarCambioPasswordAsync_CodigoIncorrecto_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        usuario.EstablecerCambioPassword(
            BCrypt.Net.BCrypt.HashPassword("NuevaClave123"),
            CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(15));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ConfirmarPasswordDto { Codigo = "999999" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmarCambioPasswordAsync(7, dto));
        Assert.NotNull(usuario.PasswordPendienteHash);
    }

    [Fact]
    public async Task ConfirmarCambioPasswordAsync_CodigoExpirado_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        usuario.EstablecerCambioPassword(
            BCrypt.Net.BCrypt.HashPassword("NuevaClave123"),
            CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(-1));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ConfirmarPasswordDto { Codigo = "123456" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ConfirmarCambioPasswordAsync(7, dto));
        Assert.NotNull(usuario.PasswordPendienteHash);
    }

    [Fact]
    public async Task ConfirmarCambioPasswordAsync_CodigoCorrecto_AplicaNuevaContraseña()
    {
        var usuario = CrearUsuario();
        var nuevoHash = BCrypt.Net.BCrypt.HashPassword("NuevaClave123");
        usuario.EstablecerCambioPassword(
            nuevoHash,
            CodigoUtil.HashCodigo("123456"),
            DateTime.UtcNow.AddMinutes(15));
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new ConfirmarPasswordDto { Codigo = "123456" };

        await _service.ConfirmarCambioPasswordAsync(7, dto);

        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
        Assert.Equal(nuevoHash, usuario.PasswordHash);
        Assert.Null(usuario.PasswordPendienteHash);
        Assert.Null(usuario.CodigoPasswordHash);
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync("juan@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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

    [Fact]
    public async Task AscenderRolAsync_HaciaVendedor_CambiaRolYDevuelveToken()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);
        _mockToken.Setup(t => t.GenerarToken(usuario)).Returns("token-nuevo");

        var resultado = await _service.AscenderRolAsync(7, new AscenderRolDto { NuevoRol = "Vendedor" });

        Assert.Equal("Vendedor", usuario.Rol);
        Assert.Equal("token-nuevo", resultado.Token);
        Assert.Equal("Vendedor", resultado.Usuario!.Rol);
        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task AscenderRolAsync_HaciaDealer_CreaPerfilYAsignaPlanGratis()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);
        _mockToken.Setup(t => t.GenerarToken(It.IsAny<Usuario>())).Returns("token-dealer");

        var dto = new AscenderRolDto
        {
            NuevoRol = "Dealer",
            NombreAgencia = "AutoLux RD",
            AgenciaRNC = "1-30-12345-6",
            UbicacionAgencia = "Santo Domingo",
            TelefonoAgencia = "8095551234"
        };

        var resultado = await _service.AscenderRolAsync(7, dto);

        Assert.Equal("Dealer", usuario.Rol);
        Assert.NotNull(usuario.PerfilDealer);
        Assert.Equal("AutoLux RD", usuario.PerfilDealer!.NombreAgencia);
        _mockSuscripcion.Verify(s => s.AsignarPlanInicialAsync(It.IsAny<int>(), It.IsAny<AutoMarket.Core.Entities.Enums.PlanNivel>(), It.IsAny<AutoMarket.Core.Entities.Enums.CicloFacturacion>()), Times.Once);
        Assert.Equal("token-dealer", resultado.Token);
    }

    [Fact]
    public async Task AscenderRolAsync_HaciaDealer_SinDatosAgencia_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AscenderRolDto { NuevoRol = "Dealer" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AscenderRolAsync(7, dto));
        Assert.Equal("Comprador", usuario.Rol);
    }

    [Fact]
    public async Task AscenderRolAsync_RolInvalido_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AscenderRolDto { NuevoRol = "Admin" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AscenderRolAsync(7, dto));
        Assert.Equal("Comprador", usuario.Rol);
    }

    // ==========================================
    // CAMBIO DE ROL POR ADMINISTRADOR
    // ==========================================

    private Usuario CrearDealer()
    {
        var usuario = new Usuario("Luis", "Rodriguez", "luis@test.com", "hashDePrueba", "8091112222", "Comprador");
        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(usuario, 7);
        usuario.ConvertirADealer("AutoLux RD", "1-30-12345-6", "Santo Domingo", "8095551234");
        return usuario;
    }

    [Fact]
    public async Task CambiarRolAdminAsync_DealerAComprador_EliminaPerfilYCambiaRol()
    {
        var usuario = CrearDealer();
        _mockRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto { NuevoRol = "Comprador" };

        var cuenta = await _service.CambiarRolAdminAsync(7, dto);

        Assert.Equal("Comprador", cuenta.Rol);
        Assert.Null(usuario.PerfilDealer);
        _mockRepo.Verify(r => r.EliminarPerfilDealerAsync(7), Times.Once);
        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task CambiarRolAdminAsync_CompradorAVendedor_CambiaRolSinPerfil()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto { NuevoRol = "Vendedor" };

        var cuenta = await _service.CambiarRolAdminAsync(7, dto);

        Assert.Equal("Vendedor", cuenta.Rol);
        _mockRepo.Verify(r => r.EliminarPerfilDealerAsync(It.IsAny<int>()), Times.Never);
        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task CambiarRolAdminAsync_CompradorADealer_CreaPerfilYAsignaPlanGratis()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto
        {
            NuevoRol = "Dealer",
            NombreAgencia = "Nueva Agencia",
            AgenciaRNC = "1-30-99999-9",
            UbicacionAgencia = "Santiago",
            TelefonoAgencia = "8090000000"
        };

        var cuenta = await _service.CambiarRolAdminAsync(7, dto);

        Assert.Equal("Dealer", cuenta.Rol);
        Assert.NotNull(usuario.PerfilDealer);
        _mockSuscripcion.Verify(s => s.AsignarPlanInicialAsync(It.IsAny<int>(), It.IsAny<AutoMarket.Core.Entities.Enums.PlanNivel>(), It.IsAny<AutoMarket.Core.Entities.Enums.CicloFacturacion>()), Times.Once);
    }

    [Fact]
    public async Task CambiarRolAdminAsync_ADealer_SinDatosAgencia_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto { NuevoRol = "Dealer" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CambiarRolAdminAsync(7, dto));
        Assert.Equal("Comprador", usuario.Rol);
    }

    [Fact]
    public async Task CambiarRolAdminAsync_RolInvalido_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto { NuevoRol = "Admin" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CambiarRolAdminAsync(7, dto));
    }

    [Fact]
    public async Task CambiarRolAdminAsync_MismoRol_DebeLanzarBusinessRule()
    {
        var usuario = CrearUsuario();
        _mockRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(7)).ReturnsAsync(usuario);

        var dto = new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto { NuevoRol = "Comprador" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CambiarRolAdminAsync(7, dto));
    }
}