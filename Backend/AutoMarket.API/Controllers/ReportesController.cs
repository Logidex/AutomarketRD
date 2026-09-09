using AutoMarket.Application.DTOs.Reportes;
using AutoMarket.Application.Features.ReportesAnuncio.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : BaseApiController
{
    public ReportesController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaReportes")]
    public async Task<IActionResult> Crear([FromBody] CrearReporteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        var id = await Mediator.Send(new CrearReporteCommand(dto, ip));

        return Ok(new
        {
            exito = true,
            mensaje = "Gracias por tu reporte. Nuestro equipo lo revisará a la brevedad.",
            id
        });
    }
}
