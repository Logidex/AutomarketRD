using AutoMarket.API.Extensions;
using AutoMarket.Application.Features.HistorialVista.Commands;
using AutoMarket.Application.Features.HistorialVista.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HistorialController : BaseApiController
{
    public HistorialController(IMediator mediator) : base(mediator) { }

    [HttpPost("{anuncioId:int}")]
    public async Task<IActionResult> RegistrarVista(int anuncioId)
    {
        await Mediator.Send(new RegistrarVistaCommand(ObtenerUsuarioIdRequerido(), anuncioId));
        return Ok(new { exito = true });
    }

    [HttpGet("recientes")]
    public async Task<IActionResult> ObtenerRecientes([FromQuery] int cantidad = 12)
    {
        var resultado = await Mediator.Send(new ObtenerRecientesQuery(ObtenerUsuarioIdRequerido(), cantidad));
        return Ok(resultado);
    }
}
