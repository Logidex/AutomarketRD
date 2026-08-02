using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class PerfilDealerServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IAlmacenadorArchivos> _almacenadorArchivosMock;
    private readonly PerfilDealerService _service;

    public PerfilDealerServiceTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _almacenadorArchivosMock = new Mock<IAlmacenadorArchivos>();

        _service = new PerfilDealerService(
            _usuarioRepositoryMock.Object,
            _almacenadorArchivosMock.Object);
    }

    [Fact]
    public async Task ActualizarMiPerfilAsync_DealerExistenteSinLogo_ActualizaPerfilYGuardaCambios()
    {
        // Arrange
        const int dealerId = 1;

        var dealer = new Usuario(
            nombre: "Erick",
            apellido: "Perez",
            email: "erick@prueba.com",
            passwordHash: "hash-de-prueba",
            telefonoPersonal: "8095550000",
            rol: "Dealer"
        );

        dealer.CrearPerfilDealer(
            nombreAgencia: "Agencia anterior",
            agenciaRNC: "131000001",
            ubicacion: "Santo Domingo",
            telefonoAgencia: "8095551111",
            descripcion: "Descripción anterior",
            whatsApp: "18095551111"
        );

        var dto = new PerfilDealerUpdateDto
        {
            NombreAgencia = "Auto Erick RD",
            Ubicacion = "Santo Domingo Este",
            TelefonoAgencia = "8095551234",
            Horarios = "Lunes a sábado, 9:00 AM - 6:00 PM",
            Descripcion = "Vehículos usados seleccionados",
            WhatsApp = "18095551234",
            Logo = null
        };

        _usuarioRepositoryMock
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(dealerId))
            .ReturnsAsync(dealer);

        _usuarioRepositoryMock
            .Setup(r => r.GuardarCambiosAsync())
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.ActualizarMiPerfilAsync(dealerId, dto);

        // Assert: respuesta
        Assert.NotNull(resultado);
        Assert.Equal("Auto Erick RD", resultado.NombreAgencia);
        Assert.Equal("Santo Domingo Este", resultado.Ubicacion);
        Assert.Equal("8095551234", resultado.TelefonoAgencia);
        Assert.Equal("Lunes a sábado, 9:00 AM - 6:00 PM", resultado.Horarios);
        Assert.Equal("Vehículos usados seleccionados", resultado.Descripcion);
        Assert.Equal("18095551234", resultado.WhatsApp);
        Assert.Null(resultado.LogoUrl);

        // Assert: entidad de dominio actualizada
        Assert.NotNull(dealer.PerfilDealer);
        Assert.Equal("Auto Erick RD", dealer.PerfilDealer.NombreAgencia);
        Assert.Equal("Santo Domingo Este", dealer.PerfilDealer.Ubicacion);
        Assert.Equal("8095551234", dealer.PerfilDealer.TelefonoAgencia);
        Assert.Equal("Lunes a sábado, 9:00 AM - 6:00 PM", dealer.PerfilDealer.Horarios);
        Assert.Equal("Vehículos usados seleccionados", dealer.PerfilDealer.Descripcion);
        Assert.Equal("18095551234", dealer.PerfilDealer.WhatsApp);

        // Assert: persistencia y almacenamiento
        _usuarioRepositoryMock.Verify(
            r => r.GuardarCambiosAsync(),
            Times.Once);

        _almacenadorArchivosMock.Verify(
            a => a.GuardarArchivoAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ActualizarMiPerfilAsync_DealerInexistente_RetornaNullYNoGuardaCambios()
    {
        // Arrange
        const int dealerId = 999;

        var dto = new PerfilDealerUpdateDto
        {
            NombreAgencia = "Auto Erick RD",
            Ubicacion = "Santo Domingo Este",
            TelefonoAgencia = "8095551234"
        };

        _usuarioRepositoryMock
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(dealerId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _service.ActualizarMiPerfilAsync(dealerId, dto);

        // Assert
        Assert.Null(resultado);

        _usuarioRepositoryMock.Verify(
            r => r.GuardarCambiosAsync(),
            Times.Never);

        _almacenadorArchivosMock.Verify(
            a => a.GuardarArchivoAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    private static Usuario CrearDealerConPerfil()
    {
        var dealer = new Usuario(
            nombre: "Erick",
            apellido: "Perez",
            email: "erick@prueba.com",
            passwordHash: "hash-de-prueba",
            telefonoPersonal: "8095550000",
            rol: "Dealer"
        );

        dealer.CrearPerfilDealer(
            nombreAgencia: "Agencia anterior",
            agenciaRNC: "131000001",
            ubicacion: "Santo Domingo",
            telefonoAgencia: "8095551111",
            descripcion: "Descripción anterior",
            whatsApp: "18095551111"
        );

        return dealer;
    }

    [Fact]
    public async Task ActualizarMiPerfilAsync_LogoValido_GuardaLogoYRetornaRuta()
    {
        // Arrange
        const int dealerId = 1;
        const string rutaLogo = "https://bucket.s3.amazonaws.com/logos/logo-dealer.png";

        var dealer = CrearDealerConPerfil();

        var contenido = new byte[] { 1, 2, 3, 4, 5 };
        await using var stream = new MemoryStream(contenido);

        IFormFile logo = new FormFile(
            baseStream: stream,
            baseStreamOffset: 0,
            length: stream.Length,
            name: "Logo",
            fileName: "logo-dealer.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var dto = new PerfilDealerUpdateDto
        {
            NombreAgencia = "Auto Erick RD",
            Ubicacion = "Santo Domingo Este",
            TelefonoAgencia = "8095551234",
            Horarios = "Lunes a sábado, 9:00 AM - 6:00 PM",
            Descripcion = "Vehículos usados seleccionados",
            WhatsApp = "18095551234",
            Logo = logo
        };

        _usuarioRepositoryMock
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(dealerId))
            .ReturnsAsync(dealer);

        _almacenadorArchivosMock
            .Setup(a => a.GuardarArchivoAsync(
                It.IsAny<Stream>(),
                "logo-dealer.png",
                "image/png"))
            .ReturnsAsync(rutaLogo);

        _usuarioRepositoryMock
            .Setup(r => r.GuardarCambiosAsync())
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.ActualizarMiPerfilAsync(dealerId, dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(rutaLogo, resultado.LogoUrl);
        Assert.Equal(rutaLogo, dealer.PerfilDealer!.LogoUrl);

        _almacenadorArchivosMock.Verify(
            a => a.GuardarArchivoAsync(
                It.IsAny<Stream>(),
                "logo-dealer.png",
                "image/png"),
            Times.Once);

        _usuarioRepositoryMock.Verify(
            r => r.GuardarCambiosAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarMiPerfilAsync_LogoConExtensionInvalida_LanzaArgumentException()
    {
        // Arrange
        const int dealerId = 1;

        var dealer = CrearDealerConPerfil();

        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        IFormFile logoInvalido = new FormFile(
            baseStream: stream,
            baseStreamOffset: 0,
            length: stream.Length,
            name: "Logo",
            fileName: "documento.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var dto = new PerfilDealerUpdateDto
        {
            NombreAgencia = "Auto Erick RD",
            Ubicacion = "Santo Domingo Este",
            TelefonoAgencia = "8095551234",
            Logo = logoInvalido
        };

        _usuarioRepositoryMock
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(dealerId))
            .ReturnsAsync(dealer);

        // Act
        var excepcion = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ActualizarMiPerfilAsync(dealerId, dto));

        // Assert
        Assert.Equal(
            "El logo debe ser una imagen JPG, JPEG, PNG o WEBP.",
            excepcion.Message);

        _almacenadorArchivosMock.Verify(
            a => a.GuardarArchivoAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _usuarioRepositoryMock.Verify(
            r => r.GuardarCambiosAsync(),
            Times.Never);
    }

    [Fact]
    public async Task ActualizarMiPerfilAsync_LogoMayorDe5MB_LanzaArgumentException()
    {
        // Arrange
        const int dealerId = 1;
        const long tamanioExcedido = 5 * 1024 * 1024 + 1;

        var dealer = CrearDealerConPerfil();

        var logoMock = new Mock<IFormFile>();
        logoMock.SetupGet(l => l.Length).Returns(tamanioExcedido);
        logoMock.SetupGet(l => l.FileName).Returns("logo-grande.png");
        logoMock.SetupGet(l => l.ContentType).Returns("image/png");

        var dto = new PerfilDealerUpdateDto
        {
            NombreAgencia = "Auto Erick RD",
            Ubicacion = "Santo Domingo Este",
            TelefonoAgencia = "8095551234",
            Logo = logoMock.Object
        };

        _usuarioRepositoryMock
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(dealerId))
            .ReturnsAsync(dealer);

        // Act
        var excepcion = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ActualizarMiPerfilAsync(dealerId, dto));

        // Assert
        Assert.Equal(
            "El logo no puede superar los 5 MB.",
            excepcion.Message);

        _almacenadorArchivosMock.Verify(
            a => a.GuardarArchivoAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _usuarioRepositoryMock.Verify(
            r => r.GuardarCambiosAsync(),
            Times.Never);
    }
}