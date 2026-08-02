using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMarket.Application.Services;
using AutoMarket.Application.DTOs;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using System.Reflection;

namespace AutoMarket.Tests.Services;

public class LeadServiceTests
{
    private readonly Mock<ILeadRepository> _mockLeadRepo;
    private readonly Mock<IAnuncioRepository> _mockAnuncioRepo;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
    private readonly Mock<IEmailSenderService> _mockEmailSender;
    private readonly Mock<ILogger<LeadService>> _mockLogger;
    private readonly LeadService _servicio;

    public LeadServiceTests()
    {
        _mockLeadRepo = new Mock<ILeadRepository>();
        _mockAnuncioRepo = new Mock<IAnuncioRepository>();
        _mockUsuarioRepo = new Mock<IUsuarioRepository>();
        _mockEmailSender = new Mock<IEmailSenderService>();
        _mockLogger = new Mock<ILogger<LeadService>>();

        _servicio = new LeadService(
            _mockLeadRepo.Object,
            _mockAnuncioRepo.Object,
            _mockUsuarioRepo.Object,
            _mockEmailSender.Object,
            _mockLogger.Object
        );
    }

    // =========================================================================
    // HELPER: Crear Entidades Encapsuladas para Tests
    // =========================================================================
    private Usuario CrearUsuarioSimulado(int usuarioId, string email)
    {
        // Usamos el constructor público de tu entidad Usuario
        var usuario = new Usuario("Dealer", "Prueba", email, "hash", "8090000000", "Dealer");
        
        // Seteamos la propiedad UsuarioId privada mediante Reflection
        var propId = typeof(Usuario).GetProperty("UsuarioId");
        propId?.SetValue(usuario, usuarioId);

        return usuario;
    }

    private Anuncio CrearAnuncioSimulado(int id, int usuarioId, string marca, string modelo)
    {
        // Bypasseamos el constructor de Anuncio para no lidiar con las validaciones
        // de Precio > 0 o Anio > 0 en las pruebas exclusivas de Leads
        var anuncio = (Anuncio)Activator.CreateInstance(typeof(Anuncio), nonPublic: true)!;
        
        typeof(Anuncio).GetProperty("Id")?.SetValue(anuncio, id);
        typeof(Anuncio).GetProperty("UsuarioId")?.SetValue(anuncio, usuarioId);
        typeof(Anuncio).GetProperty("Marca")?.SetValue(anuncio, marca);
        typeof(Anuncio).GetProperty("Modelo")?.SetValue(anuncio, modelo);

        return anuncio;
    }

    // =========================================================================
    // PRUEBA 01: Falla si el vehículo no existe o fue eliminado
    // =========================================================================
    [Fact]
    public async Task CrearLeadAsync_AnuncioNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var dto = new LeadCreateDto { AnuncioId = 99, NombreContacto = "Juan", Mensaje = "Hola", Canal = CanalContacto.Formulario };
        
        _mockAnuncioRepo.Setup(r => r.ObtenerPorIdAsync(99))
            .ReturnsAsync((Anuncio?)null);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _servicio.CrearLeadAsync(dto));

        Assert.Equal("El vehículo al que intentas contactar no existe o ya fue vendido.", excepcion.Message);
        _mockLeadRepo.Verify(r => r.AgregarAsync(It.IsAny<Lead>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 02: Falla si el vendedor asociado al anuncio no se encuentra
    // =========================================================================
    [Fact]
    public async Task CrearLeadAsync_VendedorNoExiste_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new LeadCreateDto { AnuncioId = 1, NombreContacto = "Juan", Mensaje = "Hola", Canal = CanalContacto.Formulario };
        var anuncioFalso = CrearAnuncioSimulado(id: 1, usuarioId: 5, marca: "Honda", modelo: "Civic");

        _mockAnuncioRepo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(anuncioFalso);
        _mockUsuarioRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(5)).ReturnsAsync((Usuario?)null);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _servicio.CrearLeadAsync(dto));

        Assert.Equal("No se encontró el propietario de este anuncio.", excepcion.Message);
        _mockLeadRepo.Verify(r => r.AgregarAsync(It.IsAny<Lead>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 03: Éxito total - Guarda el Lead y envía el correo
    // =========================================================================
    [Fact]
    public async Task CrearLeadAsync_DatosValidos_DebeGuardarLeadYEnviarCorreo()
    {
        // Arrange
        var dto = new LeadCreateDto 
        { 
            AnuncioId = 1, 
            NombreContacto = "Maria Perez", 
            EmailContacto = "maria@test.com",
            TelefonoContacto = "809-555-5555",
            Mensaje = "Me interesa este vehículo", 
            Canal = CanalContacto.WhatsApp 
        };

        var anuncio = CrearAnuncioSimulado(id: 1, usuarioId: 10, marca: "Toyota", modelo: "Corolla");
        var vendedor = CrearUsuarioSimulado(usuarioId: 10, email: "vendedor@auto.com");

        _mockAnuncioRepo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(anuncio);
        _mockUsuarioRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(10)).ReturnsAsync(vendedor);

        // Act
        await _servicio.CrearLeadAsync(dto);

        // Assert
        // 1. Verificamos que se guardó en la base de datos
        _mockLeadRepo.Verify(r => r.AgregarAsync(It.Is<Lead>(l => 
            l.AnuncioId == 1 && 
            l.NombreContacto == "Maria Perez" &&
            l.Canal == CanalContacto.WhatsApp
        )), Times.Once);

        // 2. Verificamos que se intentó enviar el correo con los datos correctos
        _mockEmailSender.Verify(e => e.EnviarCorreoAsync(
            vendedor.Email,
            It.Is<string>(asunto => asunto.Contains("Toyota Corolla")),
            It.Is<string>(cuerpo => cuerpo.Contains("Maria Perez") && cuerpo.Contains("Me interesa este vehículo"))
        ), Times.Once);
    }

    // =========================================================================
    // PRUEBA 04: Resiliencia - Si el correo falla, el Lead DEBE guardarse igual
    // =========================================================================
    [Fact]
    public async Task CrearLeadAsync_FallaServidorSMTP_NoLanzaExcepcionYGuardaLead()
    {
        // Arrange
        var dto = new LeadCreateDto { AnuncioId = 1, NombreContacto = "Pedro", Mensaje = "Info", Canal = CanalContacto.Formulario };
        var anuncio = CrearAnuncioSimulado(1, 10, "Ford", "Escape");
        var vendedor = CrearUsuarioSimulado(10, "vendedor@auto.com");

        _mockAnuncioRepo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(anuncio);
        _mockUsuarioRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(10)).ReturnsAsync(vendedor);

        // Simulamos que el servidor SMTP está caído o la contraseña es incorrecta
        _mockEmailSender.Setup(e => e.EnviarCorreoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP Timeout"));

        // Act
        // Como capturamos la excepción en el servicio con un try-catch, esto no debe lanzar error.
        await _servicio.CrearLeadAsync(dto);

        // Assert
        // El Lead TIENE que haberse guardado para no perder al cliente, aunque el correo fallara.
        _mockLeadRepo.Verify(r => r.AgregarAsync(It.IsAny<Lead>()), Times.Once);
    }
}