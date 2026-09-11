using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.Dashboard.Queries;
using AutoMarket.Application.Features.PerfilDealer.Commands;
using AutoMarket.Application.Features.PerfilDealer.Queries;
using AutoMarket.Application.Features.Suscripciones.Commands;
using AutoMarket.Application.Features.Suscripciones.Queries;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealersController : BaseApiController
{
    public DealersController(IMediator mediator) : base(mediator) { }

    [HttpGet("{dealerId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerPerfilPublico(int dealerId)
    {
        var perfil = await Mediator.Send(new ObtenerPerfilPublicoQuery(dealerId));
        if (perfil is null)
            return NotFound(new { mensaje = "Dealer no encontrado." });
        return Ok(perfil);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListarAgencias(
        [FromQuery] string? busqueda,
        [FromQuery] bool? soloVerificadas,
        [FromQuery] string? planNivel,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidadPorPagina = 12)
    {
        var resultado = await Mediator.Send(new ListarAgenciasQuery(busqueda, soloVerificadas, planNivel, pagina, cantidadPorPagina));
        return Ok(resultado);
    }

    [HttpPut("me")]
    [Authorize(Roles = Roles.Dealer)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ActualizarMiPerfil([FromForm] PerfilDealerUpdateDto dto)
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();
        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        try
        {
            var perfilActualizado = await Mediator.Send(new ActualizarMiPerfilCommand(dealerId.Value, dto));
            if (perfilActualizado is null)
                return NotFound(new { mensaje = "No existe un perfil de dealer asociado a este usuario." });
            return Ok(perfilActualizado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("me/dashboard-resumen")]
    [Authorize(Roles = Roles.Dealer)]
    public async Task<IActionResult> ObtenerDashboardResumen()
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();
        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        var resumen = await Mediator.Send(new ObtenerResumenDashboardQuery(dealerId.Value));
        return Ok(resumen);
    }

    [HttpGet("me/suscripcion")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> ObtenerMiSuscripcion()
    {
        var usuarioId = User.ObtenerUsuarioIdOpcional();
        var dealerId = User.ObtenerDealerIdOpcional();

        if (usuarioId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        var suscripcion = dealerId.HasValue
            ? await Mediator.Send(new ObtenerSuscripcionQuery(dealerId.Value))
            : await Mediator.Send(new ObtenerSuscripcionPorUsuarioIdQuery(usuarioId.Value));

        if (suscripcion is null)
            return NotFound(new { mensaje = "El usuario aún no posee una suscripción." });

        return Ok(suscripcion);
    }

    [HttpGet("me/suscripcion/pagos")]
    [Authorize(Roles = Roles.Dealer)]
    public async Task<IActionResult> ObtenerHistorialPagos()
    {
        var dealerId = User.ObtenerUsuarioIdOpcional();
        if (dealerId is null)
            return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

        var pagos = await Mediator.Send(new ObtenerHistorialPagosQuery(dealerId.Value));
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
            await Mediator.Send(new CancelarSuscripcionCommand(dealerId.Value));
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
