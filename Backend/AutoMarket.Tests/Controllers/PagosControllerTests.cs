using System.Security.Claims;
using System.Text;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class PagosControllerTests
{
    private readonly Mock<IPayPalService> _mockPayPalService;
    private readonly Mock<ISuscripcionService> _mockSuscripcionService;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly Mock<IPlanCatalogoService> _mockPlanCatalogoService;
    private readonly Mock<ILogger<PagosController>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly PagosController _controller;

    public PagosControllerTests()
    {
        _mockPayPalService = new Mock<IPayPalService>();
        _mockSuscripcionService = new Mock<ISuscripcionService>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();
        _mockPlanCatalogoService = new Mock<IPlanCatalogoService>();
        _mockLogger = new Mock<ILogger<PagosController>>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Pago:TasaCambioRD_USD"] = "0.01"
            })
            .Build();

        _controller = new PagosController(
            _mockPayPalService.Object,
            _mockSuscripcionService.Object,
            _mockUsuarioRepository.Object,
            _mockPlanCatalogoService.Object,
            _configuration,
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

    private void ConfigurarPlanCatalogoPro()
    {
        _mockPlanCatalogoService
            .Setup(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Pro))
            .ReturnsAsync(new PlanCatalogoDto
            {
                Nivel = PlanNivel.Pro,
                Nombre = "Plan Pro",
                LimiteAnuncios = 200,
                PrecioMensual = 3000m,
                PrecioTrimestral = 8370m,
                PrecioAnual = 30600m
            });
    }

    private static CrearOrdenDto CrearOrdenProMensual()
    {
        return new CrearOrdenDto
        {
            NombrePlan = "Pro",
            Ciclo = "Mensual"
        };
    }

    // =========================================================================
    // GENERAR LINK DE PAGO
    // =========================================================================

    [Fact]
    public async Task GenerarLinkDePago_TokenInvalido_LanzaExcepcionNoAutorizado()
    {
        // Arrange
        SimularUsuarioAutenticado("abc");
        var dto = CrearOrdenProMensual();

        // Act & Assert
        // ObtenerUsuarioId() lanza UnauthorizedAccessException (el middleware la convierte a 401).
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _controller.GenerarLinkDePago(dto));
    }

    [Fact]
    public async Task GenerarLinkDePago_UsuarioSinPerfilDealer_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync((Usuario?)null);

        var dto = CrearOrdenProMensual();

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GenerarLinkDePago_PlanInvalido_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        var usuario = CrearUsuarioConPerfilDealer(15);

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(usuario);

        var dto = new CrearOrdenDto
        {
            NombrePlan = "PlanInexistente",
            Ciclo = "Mensual"
        };

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        _mockPayPalService.Verify(s =>
            s.CrearOrdenDeSuscripcionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GenerarLinkDePago_PlanNoEncontrado_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        var usuario = CrearUsuarioConPerfilDealer(15);

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(usuario);

        _mockPlanCatalogoService
            .Setup(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Pro))
            .ReturnsAsync((PlanCatalogoDto?)null);

        var dto = CrearOrdenProMensual();

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        _mockPayPalService.Verify(m =>
            m.CrearOrdenDeSuscripcionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GenerarLinkDePago_PlanGratis_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        var usuario = CrearUsuarioConPerfilDealer(15);

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(usuario);

        _mockPlanCatalogoService
            .Setup(s => s.ObtenerPlanPorNivelAsync(PlanNivel.Gratis))
            .ReturnsAsync(new PlanCatalogoDto
            {
                Nivel = PlanNivel.Gratis,
                Nombre = "Plan Gratis",
                LimiteAnuncios = 1,
                PrecioMensual = 0m,
                PrecioTrimestral = 0m,
                PrecioAnual = 0m
            });

        var dto = new CrearOrdenDto
        {
            NombrePlan = "Gratis",
            Ciclo = "Mensual"
        };

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        _mockPayPalService.Verify(s =>
            s.CrearOrdenDeSuscripcionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
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

        ConfigurarPlanCatalogoPro();

        // 3000 RD$ * 0.01 = 30.00 USD
        _mockPayPalService
            .Setup(s => s.CrearOrdenDeSuscripcionAsync(15, 30.00m, "Pro", "Mensual"))
            .ReturnsAsync("https://paypal.com/approve");

        var dto = CrearOrdenProMensual();

        // Act
        var resultado = await _controller.GenerarLinkDePago(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var url = ok.Value?.GetType().GetProperty("url")?.GetValue(ok.Value);

        Assert.Equal("https://paypal.com/approve", url);

        _mockPayPalService.Verify(s =>
            s.CrearOrdenDeSuscripcionAsync(15, 30.00m, "Pro", "Mensual"), Times.Once);
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

        ConfigurarPlanCatalogoPro();

        _mockPayPalService
            .Setup(s => s.CrearOrdenDeSuscripcionAsync(15, 30.00m, "Pro", "Mensual"))
            .ThrowsAsync(new Exception("falló paypal"));

        var dto = CrearOrdenProMensual();

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
                "reference_id": "DEALER-15-PLAN-PRO-CICLO-MENSUAL",
                "amount": { "currency_code": "USD", "value": "30.00" }
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

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(CrearUsuarioConPerfilDealer(15));

        ConfigurarPlanCatalogoPro();

        // 3000 RD$ * 0.01 = 30.00 USD
        _mockSuscripcionService
            .Setup(s => s.ExistePagoPorEventoAsync("evt-1"))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<OkResult>(resultado);
        _mockPayPalService.Verify(s => s.CapturarOrdenAsync("ord-999"), Times.Once);
        _mockSuscripcionService.Verify(s =>
            s.ProcesarPagoSuscripcionAsync(15, PlanNivel.Pro, CicloFacturacion.Mensual),
            Times.Once);
        _mockSuscripcionService.Verify(s =>
            s.RegistrarPagoAsync(15, PlanNivel.Pro, CicloFacturacion.Mensual, 30.00m, "USD", "ord-999", "evt-1", "DEALER-15-PLAN-PRO-CICLO-MENSUAL"),
            Times.Once);
    }

    [Fact]
    public async Task PayPalWebhook_EventoYaProcesado_DebeIgnorarse()
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
                "reference_id": "DEALER-15-PLAN-PRO-CICLO-MENSUAL",
                "amount": { "currency_code": "USD", "value": "30.00" }
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

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(CrearUsuarioConPerfilDealer(15));

        ConfigurarPlanCatalogoPro();

        _mockSuscripcionService
            .Setup(s => s.ExistePagoPorEventoAsync("evt-1"))
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
    public async Task PayPalWebhook_MontoNoCoincide_DebeNoProcesar()
    {
        var json = """
        {
          "id": "evt-2",
          "event_type": "CHECKOUT.ORDER.APPROVED",
          "resource": {
            "id": "ord-998",
            "purchase_units": [
              {
                "reference_id": "DEALER-15-PLAN-PRO-CICLO-MENSUAL",
                "amount": { "currency_code": "USD", "value": "1.00" }
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

        _mockUsuarioRepository
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(15))
            .ReturnsAsync(CrearUsuarioConPerfilDealer(15));

        ConfigurarPlanCatalogoPro();

        _mockSuscripcionService
            .Setup(s => s.ExistePagoPorEventoAsync("evt-2"))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.PayPalWebhook();

        // Assert
        Assert.IsType<OkResult>(resultado);
        _mockPayPalService.Verify(s => s.CapturarOrdenAsync(It.IsAny<string>()), Times.Never);
        _mockSuscripcionService.Verify(s =>
            s.ProcesarPagoSuscripcionAsync(It.IsAny<int>(), It.IsAny<PlanNivel>(), It.IsAny<CicloFacturacion>()),
            Times.Never);
    }
}
