using System.Security.Claims;
using System.Text;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class PagosControllerTests
{
    private readonly Mock<IPayPalService> _mockPayPalService;
    private readonly Mock<ISuscripcionService> _mockSuscripcionService;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly Mock<ILogger<PagosController>> _mockLogger;
    private readonly PagosController _controller;

    public PagosControllerTests()
    {
        _mockPayPalService = new Mock<IPayPalService>();
        _mockSuscripcionService = new Mock<ISuscripcionService>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();
        _mockLogger = new Mock<ILogger<PagosController>>();

        _controller = new PagosController(
            _mockPayPalService.Object,
            _mockSuscripcionService.Object,
            _mockUsuarioRepository.Object,
            _mockLogger.Object);
    }

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void ConfigurarWebhook(string jsonBody, params (string key, string value)[] headers)
    {
        var bytes = Encoding.UTF8.GetBytes(jsonBody);
        var stream = new MemoryStream(bytes);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = stream;

        foreach (var (key, value) in headers)
            httpContext.Request.Headers[key] = value;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private Usuario CrearUsuarioConPerfilDealer(int perfilDealerId)
    {
        var usuario = new Usuario(
            nombre: "Erick",
            apellido: "Hipolito",
            email: "erick@test.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveSegura123"),
            rol: "Dealer",
            telefonoPersonal: "8095555555"
        );

        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(usuario, perfilDealerId);

        var perfilDealer = (PerfilDealer)Activator.CreateInstance(typeof(PerfilDealer), nonPublic: true)!;
        typeof(PerfilDealer).GetProperty("UsuarioId")?.SetValue(perfilDealer, perfilDealerId);

        typeof(Usuario).GetProperty("PerfilDealer")?.SetValue(usuario, perfilDealer);

        return usuario;
    }

    // =========================================================================
    // GENERAR LINK DE PAGO
    // =========================================================================

    [Fact]
    public async Task GenerarLinkDePago_TokenInvalido_DebeRetornarUnauthorized()
    {
        // Arrange
        SimularUsuarioAutenticado("abc");
        var dto = new CrearOrdenDto
        {
            Monto = 100m,
            NombrePlan = "Pro",
            Ciclo = "Mensual"
        };

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado);
        Assert.Equal(401, unauthorized.StatusCode);
    }

    [Fact]
    public async Task GenerarLinkDePago_UsuarioSinPerfilDealer_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync((Usuario?)null);

        var dto = new CrearOrdenDto
        {
            Monto = 100m,
            NombrePlan = "Pro",
            Ciclo = "Mensual"
        };

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GenerarLinkDePago_DatosValidos_DebeRetornarOkConUrl()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        var usuario = CrearUsuarioConPerfilDealer(15);

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(usuario);

        _mockPayPalService
            .Setup(s => s.CrearOrdenDeSuscripcionAsync(15, 100m, "Pro", "Mensual"))
            .ReturnsAsync("https://paypal.com/approve");

        var dto = new CrearOrdenDto
        {
            Monto = 100m,
            NombrePlan = "Pro",
            Ciclo = "Mensual"
        };

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var url = ok.Value?.GetType().GetProperty("url")?.GetValue(ok.Value);

        Assert.Equal("https://paypal.com/approve", url);

        _mockPayPalService.Verify(s =>
            s.CrearOrdenDeSuscripcionAsync(15, 100m, "Pro", "Mensual"), Times.Once);
    }

    [Fact]
    public async Task GenerarLinkDePago_SiPayPalFalla_DebeRetornar500()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        var usuario = CrearUsuarioConPerfilDealer(15);

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(usuario);

        _mockPayPalService
            .Setup(s => s.CrearOrdenDeSuscripcionAsync(15, 100m, "Pro", "Mensual"))
            .ThrowsAsync(new Exception("falló paypal"));

        var dto = new CrearOrdenDto
        {
            Monto = 100m,
            NombrePlan = "Pro",
            Ciclo = "Mensual"
        };

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var obj = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(500, obj.StatusCode);
    }

    // =========================================================================
    // WEBHOOK
    // =========================================================================

    [Fact]
    public async Task PayPalWebhook_BodyVacio_DebeRetornarUnauthorized()
    {
        // Arrange
        ConfigurarWebhook(string.Empty,
            ("PAYPAL-TRANSMISSION-ID", "1"),
            ("PAYPAL-TRANSMISSION-TIME", "2"),
            ("PAYPAL-TRANSMISSION-SIG", "3"),
            ("PAYPAL-CERT-URL", "4"),
            ("PAYPAL-AUTH-ALGO", "5"));

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<UnauthorizedResult>(resultado);
    }

    [Fact]
    public async Task PayPalWebhook_HeadersIncompletos_DebeRetornarUnauthorized()
    {
        // Arrange
        var json = """{"id":"evt-1","event_type":"CHECKOUT.ORDER.APPROVED","resource":{"id":"ord-1","purchase_units":[{"reference_id":"DEALER-15-PLAN-PRO-CICLO-MENSUAL"}]}}""";

        ConfigurarWebhook(json,
            ("PAYPAL-TRANSMISSION-ID", "1"),
            ("PAYPAL-TRANSMISSION-TIME", "2"));

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<UnauthorizedResult>(resultado);
    }

    [Fact]
    public async Task PayPalWebhook_FirmaInvalida_DebeRetornarUnauthorized()
    {
        // Arrange
        var json = """{"id":"evt-1","event_type":"CHECKOUT.ORDER.APPROVED","resource":{"id":"ord-1","purchase_units":[{"reference_id":"DEALER-15-PLAN-PRO-CICLO-MENSUAL"}]}}""";

        ConfigurarWebhook(json,
            ("PAYPAL-TRANSMISSION-ID", "1"),
            ("PAYPAL-TRANSMISSION-TIME", "2"),
            ("PAYPAL-TRANSMISSION-SIG", "3"),
            ("PAYPAL-CERT-URL", "4"),
            ("PAYPAL-AUTH-ALGO", "5"));

        _mockPayPalService
            .Setup(s => s.VerificarFirmaWebhookAsync(json, "1", "2", "3", "4", "5"))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<UnauthorizedResult>(resultado);
    }

    [Fact]
    public async Task PayPalWebhook_EventoDistintoAApproved_DebeRetornarOk()
    {
        // Arrange
        var json = """{"id":"evt-1","event_type":"PAYMENT.CAPTURE.COMPLETED","resource":{"id":"ord-1","purchase_units":[{"reference_id":"DEALER-15-PLAN-PRO-CICLO-MENSUAL"}]}}""";

        ConfigurarWebhook(json,
            ("PAYPAL-TRANSMISSION-ID", "1"),
            ("PAYPAL-TRANSMISSION-TIME", "2"),
            ("PAYPAL-TRANSMISSION-SIG", "3"),
            ("PAYPAL-CERT-URL", "4"),
            ("PAYPAL-AUTH-ALGO", "5"));

        _mockPayPalService
            .Setup(s => s.VerificarFirmaWebhookAsync(json, "1", "2", "3", "4", "5"))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<OkResult>(resultado);
        _mockPayPalService.Verify(s => s.CapturarOrdenAsync(It.IsAny<string>()), Times.Never);
        _mockSuscripcionService.Verify(s =>
            s.ProcesarPagoSuscripcionAsync(It.IsAny<int>(), It.IsAny<PlanNivel>(), It.IsAny<CicloFacturacion>()),
            Times.Never);
    }

    [Fact]
    public async Task PayPalWebhook_ReferenceIdInvalido_DebeRetornarOk()
    {
        // Arrange
        var json = """{"id":"evt-1","event_type":"CHECKOUT.ORDER.APPROVED","resource":{"id":"ord-1","purchase_units":[{"reference_id":"MALFORMADO"}]}}""";

        ConfigurarWebhook(json,
            ("PAYPAL-TRANSMISSION-ID", "1"),
            ("PAYPAL-TRANSMISSION-TIME", "2"),
            ("PAYPAL-TRANSMISSION-SIG", "3"),
            ("PAYPAL-CERT-URL", "4"),
            ("PAYPAL-AUTH-ALGO", "5"));

        _mockPayPalService
            .Setup(s => s.VerificarFirmaWebhookAsync(json, "1", "2", "3", "4", "5"))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<OkResult>(resultado);
        _mockPayPalService.Verify(s => s.CapturarOrdenAsync(It.IsAny<string>()), Times.Never);
        _mockSuscripcionService.Verify(s =>
            s.ProcesarPagoSuscripcionAsync(It.IsAny<int>(), It.IsAny<PlanNivel>(), It.IsAny<CicloFacturacion>()),
            Times.Never);
    }

    [Fact]
    public async Task PayPalWebhook_FlujoValido_DebeCapturarYProcesarSuscripcion()
    {
        // Arrange
        var json = """
        {
          "id": "evt-1",
          "event_type": "CHECKOUT.ORDER.APPROVED",
          "resource": {
            "id": "ord-999",
            "purchase_units": [
              {
                "reference_id": "DEALER-15-PLAN-PRO-CICLO-MENSUAL"
              }
            ]
          }
        }
        """;

        ConfigurarWebhook(json,
            ("PAYPAL-TRANSMISSION-ID", "1"),
            ("PAYPAL-TRANSMISSION-TIME", "2"),
            ("PAYPAL-TRANSMISSION-SIG", "3"),
            ("PAYPAL-CERT-URL", "4"),
            ("PAYPAL-AUTH-ALGO", "5"));

        _mockPayPalService
            .Setup(s => s.VerificarFirmaWebhookAsync(json, "1", "2", "3", "4", "5"))
            .ReturnsAsync(true);

        _mockPayPalService
            .Setup(s => s.CapturarOrdenAsync("ord-999"))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<OkResult>(resultado);
        _mockPayPalService.Verify(s => s.CapturarOrdenAsync("ord-999"), Times.Once);
        _mockSuscripcionService.Verify(s =>
            s.ProcesarPagoSuscripcionAsync(15, PlanNivel.Pro, CicloFacturacion.Mensual),
            Times.Once);
    }
}