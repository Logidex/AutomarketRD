using AutoMarket.API.Extensions;
using AutoMarket.Application.Features.Favoritos.Commands;
using AutoMarket.Application.Features.Favoritos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritosController : BaseApiController
{
    public FavoritosController(IMediator mediator) : base(mediator) { }

    [HttpPost("{anuncioId:int}")]
    public async Task<IActionResult> AgregarFavorito(int anuncioId)
    {
        try
        {
            await Mediator.Send(new AgregarFavoritoCommand(ObtenerUsuarioIdRequerido(), anuncioId));
            return Ok(new { exito = true, mensaje = "Vehículo agregado a favoritos." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{anuncioId:int}")]
    public async Task<IActionResult> QuitarFavorito(int anuncioId)
    {
        try
        {
            await Mediator.Send(new QuitarFavoritoCommand(ObtenerUsuarioIdRequerido(), anuncioId));
            return Ok(new { exito = true, mensaje = "Vehículo removido de favoritos." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMisFavoritos()
    {
        var resultado = await Mediator.Send(new ObtenerFavoritosQuery(ObtenerUsuarioIdRequerido()));
        return Ok(resultado);
    }
}
