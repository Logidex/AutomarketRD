using AutoMarket.Application.DTOs;
using AutoMarket.API.Extensions;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador para gestionar Leads.
/// </summary>
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

/// <summary>
/// Inicializa una nueva instancia de la clase LeadsController. Parámetro leadService (ILeadService)
/// </summary>
    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLeads")]
    public async Task<IActionResult> CrearLead([FromBody] LeadCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _leadService.CrearLeadAsync(dto, User.ObtenerUsuarioIdOpcional());
        
        return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente al vendedor." });
    }

    [HttpGet("anuncio/{anuncioId}")]
    [Authorize]
    public async Task<IActionResult> ObtenerPorAnuncio(int anuncioId)
    {
        var usuarioId = User.ObtenerUsuarioId();

        try
        {
            var leads = await _leadService.ObtenerLeadsPorAnuncioAsync(anuncioId, usuarioId);
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
        var dealerId = User.ObtenerUsuarioId();

        var leads = await _leadService.ObtenerLeadsPorDealerAsync(dealerId);
        return Ok(leads);
    }

    // Historial del comprador: vehículos que contactó
    [HttpGet("mis-contactos")]
    [Authorize]
    public async Task<IActionResult> ObtenerMisContactos()
    {
        var usuarioId = User.ObtenerUsuarioId();

        var contactos = await _leadService.ObtenerMisContactosAsync(usuarioId);
        return Ok(contactos);
    }

    // Resumen para el campanario del dealer/vendedor: no leídos + últimos
    [HttpGet("resumen-no-leidos")]
    [Authorize]
    public async Task<IActionResult> ObtenerResumenNoLeidos()
    {
        var usuarioId = User.ObtenerUsuarioId();

        var resumen = await _leadService.ObtenerResumenNoLeidosAsync(usuarioId);
        return Ok(resumen);
    }

    [HttpPatch("marcar-todos-leido")]
    [Authorize]
    public async Task<IActionResult> MarcarTodosLeido()
    {
        var dealerId = User.ObtenerUsuarioId();

        var cantidad = await _leadService.MarcarTodosLeidosAsync(dealerId);
        return Ok(new { mensaje = "Leads marcados como leídos.", cantidad });
    }

    [HttpPatch("{id:int}/leido")]
    [Authorize]
    public async Task<IActionResult> MarcarLeido(int id)
    {
        var dealerId = User.ObtenerUsuarioId();

        try
        {
            var marcado = await _leadService.MarcarLeidoAsync(id, dealerId);

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
