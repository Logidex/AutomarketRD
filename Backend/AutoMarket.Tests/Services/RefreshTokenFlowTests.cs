using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

/// <summary>
/// Flujo de refresh tokens: emisión en login, rotación, reuso detectado
/// y revocación en logout.
/// </summary>
public class RefreshTokenFlowTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();

    private AuthService CrearServicio()
    {
        return new AuthService(
            _repo.Object,
            _tokenService.Object,
            new Mock<ISuscripcionService>().Object,
            new Mock<IEmailSenderService>().Object,
            Mock.Of<IConfiguration>(),
            Mock.Of<ILogger<AuthService>>(),
            _refreshTokens.Object);
    }

    private static Usuario CrearUsuario(int id = 7)
    {
        var usuario = new Usuario(
            "Juan", "Perez", "juan@test.com",
            BCrypt.Net.BCrypt.HashPassword("Clave12345"),
            telefonoPersonal: null,
            rol: "Comprador");

        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(usuario, id);
        return usuario;
    }

    [Fact]
    public async Task LoginAsync_EmiteYGuardaRefreshToken()
    {
        var usuario = CrearUsuario();
        const string crudo = "refresh-crudo";

        _repo.Setup(r => r.ObtenerPorEmailAsync("juan@test.com")).ReturnsAsync(usuario);
        _repo.Setup(r => r.ObtenerPorEmailParaEscrituraAsync("juan@test.com")).ReturnsAsync(usuario);
        _tokenService.Setup(t => t.GenerarToken(usuario)).Returns("jwt");
        _tokenService.Setup(t => t.GenerarRefreshToken()).Returns(crudo);
        _tokenService.Setup(t => t.HashRefreshToken(crudo)).Returns("HASH-CRUDO");

        var servicio = CrearServicio();

        var resultado = await servicio.LoginAsync(
            new LoginDto { Email = "juan@test.com", Password = "Clave12345" });

        Assert.Equal(crudo, resultado.RefreshToken);
        _refreshTokens.Verify(r => r.AgregarAsync(It.IsAny<RefreshToken>()), Times.Once);
        _refreshTokens.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task RefrescarSesionAsync_TokenValido_RotaYEmiteNuevoAcceso()
    {
        var usuario = CrearUsuario();
        var activo = new RefreshToken(7, "HASH-VIEJO", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddDays(14));

        _tokenService.Setup(t => t.HashRefreshToken("crudo-viejo")).Returns("HASH-VIEJO");
        _tokenService.Setup(t => t.GenerarRefreshToken()).Returns("crudo-nuevo");
        _tokenService.Setup(t => t.HashRefreshToken("crudo-nuevo")).Returns("HASH-NUEVO");
        _tokenService.Setup(t => t.GenerarToken(usuario)).Returns("jwt-nuevo");
        _refreshTokens.Setup(r => r.ObtenerPorHashAsync("HASH-VIEJO")).ReturnsAsync(activo);
        _repo.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(usuario);

        var resultado = await CrearServicio().RefrescarSesionAsync("crudo-viejo");

        Assert.Equal("jwt-nuevo", resultado.Token);
        Assert.Equal("crudo-nuevo", resultado.RefreshToken);
        Assert.True(activo.FueRotado);
        Assert.Equal("HASH-NUEVO", activo.ReplacedByTokenHash);
        _refreshTokens.Verify(r => r.RevocarActivosDeUsuarioAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RefrescarSesionAsync_HashDesconocido_LanzaUnauthorized()
    {
        _tokenService.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("HASH-INEXISTENTE");
        _refreshTokens.Setup(r => r.ObtenerPorHashAsync("HASH-INEXISTENTE")).ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CrearServicio().RefrescarSesionAsync("lo-que-sea"));
    }

    [Fact]
    public async Task RefrescarSesionAsync_ReusoDeTokenRotado_RevocaTodaLaFamilia()
    {
        var rotado = new RefreshToken(7, "HASH-USADO", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(13));
        rotado.Revocar("HASH-REEMPLAZO");

        _tokenService.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("HASH-USADO");
        _refreshTokens.Setup(r => r.ObtenerPorHashAsync("HASH-USADO")).ReturnsAsync(rotado);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CrearServicio().RefrescarSesionAsync("reusado"));

        // Contención: se revocan los tokens aún activos del usuario
        _refreshTokens.Verify(r => r.RevocarActivosDeUsuarioAsync(7), Times.Once);
        _refreshTokens.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task RefrescarSesionAsync_TokenExpirado_LanzaUnauthorized()
    {
        var expirado = new RefreshToken(7, "HASH-VENCIDO", DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-1));

        _tokenService.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("HASH-VENCIDO");
        _refreshTokens.Setup(r => r.ObtenerPorHashAsync("HASH-VENCIDO")).ReturnsAsync(expirado);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CrearServicio().RefrescarSesionAsync("vencido"));
    }

    [Fact]
    public async Task RevocarSesionAsync_TokenActivo_LoRevoca()
    {
        var activo = new RefreshToken(7, "HASH-ACTIVO", DateTime.UtcNow, DateTime.UtcNow.AddDays(14));

        _tokenService.Setup(t => t.HashRefreshToken("crudo-logout")).Returns("HASH-ACTIVO");
        _refreshTokens.Setup(r => r.ObtenerPorHashAsync("HASH-ACTIVO")).ReturnsAsync(activo);

        await CrearServicio().RevocarSesionAsync("crudo-logout");

        Assert.False(activo.EstaActivo);
        _refreshTokens.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }
}
