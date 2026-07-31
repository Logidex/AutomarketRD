using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class FavoritosController : ControllerBase
{
    private readonly IFavoritoService _favoritoService;

    public FavoritosController(IFavoritoService favoritoService)
    {
        _favoritoService = favoritoService;
    }

    private int ObtenerUsuarioId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost("{anuncioId:int}")]
    public async Task<IActionResult> AgregarFavorito(int anuncioId)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            await _favoritoService.AgregarFavoritoAsync(usuarioId, anuncioId);
            return Ok(new { exito = true, mensaje = "Vehículo agregado a favoritos ❤️" });
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
            var usuarioId = ObtenerUsuarioId();
            await _favoritoService.QuitarFavoritoAsync(usuarioId, anuncioId);
            return Ok(new { exito = true, mensaje = "Vehículo removido de favoritos 💔" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMisFavoritos()
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _favoritoService.ObtenerFavoritosAsync(usuarioId);
        return Ok(resultado);
    }
}