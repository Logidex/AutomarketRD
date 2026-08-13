using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
/// <summary>
/// Controlador para gestionar Planes.
/// </summary>
public class PlanesController : ControllerBase
{
    private readonly IPlanCatalogoService _planCatalogoService;

/// <summary>
/// Inicializa una nueva instancia de la clase PlanesController. Parámetro planCatalogoService (IPlanCatalogoService)
/// </summary>
    public PlanesController(IPlanCatalogoService planCatalogoService)
    {
        _planCatalogoService = planCatalogoService;
    }

    /// <summary>Catálogo público de planes con sus precios por ciclo (RD$).</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCatalogo()
    {
        var planes = await _planCatalogoService.ObtenerCatalogoPublicoAsync();
        return Ok(planes);
    }
}
