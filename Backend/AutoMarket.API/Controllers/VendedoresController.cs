using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.Features.Vendedores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/vendedores")]
[Authorize]
public class VendedoresController : BaseApiController
{
    public VendedoresController(IMediator mediator) : base(mediator) { }

    [HttpGet("me/suscripcion")]
    public async Task<IActionResult> ObtenerMiSuscripcion()
    {
        if (!User.IsInRole(Roles.Vendedor))
            return Forbid();

        var usuarioId = ObtenerUsuarioIdRequerido();
        var suscripcion = await Mediator.Send(new ObtenerMiSuscripcionQuery(usuarioId));

        if (suscripcion is null)
            return NotFound(new { mensaje = "No se encontró información de suscripción." });

        return Ok(suscripcion);
    }

    [HttpGet("me/anuncios")]
    public async Task<IActionResult> ObtenerMisAnuncios()
    {
        if (!User.IsInRole(Roles.Vendedor))
            return Forbid();

        var usuarioId = ObtenerUsuarioIdRequerido();
        var anuncios = await Mediator.Send(new ObtenerAnunciosVendedorQuery(usuarioId));
        return Ok(anuncios);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerPerfilPublico(int id)
    {
        var perfil = await Mediator.Send(new ObtenerPerfilPublicoVendedorQuery(id));

        if (perfil is null)
            return NotFound(new { mensaje = "Vendedor no encontrado." });

        return Ok(perfil);
    }
}
