using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Leads.Commands;
using AutoMarket.Application.Features.Leads.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : BaseApiController
{
    public LeadsController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLeads")]
    public async Task<IActionResult> CrearLead([FromBody] LeadCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await Mediator.Send(new CrearLeadCommand(dto, User.ObtenerUsuarioIdOpcional()));
        return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente al vendedor." });
    }

    [HttpGet("anuncio/{anuncioId}")]
    [Authorize]
    public async Task<IActionResult> ObtenerPorAnuncio(int anuncioId)
    {
        try
        {
            var leads = await Mediator.Send(new ObtenerLeadsPorAnuncioQuery(anuncioId, ObtenerUsuarioIdRequerido()));
            return Ok(leads);
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

    [HttpGet("mis-leads")]
    [Authorize]
    public async Task<IActionResult> ObtenerMisLeads()
    {
        var leads = await Mediator.Send(new ObtenerLeadsPorDealerQuery(ObtenerUsuarioIdRequerido()));
        return Ok(leads);
    }

    [HttpGet("mis-contactos")]
    [Authorize]
    public async Task<IActionResult> ObtenerMisContactos()
    {
        var contactos = await Mediator.Send(new ObtenerMisContactosQuery(ObtenerUsuarioIdRequerido()));
        return Ok(contactos);
    }

    [HttpGet("resumen-no-leidos")]
    [Authorize]
    public async Task<IActionResult> ObtenerResumenNoLeidos()
    {
        var resumen = await Mediator.Send(new ObtenerResumenNoLeidosQuery(ObtenerUsuarioIdRequerido()));
        return Ok(resumen);
    }

    [HttpPatch("marcar-todos-leido")]
    [Authorize]
    public async Task<IActionResult> MarcarTodosLeido()
    {
        var cantidad = await Mediator.Send(new MarcarTodosLeidosCommand(ObtenerUsuarioIdRequerido()));
        return Ok(new { mensaje = "Leads marcados como leídos.", cantidad });
    }

    [HttpPatch("{id:int}/leido")]
    [Authorize]
    public async Task<IActionResult> MarcarLeido(int id)
    {
        try
        {
            var marcado = await Mediator.Send(new MarcarLeidoCommand(id, ObtenerUsuarioIdRequerido()));
            if (!marcado)
                return NotFound(new { mensaje = "El lead no fue encontrado." });

            return Ok(new { mensaje = "Lead marcado como leído." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }
}
