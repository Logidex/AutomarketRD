using System.Security.Claims;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealersController : ControllerBase
{
    private readonly IPerfilDealerService _perfilDealerService;

    public DealersController(IPerfilDealerService perfilDealerService)
    {
        _perfilDealerService = perfilDealerService;
    }

    [HttpGet("{dealerId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerPerfilPublico(int dealerId)
    {
        var perfil = await _perfilDealerService
            .ObtenerPerfilPublicoAsync(dealerId);

        if (perfil is null)
        {
            return NotFound(new
            {
                mensaje = "Dealer no encontrado."
            });
        }

        return Ok(perfil);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Dealer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ActualizarMiPerfil(
        [FromForm] PerfilDealerUpdateDto dto)
    {
        var dealerId = ObtenerUsuarioIdDelToken();

        if (dealerId is null)
        {
            return Unauthorized(new
            {
                mensaje = "Token inválido o usuario no identificado."
            });
        }

        try
        {
            var perfilActualizado = await _perfilDealerService
                .ActualizarMiPerfilAsync(dealerId.Value, dto);

            if (perfilActualizado is null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un perfil de dealer asociado a este usuario."
                });
            }

            return Ok(perfilActualizado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    private int? ObtenerUsuarioIdDelToken()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(idClaim, out var usuarioId)
            ? usuarioId
            : null;
    }
}