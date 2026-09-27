using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMarket.Application.Interfaces;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/visit-counter")]
public class VisitCounterController : ControllerBase
{
    private readonly IVisitCounterService _service;

    public VisitCounterController(IVisitCounterService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerConteo()
    {
        var total = await _service.ObtenerTotalAsync();
        return Ok(new { totalVisitas = total });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Incrementar()
    {
        var total = await _service.IncrementarAsync();
        return Ok(new { totalVisitas = total });
    }
}
