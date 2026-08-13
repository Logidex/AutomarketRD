using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador para gestionar Dealers.
/// </summary>
public class DealersController : ControllerBase
{
    private readonly IPerfilDealerService _perfilDealerService;
    private readonly IDashboardService _dashboardService;
    private readonly ISuscripcionService _suscripcionService;

/// <summary>
/// Inicializa una nueva instancia de la clase DealersController.
/// </summary>
    public DealersController(
        IPerfilDealerService perfilDealerService,
        IDashboardService dashboardService,
        ISuscripcionService suscripcionService)
    {
        _perfilDealerService = perfilDealerService;
        _dashboardService = dashboardService;
        _suscripcionService = suscripcionService;
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
    [Authorize(Roles = Roles.Dealer)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ActualizarMiPerfil(
        [FromForm] PerfilDealerUpdateDto dto)
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();

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

    [HttpGet("me/dashboard-resumen")]
    [Authorize(Roles = Roles.Dealer)]
    public async Task<IActionResult> ObtenerDashboardResumen()
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();

        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        var resumen = await _dashboardService.ObtenerResumenAsync(dealerId.Value);
        return Ok(resumen);
    }

    [HttpGet("me/suscripcion")]
    [Authorize(Roles = Roles.Dealer)]
    public async Task<IActionResult> ObtenerMiSuscripcion()
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();

        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        var suscripcion = await _suscripcionService.ObtenerSuscripcionAsync(dealerId.Value);

        if (suscripcion is null)
            return NotFound(new { mensaje = "El dealer aún no posee una suscripción." });

        return Ok(suscripcion);
    }

    [HttpGet("me/suscripcion/pagos")]
    [Authorize(Roles = Roles.Dealer)]
    public async Task<IActionResult> ObtenerHistorialPagos()
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();

        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        var pagos = await _suscripcionService.ObtenerHistorialPagosAsync(dealerId.Value);

        return Ok(pagos);
    }

    [HttpPost("me/suscripcion/cancelar")]
    [Authorize(Roles = Roles.Dealer)]
    public async Task<IActionResult> CancelarMiSuscripcion()
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();

        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        try
        {
            await _suscripcionService.CancelarSuscripcionAsync(dealerId.Value);
            return Ok(new { mensaje = "Suscripción cancelada correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
