using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CatalogoController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogoController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerAnuncios([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
    {
        var resultado = await _catalogoService.ObtenerCatalogoPaginadoAsync(pagina, tamanoPagina);
        
        return Ok(resultado);
    }
}