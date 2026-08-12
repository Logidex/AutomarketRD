using AutoMarket.Application.DTOs;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactoController : ControllerBase
{
    private readonly IContactoService _contactoService;
    private readonly ILogger<ContactoController> _logger;

    public ContactoController(
        IContactoService contactoService,
        ILogger<ContactoController> logger)
    {
        _contactoService = contactoService;
        _logger = logger;
    }

    /// <summary>
    /// Recibe un mensaje del formulario público de contacto
    /// (página /contacto) y lo reenvía al correo de soporte configurado.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLeads")]
    public async Task<IActionResult> EnviarMensajeContacto([FromBody] ContactoCreateDto dto)
    {
        // Honeypot anti-spam: si el campo "Website" viene lleno, es un bot.
        // Se responde 200 OK para que el bot crea que tuvo éxito y no reintente.
        if (!string.IsNullOrWhiteSpace(dto.Website))
        {
            _logger.LogWarning("Mensaje de contacto descartado por honeypot.");
            return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var enviado = await _contactoService.ProcesarMensajeContactoAsync(dto);

        if (!enviado)
        {
            _logger.LogError("No se pudo enviar el mensaje de contacto.");
            return StatusCode(500, new { mensaje = "No fue posible enviar tu mensaje en este momento. Inténtalo más tarde." });
        }

        return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente. Te responderemos lo antes posible." });
    }
}
