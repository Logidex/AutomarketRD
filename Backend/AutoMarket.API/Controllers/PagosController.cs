using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PagosController : ControllerBase
{
    private readonly IPagoOrquestacionService _pagoService;
    private readonly ILogger<PagosController> _logger;

    public PagosController(IPagoOrquestacionService pagoService, ILogger<PagosController> logger)
    {
        _pagoService = pagoService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("generar-link")]
    public async Task<IActionResult> GenerarLinkDePago([FromBody] CrearOrdenDto request)
    {
        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            var (url, montoUsd) = await _pagoService.GenerarLinkDePagoAsync(usuarioId, request);
            return Ok(new { url, monto = montoUsd, moneda = "USD" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando link de PayPal.");
            return StatusCode(500, new { mensaje = "Ocurrió un error al generar el link de pago." });
        }
    }

    [Authorize]
    [HttpPost("confirmar-pago")]
    public async Task<IActionResult> ConfirmarPago([FromBody] ConfirmarPagoDto dto)
    {
        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            var resultado = await _pagoService.ConfirmarPagoAsync(usuarioId, dto);

            if (resultado.Mensaje is not null)
                return BadRequest(new { mensaje = resultado.Mensaje });

            if (resultado.YaProcesado)
                return Ok(new { exito = true, yaProcesado = true });

            return Ok(new { exito = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirmando pago de PayPal.");
            return StatusCode(500, new { mensaje = "Ocurrió un error al confirmar el pago." });
        }
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> PayPalWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var jsonBody = await reader.ReadToEndAsync();

        var headers = new Dictionary<string, string>
        {
            ["PAYPAL-TRANSMISSION-ID"] = Request.Headers["PAYPAL-TRANSMISSION-ID"].ToString(),
            ["PAYPAL-TRANSMISSION-TIME"] = Request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString(),
            ["PAYPAL-TRANSMISSION-SIG"] = Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString(),
            ["PAYPAL-CERT-URL"] = Request.Headers["PAYPAL-CERT-URL"].ToString(),
            ["PAYPAL-AUTH-ALGO"] = Request.Headers["PAYPAL-AUTH-ALGO"].ToString()
        };

        var resultado = await _pagoService.ProcesarWebhookAsync(jsonBody, headers);

        if (resultado.StatusCode == 401)
            return Unauthorized();

        return Ok();
    }

    [Authorize]
    [HttpPost("transferencia")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegistrarTransferencia(
        [FromForm] string nombrePlan,
        [FromForm] string ciclo,
        [FromForm] IFormFile imagen)
    {
        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            var resultado = await _pagoService.RegistrarTransferenciaAsync(usuarioId, nombrePlan, ciclo, imagen);

            if (resultado.Mensaje is not null && !resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new
            {
                exito = true,
                pagoId = resultado.PagoId,
                mensaje = resultado.Mensaje
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando transferencia.");
            return StatusCode(500, new { mensaje = "Ocurrió un error al registrar la transferencia." });
        }
    }
}
