using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class PagosControllerTests
{
    private readonly Mock<IPagoOrquestacionService> _mockPagoService;
    private readonly Mock<ILogger<PagosController>> _mockLogger;
    private readonly PagosController _controller;

    public PagosControllerTests()
    {
        _mockPagoService = new Mock<IPagoOrquestacionService>();
        _mockLogger = new Mock<ILogger<PagosController>>();
        _controller = new PagosController(_mockPagoService.Object, _mockLogger.Object);
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
    public async Task GenerarLinkDePago_TokenInvalido_Retorna500()
    {
        SimularUsuarioAutenticado("abc");
        var dto = CrearOrdenProMensual();

        var resultado = await _controller.GenerarLinkDePago(dto);

        var objectResult = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GenerarLinkDePago_Exitoso_DebeRetornarOkConUrl()
    {
        SimularUsuarioAutenticado("15");
        var dto = CrearOrdenProMensual();

        _mockPagoService
            .Setup(s => s.GenerarLinkDePagoAsync(15, dto))
            .ReturnsAsync(("https://paypal.com/approve", 30.00m));

        var resultado = await _controller.GenerarLinkDePago(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = ok.Value!;
        Assert.Equal("https://paypal.com/approve", value.GetType().GetProperty("url")!.GetValue(value));
        Assert.Equal(30.00m, value.GetType().GetProperty("monto")!.GetValue(value));
        Assert.Equal("USD", value.GetType().GetProperty("moneda")!.GetValue(value));
    }

    [Fact]
    public async Task GenerarLinkDePago_InvalidOperationException_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = CrearOrdenProMensual();

        _mockPagoService
            .Setup(s => s.GenerarLinkDePagoAsync(15, dto))
            .ThrowsAsync(new InvalidOperationException("Plan no válido"));

        var resultado = await _controller.GenerarLinkDePago(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        var value = badRequest.Value!;
        Assert.Equal("Plan no válido", value.GetType().GetProperty("mensaje")!.GetValue(value));
    }

    [Fact]
    public async Task GenerarLinkDePago_ExcepcionGenerica_DebeRetornar500()
    {
        SimularUsuarioAutenticado("15");
        var dto = CrearOrdenProMensual();

        _mockPagoService
            .Setup(s => s.GenerarLinkDePagoAsync(15, dto))
            .ThrowsAsync(new Exception("falló inesperado"));

        var resultado = await _controller.GenerarLinkDePago(dto);

        var obj = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(500, obj.StatusCode);
    }

    // =========================================================================
    // CONFIRMAR PAGO
    // =========================================================================

    [Fact]
    public async Task ConfirmarPago_Exitoso_DebeRetornarOkConExito()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ConfirmarPagoDto();

        _mockPagoService
            .Setup(s => s.ConfirmarPagoAsync(15, dto))
            .ReturnsAsync(new ConfirmarPagoResult { Exito = true });

        var resultado = await _controller.ConfirmarPago(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = ok.Value!;
        Assert.Equal(true, value.GetType().GetProperty("exito")!.GetValue(value));
    }

    [Fact]
    public async Task ConfirmarPago_YaProcesado_DebeRetornarOkConYaProcesado()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ConfirmarPagoDto();

        _mockPagoService
            .Setup(s => s.ConfirmarPagoAsync(15, dto))
            .ReturnsAsync(new ConfirmarPagoResult { Exito = true, YaProcesado = true });

        var resultado = await _controller.ConfirmarPago(dto);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = ok.Value!;
        Assert.Equal(true, value.GetType().GetProperty("exito")!.GetValue(value));
        Assert.Equal(true, value.GetType().GetProperty("yaProcesado")!.GetValue(value));
    }

    [Fact]
    public async Task ConfirmarPago_ConMensaje_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ConfirmarPagoDto();

        _mockPagoService
            .Setup(s => s.ConfirmarPagoAsync(15, dto))
            .ReturnsAsync(new ConfirmarPagoResult { Exito = false, Mensaje = "Pago rechazado" });

        var resultado = await _controller.ConfirmarPago(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        var value = badRequest.Value!;
        Assert.Equal("Pago rechazado", value.GetType().GetProperty("mensaje")!.GetValue(value));
    }

    [Fact]
    public async Task ConfirmarPago_InvalidOperationException_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ConfirmarPagoDto();

        _mockPagoService
            .Setup(s => s.ConfirmarPagoAsync(15, dto))
            .ThrowsAsync(new InvalidOperationException("Transacción no encontrada"));

        var resultado = await _controller.ConfirmarPago(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        var value = badRequest.Value!;
        Assert.Equal("Transacción no encontrada", value.GetType().GetProperty("mensaje")!.GetValue(value));
    }

    [Fact]
    public async Task ConfirmarPago_ExcepcionGenerica_DebeRetornar500()
    {
        SimularUsuarioAutenticado("15");
        var dto = new ConfirmarPagoDto();

        _mockPagoService
            .Setup(s => s.ConfirmarPagoAsync(15, dto))
            .ThrowsAsync(new Exception("falló inesperado"));

        var resultado = await _controller.ConfirmarPago(dto);

        var obj = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(500, obj.StatusCode);
    }

    // =========================================================================
    // PAYPAL WEBHOOK
    // =========================================================================

    [Fact]
    public async Task PayPalWebhook_Status401_DebeRetornarUnauthorized()
    {
        var json = "{}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = stream;
        httpContext.Request.Headers["PAYPAL-TRANSMISSION-ID"] = "tid-1";
        httpContext.Request.Headers["PAYPAL-TRANSMISSION-TIME"] = "ttime-1";
        httpContext.Request.Headers["PAYPAL-TRANSMISSION-SIG"] = "sig-1";
        httpContext.Request.Headers["PAYPAL-CERT-URL"] = "cert-1";
        httpContext.Request.Headers["PAYPAL-AUTH-ALGO"] = "algo-1";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var headers = new Dictionary<string, string>
        {
            ["PAYPAL-TRANSMISSION-ID"] = "tid-1",
            ["PAYPAL-TRANSMISSION-TIME"] = "ttime-1",
            ["PAYPAL-TRANSMISSION-SIG"] = "sig-1",
            ["PAYPAL-CERT-URL"] = "cert-1",
            ["PAYPAL-AUTH-ALGO"] = "algo-1"
        };

        _mockPagoService
            .Setup(s => s.ProcesarWebhookAsync(json, headers))
            .ReturnsAsync(new WebhookResult { StatusCode = 401 });

        var resultado = await _controller.PayPalWebhook();

        Assert.IsType<UnauthorizedResult>(resultado);
    }

    [Fact]
    public async Task PayPalWebhook_Status200_DebeRetornarOk()
    {
        var json = "{}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = stream;
        httpContext.Request.Headers["PAYPAL-TRANSMISSION-ID"] = "tid-2";
        httpContext.Request.Headers["PAYPAL-TRANSMISSION-TIME"] = "ttime-2";
        httpContext.Request.Headers["PAYPAL-TRANSMISSION-SIG"] = "sig-2";
        httpContext.Request.Headers["PAYPAL-CERT-URL"] = "cert-2";
        httpContext.Request.Headers["PAYPAL-AUTH-ALGO"] = "algo-2";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var headers = new Dictionary<string, string>
        {
            ["PAYPAL-TRANSMISSION-ID"] = "tid-2",
            ["PAYPAL-TRANSMISSION-TIME"] = "ttime-2",
            ["PAYPAL-TRANSMISSION-SIG"] = "sig-2",
            ["PAYPAL-CERT-URL"] = "cert-2",
            ["PAYPAL-AUTH-ALGO"] = "algo-2"
        };

        _mockPagoService
            .Setup(s => s.ProcesarWebhookAsync(json, headers))
            .ReturnsAsync(new WebhookResult { StatusCode = 200 });

        var resultado = await _controller.PayPalWebhook();

        Assert.IsType<OkResult>(resultado);
    }

    // =========================================================================
    // REGISTRAR TRANSFERENCIA
    // =========================================================================

    [Fact]
    public async Task RegistrarTransferencia_Exitoso_DebeRetornarOkConPagoId()
    {
        SimularUsuarioAutenticado("15");
        var imagenMock = new FormFile(Stream.Null, 0, 0, "imagen", "test.jpg");

        _mockPagoService
            .Setup(s => s.RegistrarTransferenciaAsync(15, "Pro", "Mensual", imagenMock))
            .ReturnsAsync(new TransferenciaResult { Exito = true, PagoId = 42 });

        var resultado = await _controller.RegistrarTransferencia("Pro", "Mensual", imagenMock);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = ok.Value!;
        Assert.Equal(true, value.GetType().GetProperty("exito")!.GetValue(value));
        Assert.Equal(42, value.GetType().GetProperty("pagoId")!.GetValue(value));
    }

    [Fact]
    public async Task RegistrarTransferencia_ExitoFalse_ConMensaje_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var imagenMock = new FormFile(Stream.Null, 0, 0, "imagen", "test.jpg");

        _mockPagoService
            .Setup(s => s.RegistrarTransferenciaAsync(15, "Pro", "Mensual", imagenMock))
            .ReturnsAsync(new TransferenciaResult
            {
                Exito = false,
                Mensaje = "Formato de imagen no válido"
            });

        var resultado = await _controller.RegistrarTransferencia("Pro", "Mensual", imagenMock);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        var value = badRequest.Value!;
        Assert.Equal("Formato de imagen no válido", value.GetType().GetProperty("mensaje")!.GetValue(value));
    }

    [Fact]
    public async Task RegistrarTransferencia_InvalidOperationException_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var imagenMock = new FormFile(Stream.Null, 0, 0, "imagen", "test.jpg");

        _mockPagoService
            .Setup(s => s.RegistrarTransferenciaAsync(15, "Pro", "Mensual", imagenMock))
            .ThrowsAsync(new InvalidOperationException("Usuario no encontrado"));

        var resultado = await _controller.RegistrarTransferencia("Pro", "Mensual", imagenMock);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
        var value = badRequest.Value!;
        Assert.Equal("Usuario no encontrado", value.GetType().GetProperty("mensaje")!.GetValue(value));
    }

    [Fact]
    public async Task RegistrarTransferencia_ExcepcionGenerica_DebeRetornar500()
    {
        SimularUsuarioAutenticado("15");
        var imagenMock = new FormFile(Stream.Null, 0, 0, "imagen", "test.jpg");

        _mockPagoService
            .Setup(s => s.RegistrarTransferenciaAsync(15, "Pro", "Mensual", imagenMock))
            .ThrowsAsync(new Exception("falló inesperado"));

        var resultado = await _controller.RegistrarTransferencia("Pro", "Mensual", imagenMock);

        var obj = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(500, obj.StatusCode);
    }
}
