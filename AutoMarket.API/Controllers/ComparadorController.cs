using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ComparadorController : ControllerBase
{
    private readonly IComparadorService _comparadorService;

    public ComparadorController(IComparadorService comparadorService)
    {
        _comparadorService = comparadorService;
    }

    [HttpGet]
    public async Task<IActionResult> CompararVehiculos([FromQuery] int[] ids)
    {
        try
        {
            var resultado = await _comparadorService.CompararVehiculosAsync(ids);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}