using AutoMarket.API.Constants;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Features.Suscripciones.Commands;
using AutoMarket.Application.Features.Suscripciones.Queries;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/suscripciones")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminSuscripcionesController : BaseApiController
{
    public AdminSuscripcionesController(IMediator mediator) : base(mediator) { }

    [HttpPut("~/api/admin/suscripciones/{dealerId:int}/plan")]
    public async Task<IActionResult> CambiarPlanForzoso(int dealerId, [FromBody] CambiarPlanAdminDto dto)
    {
        await Mediator.Send(new CambiarPlanCommand(dealerId, dto.NuevoNivel, CicloFacturacion.Mensual));
        return Ok(new { exito = true, mensaje = $"Plan del dealer {dealerId} actualizado a {dto.NuevoNivel}." });
    }

    [HttpPut("~/api/admin/suscripciones/{dealerId:int}/renovar")]
    public async Task<IActionResult> RenovarSuscripcionManual(int dealerId, [FromBody] RenovarSuscripcionDto dto)
    {
        var fechaUtc = dto.NuevaFechaVencimiento.ToUniversalTime();
        await Mediator.Send(new RenovarManualCommand(dealerId, fechaUtc));
        return Ok(new { exito = true, mensaje = $"Suscripción extendida y activada hasta {fechaUtc:dd/MM/yyyy}." });
    }

    [HttpGet("~/api/admin/pagos")]
    public async Task<IActionResult> ListarPagos()
    {
        var pagos = await Mediator.Send(new ObtenerPagosAdminQuery());
        return Ok(pagos);
    }

    [HttpPost("~/api/admin/pagos/{id:int}/reembolsar")]
    public async Task<IActionResult> ReembolsarPago(int id)
    {
        try
        {
            var metodo = await Mediator.Send(new ReembolsarPagoCommand(id));
            var mensaje = metodo == MetodoPago.Transferencia
                ? "Reembolso registrado. Recuerda devolver el dinero manualmente al dealer. Su suscripción fue cancelada."
                : "Reembolso procesado en PayPal. La suscripción del dealer fue cancelada.";
            return Ok(new { exito = true, mensaje });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpGet("~/api/admin/transferencias")]
    public async Task<IActionResult> ListarTransferenciasPendientes()
    {
        var transferencias = await Mediator.Send(new ObtenerTransferenciasPendientesQuery());
        return Ok(transferencias);
    }

    [HttpPost("~/api/admin/transferencias/{id:int}/aprobar")]
    public async Task<IActionResult> AprobarTransferencia(int id, [FromBody] NotasTransferenciaDto? dto = null)
    {
        try
        {
            await Mediator.Send(new AprobarTransferenciaCommand(id, dto?.Notas));
            return Ok(new { exito = true, mensaje = $"La transferencia {id} fue aprobada. La suscripción del dealer fue extendida." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPost("~/api/admin/transferencias/{id:int}/rechazar")]
    public async Task<IActionResult> RechazarTransferencia(int id, [FromBody] NotasTransferenciaDto? dto = null)
    {
        try
        {
            await Mediator.Send(new RechazarTransferenciaCommand(id, dto?.Notas));
            return Ok(new { exito = true, mensaje = $"La transferencia {id} fue rechazada. La suscripción del dealer fue revocada." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }
}
