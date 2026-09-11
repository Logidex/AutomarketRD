using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Contacto.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactoController : BaseApiController
{
    private readonly ILogger<ContactoController> _logger;

    public ContactoController(IMediator mediator, ILogger<ContactoController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLeads")]
    public async Task<IActionResult> EnviarMensajeContacto([FromBody] ContactoCreateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Website))
        {
            _logger.LogWarning("Mensaje de contacto descartado por honeypot.");
            return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente." });
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var enviado = await Mediator.Send(new ProcesarMensajeContactoCommand(dto));

        if (!enviado)
        {
            _logger.LogError("No se pudo enviar el mensaje de contacto.");
            return StatusCode(500, new { mensaje = "No fue posible enviar tu mensaje en este momento. Inténtalo más tarde." });
        }

        return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente. Te responderemos lo antes posible." });
    }
}
