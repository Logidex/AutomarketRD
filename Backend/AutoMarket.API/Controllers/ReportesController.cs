using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/reportes")]
/// <summary>
/// Recepción pública de reportes de anuncios. Anónimo permitido;
/// protegido con rate limiting por IP contra abuso.
/// </summary>
public class ReportesController : ControllerBase
{
    private readonly IReporteAnuncioService _reporteService;

    public ReportesController(IReporteAnuncioService reporteService)
    {
        _reporteService = reporteService;
    }

    /// <summary>
    /// Reporta un anuncio por contenido inapropiado, fraude u otro motivo.
    /// Acceso anónimo permitido. Aplica rate limiting por IP.
    /// </summary>
    /// <param name="dto">Anuncio, motivo y detalle opcional.</param>
    /// <returns>Confirmación del reporte.</returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaReportes")]
    public async Task<IActionResult> Crear([FromBody] CrearReporteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        var id = await _reporteService.CrearAsync(dto, ip);

        return Ok(new
        {
            exito = true,
            mensaje = "Gracias por tu reporte. Nuestro equipo lo revisará a la brevedad.",
            id
        });
    }
}
