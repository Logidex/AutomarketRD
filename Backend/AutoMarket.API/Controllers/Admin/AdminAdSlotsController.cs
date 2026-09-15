using AutoMarket.API.Constants;
using AutoMarket.Application.DTOs.AdSlots;
using AutoMarket.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers.Admin;

[Route("api/admin/adslots")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminAdSlotsController : BaseApiController
{
    private readonly IAdSlotService _adSlotService;

    public AdminAdSlotsController(IMediator mediator, IAdSlotService adSlotService) : base(mediator)
    {
        _adSlotService = adSlotService;
    }

    // ==========================================
    // SLOTS
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> ObtenerSlots()
    {
        var slots = await _adSlotService.ObtenerTodosLosSlotsAsync();
        return Ok(slots);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerSlotPorId(int id)
    {
        try
        {
            var slot = await _adSlotService.ObtenerSlotPorIdAsync(id);
            return Ok(slot);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearSlot([FromBody] CrearAdSlotAdminDto dto)
    {
        try
        {
            var slot = await _adSlotService.CrearSlotAsync(dto);
            return CreatedAtAction(nameof(ObtenerSlotPorId), new { id = slot.Id }, slot);
        }
        catch (Exception ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarSlot(int id, [FromBody] ActualizarAdSlotAdminDto dto)
    {
        try
        {
            var slot = await _adSlotService.ActualizarSlotAsync(id, dto);
            return Ok(slot);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarSlot(int id)
    {
        try
        {
            await _adSlotService.EliminarSlotAsync(id);
            return Ok(new { exito = true, mensaje = "Slot desactivado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    // ==========================================
    // ANUNCIOS
    // ==========================================

    [HttpGet("anuncios")]
    public async Task<IActionResult> ObtenerTodosLosAnuncios()
    {
        var anuncios = await _adSlotService.ObtenerTodosLosAnunciosAsync();
        return Ok(anuncios);
    }

    [HttpGet("anuncios/{id:int}")]
    public async Task<IActionResult> ObtenerAnuncioPorId(int id)
    {
        try
        {
            var anuncio = await _adSlotService.ObtenerAnuncioAdminPorIdAsync(id);
            return Ok(anuncio);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPost("anuncios/{id:int}/rechazar")]
    public async Task<IActionResult> RechazarAnuncio(int id)
    {
        try
        {
            await _adSlotService.RechazarAnuncioAsync(id);
            return Ok(new { exito = true, mensaje = "Anuncio rechazado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpDelete("anuncios/{id:int}")]
    public async Task<IActionResult> EliminarAnuncio(int id)
    {
        try
        {
            await _adSlotService.EliminarAnuncioAsync(id);
            return Ok(new { exito = true, mensaje = "Anuncio eliminado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpGet("anuncios/por-vencer")]
    public async Task<IActionResult> ObtenerAnunciosPorVencer([FromQuery] int diasAntes = 3)
    {
        var anuncios = await _adSlotService.ObtenerAnunciosPorVencerAsync(diasAntes);
        return Ok(anuncios);
    }
}
