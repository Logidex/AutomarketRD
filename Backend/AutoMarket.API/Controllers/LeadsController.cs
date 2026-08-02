using AutoMarket.Application.DTOs;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    // =========================================================================
    // ENDPOINT 1: Crear un nuevo Lead (Público)
    // POST: api/leads
    // =========================================================================
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLeads")]
    public async Task<IActionResult> CrearLead([FromBody] LeadCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _leadService.CrearLeadAsync(dto);
        
        return Ok(new { mensaje = "Tu mensaje ha sido enviado exitosamente al vendedor." });
    }

    // =========================================================================
    // ENDPOINT 2: Ver Leads por Anuncio (Protegido)
    // GET: api/leads/anuncio/{anuncioId}
    // =========================================================================
    [HttpGet("anuncio/{anuncioId}")]
    [Authorize] // 👈 Solo usuarios logueados (Dealers/Vendedores)
    public async Task<IActionResult> ObtenerPorAnuncio(int anuncioId)
    {
        var leads = await _leadService.ObtenerLeadsPorAnuncioAsync(anuncioId);
        return Ok(leads);
    }

    // =========================================================================
    // ENDPOINT 3: Dashboard del Dealer - Ver todos sus Leads (Protegido)
    // GET: api/leads/mis-leads
    // =========================================================================
    [HttpGet("mis-leads")]
    [Authorize] // 👈 Requisito para el panel privado del Dealer
    public async Task<IActionResult> ObtenerMisLeads()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int dealerId))
        {
            return Unauthorized(new { mensaje = "Usuario no válido o sesión expirada." });
        }

        var leads = await _leadService.ObtenerLeadsPorDealerAsync(dealerId);
        return Ok(leads);
    }
}