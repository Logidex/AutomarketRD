using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.DealerVendedor)]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearTicket([FromBody] TicketCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioId = User.ObtenerUsuarioId();
        var ticketId = await _ticketService.CrearTicketAsync(dto, usuarioId);

        return CreatedAtAction(nameof(ObtenerTicket), new { id = ticketId }, new
        {
            mensaje = "Tu ticket de soporte fue creado. Te responderemos pronto.",
            ticketId
        });
    }

    [HttpGet("mis-tickets")]
    public async Task<IActionResult> ObtenerMisTickets()
    {
        var usuarioId = User.ObtenerUsuarioId();
        var tickets = await _ticketService.ObtenerMisTicketsAsync(usuarioId);
        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerTicket(int id)
    {
        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            var ticket = await _ticketService.ObtenerTicketAsync(id, usuarioId);
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

        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            await _ticketService.ResponderTicketAsync(id, dto, usuarioId);
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
        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            await _ticketService.CerrarTicketAsync(id, usuarioId);
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
