using AutoMarket.Application.Features.Comparador.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ComparadorController : BaseApiController
{
    public ComparadorController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> CompararVehiculos([FromQuery] int[] ids)
    {
        try
        {
            var resultado = await Mediator.Send(new CompararVehiculosQuery(ids));
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
