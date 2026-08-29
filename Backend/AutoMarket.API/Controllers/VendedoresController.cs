using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/vendedores")]
[Authorize]
public class VendedoresController : ControllerBase
{
    private bool EsVendedor()
    {
        var rolClaim = User.FindFirst(ClaimTypes.Role) 
            ?? User.FindFirst("role")
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role");
        return rolClaim?.Value == Roles.Vendedor;
    }

    [HttpGet("me/suscripcion")]
    public IActionResult ObtenerMiSuscripcion()
    {
        if (!EsVendedor())
        {
            return Forbid();
        }

        var usuarioId = User.ObtenerUsuarioId();

        return Ok(new
        {
            nivel = "Gratis",
            ciclo = "Mensual",
            estado = "Activa",
            limiteAnuncios = 1,
            cuotaDestacados = 0,
            maxFotos = 8,
            fechaInicioUtc = DateTime.UtcNow,
            fechaVencimientoUtc = DateTime.UtcNow.AddYears(10),
            diasRestantes = 3650,
            activa = true
        });
    }

    [HttpGet("me/anuncios")]
    public IActionResult ObtenerMisAnuncios()
    {
        if (!EsVendedor())
        {
            return Forbid();
        }

        var usuarioId = User.ObtenerUsuarioId();
        return Ok(new { mensaje = "Endpoint para listar anuncios del vendedor", usuarioId });
    }
}
