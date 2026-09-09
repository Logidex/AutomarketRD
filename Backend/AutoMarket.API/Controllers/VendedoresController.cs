using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/vendedores")]
[Authorize]
public class VendedoresController : ControllerBase
{
    private readonly IVendedorService _vendedorService;

    public VendedoresController(IVendedorService vendedorService)
    {
        _vendedorService = vendedorService;
    }

    [HttpGet("me/suscripcion")]
    public async Task<IActionResult> ObtenerMiSuscripcion()
    {
        if (!User.IsInRole(Roles.Vendedor))
            return Forbid();

        var usuarioId = User.ObtenerUsuarioId();
        var suscripcion = await _vendedorService.ObtenerMiSuscripcionAsync(usuarioId);

        if (suscripcion is null)
            return NotFound(new { mensaje = "No se encontró información de suscripción." });

        return Ok(suscripcion);
    }

    [HttpGet("me/anuncios")]
    public async Task<IActionResult> ObtenerMisAnuncios()
    {
        if (!User.IsInRole(Roles.Vendedor))
            return Forbid();

        var usuarioId = User.ObtenerUsuarioId();
        var anuncios = await _vendedorService.ObtenerAnunciosDelVendedorAsync(usuarioId);
        return Ok(anuncios);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerPerfilPublico(int id)
    {
        var perfil = await _vendedorService.ObtenerPerfilPublicoAsync(id);

        if (perfil is null)
            return NotFound(new { mensaje = "Vendedor no encontrado." });

        return Ok(perfil);
    }
}
