using AutoMarket.API.Extensions;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
/// <summary>
/// Controlador para gestionar Historial.
/// </summary>
public class HistorialController : ControllerBase
{
    private readonly IHistorialVistaService _historialService;

/// <summary>
/// Inicializa una nueva instancia de la clase HistorialController. Parámetro historialService (IHistorialVistaService)
/// </summary>
    public HistorialController(IHistorialVistaService historialService)
    {
        _historialService = historialService;
    }

    // Registra que el comprador abrió el detalle de un anuncio
    [HttpPost("{anuncioId:int}")]
    public async Task<IActionResult> RegistrarVista(int anuncioId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        await _historialService.RegistrarVistaAsync(usuarioId, anuncioId);
        return Ok(new { exito = true });
    }

    // Últimos vehículos vistos por el comprador
    [HttpGet("recientes")]
    public async Task<IActionResult> ObtenerRecientes([FromQuery] int cantidad = 12)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var resultado = await _historialService.ObtenerRecientesAsync(usuarioId, cantidad);
        return Ok(resultado);
    }
}
