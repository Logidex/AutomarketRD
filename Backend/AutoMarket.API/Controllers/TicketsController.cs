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

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.DealerVendedor)]
public class TicketsController : BaseApiController
{
    public TicketsController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    public async Task<IActionResult> CrearTicket([FromBody] TicketCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ticketId = await Mediator.Send(new CrearTicketCommand(dto, ObtenerUsuarioIdRequerido()));

        return CreatedAtAction(nameof(ObtenerTicket), new { id = ticketId }, new
        {
            mensaje = "Tu ticket de soporte fue creado. Te responderemos pronto.",
            ticketId
        });
    }

    [HttpGet("mis-tickets")]
    public async Task<IActionResult> ObtenerMisTickets()
    {
        var tickets = await Mediator.Send(new ObtenerMisTicketsQuery(ObtenerUsuarioIdRequerido()));
        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerTicket(int id)
    {
        try
        {
            var ticket = await Mediator.Send(new ObtenerTicketQuery(id, ObtenerUsuarioIdRequerido()));
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/mensajes")]
    public async Task<IActionResult> Responder(int id, [FromBody] TicketMensajeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await Mediator.Send(new AgregarMensajeTicketCommand(id, dto, ObtenerUsuarioIdRequerido()));
            return Ok(new { mensaje = "Tu respuesta fue enviada al equipo de soporte." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/cerrar")]
    public async Task<IActionResult> Cerrar(int id)
    {
        try
        {
            await Mediator.Send(new CerrarTicketCommand(id, ObtenerUsuarioIdRequerido()));
            return Ok(new { mensaje = "El ticket fue cerrado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
