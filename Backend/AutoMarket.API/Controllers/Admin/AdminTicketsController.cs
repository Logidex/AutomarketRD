using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Application.Features.Tickets.Commands;
using AutoMarket.Application.Features.Tickets.Queries;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminTicketsController : BaseApiController
{
    public AdminTicketsController(IMediator mediator) : base(mediator) { }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumenTickets()
    {
        var resumen = await Mediator.Send(new ObtenerResumenAdminQuery());
        return Ok(resumen);
    }

    [HttpGet]
    public async Task<IActionResult> ListarTickets()
    {
        var tickets = await Mediator.Send(new ObtenerTicketsAdminQuery());
        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerTicketAdmin(int id)
    {
        try
        {
            var ticket = await Mediator.Send(new ObtenerTicketAdminQuery(id));
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/mensajes")]
    public async Task<IActionResult> ResponderTicket(int id, [FromBody] TicketMensajeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await Mediator.Send(new AgregarMensajeAdminTicketCommand(id, dto, ObtenerUsuarioIdRequerido()));
            return Ok(new { exito = true, mensaje = "Respuesta enviada al cliente." });
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

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoTicket(int id, [FromBody] CambiarEstadoTicketDto dto)
    {
        try
        {
            await Mediator.Send(new CambiarEstadoTicketCommand(id, dto));
            return Ok(new { exito = true, mensaje = $"El ticket pasó a estado {dto.NuevoEstado}." });
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
