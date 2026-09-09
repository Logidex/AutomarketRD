using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Application.Features.Cupones.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CuponesController : BaseApiController
{
    public CuponesController(IMediator mediator) : base(mediator) { }

    [Authorize(Roles = Roles.Dealer)]
    [HttpPost("aplicar")]
    [ProducesResponseType(typeof(CuponAplicadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Aplicar([FromBody] AplicarCuponDto dto)
    {
        var resultado = await Mediator.Send(new AplicarCuponCommand(ObtenerUsuarioIdRequerido(), dto.Codigo));
        return Ok(resultado);
    }
}
