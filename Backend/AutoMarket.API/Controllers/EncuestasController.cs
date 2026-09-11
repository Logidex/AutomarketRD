using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.Features.Encuestas.Commands;
using AutoMarket.Application.Features.Encuestas.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EncuestasController : BaseApiController
{
    public EncuestasController(IMediator mediator) : base(mediator) { }

    [Authorize]
    [HttpGet("activa")]
    public async Task<IActionResult> ObtenerActiva()
    {
        var encuesta = await Mediator.Send(new ObtenerEncuestaActivaQuery(ObtenerUsuarioIdRequerido()));
        if (encuesta is null)
            return NotFound(new { mensaje = "No hay encuestas disponibles." });
        return Ok(encuesta);
    }

    [Authorize]
    [HttpPost("respuestas")]
    public async Task<IActionResult> Responder([FromBody] ResponderEncuestaDto dto)
    {
        await Mediator.Send(new ResponderEncuestaCommand(ObtenerUsuarioIdRequerido(), dto));
        return Ok(new { mensaje = "¡Gracias por tu opinión!" });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{encuestaId:int}/resultados")]
    public async Task<IActionResult> ObtenerResultados(int encuestaId)
    {
        var resultados = await Mediator.Send(new ObtenerResultadosEncuestaQuery(encuestaId));
        return Ok(resultados);
    }
}
