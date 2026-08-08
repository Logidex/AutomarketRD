using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlanesController : ControllerBase
{
    private readonly IPlanCatalogoService _planCatalogoService;

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