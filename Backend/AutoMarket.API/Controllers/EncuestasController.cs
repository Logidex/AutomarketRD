using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.Interfaces;
using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
/// <summary>
/// Controlador de encuestas para usuarios de la plataforma.
/// </summary>
public class EncuestasController : ControllerBase
{
    private readonly IEncuestaService _encuestaService;

    public EncuestasController(IEncuestaService encuestaService)
    {
        _encuestaService = encuestaService;
    }

    /// <summary>
    /// Encuesta activa para el usuario autenticado (con flag de ya respondida).
    /// Devuelve 404 si no hay encuesta activa.
    /// </summary>
    [Authorize]
    [HttpGet("activa")]
    public async Task<IActionResult> ObtenerActiva()
    {
        var usuarioId = User.ObtenerUsuarioId();
        var encuesta = await _encuestaService.ObtenerActivaAsync(usuarioId);

        if (encuesta is null)
            return NotFound(new { mensaje = "No hay encuestas disponibles." });

        return Ok(encuesta);
    }

    /// <summary>
    /// Registra las respuestas del usuario autenticado. Las reglas (encuesta
    /// activa, todas las preguntas, una sola vez) llegan como
    /// BusinessRuleException (400 con mensaje para el usuario).
    /// </summary>
    [Authorize]
    [HttpPost("respuestas")]
    public async Task<IActionResult> Responder([FromBody] ResponderEncuestaDto dto)
    {
        var usuarioId = User.ObtenerUsuarioId();
        await _encuestaService.ResponderAsync(usuarioId, dto);
        return Ok(new { mensaje = "¡Gracias por tu opinión!" });
    }

    /// <summary>Resultados agregados de una encuesta (solo administradores).</summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{encuestaId:int}/resultados")]
    public async Task<IActionResult> ObtenerResultados(int encuestaId)
    {
        var resultados = await _encuestaService.ObtenerResultadosAsync(encuestaId);
        return Ok(resultados);
    }
}
