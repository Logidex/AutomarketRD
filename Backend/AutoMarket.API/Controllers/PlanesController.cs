using AutoMarket.Application.Features.PlanCatalogo.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlanesController : BaseApiController
{
    public PlanesController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> ObtenerCatalogo()
    {
        var planes = await Mediator.Send(new ObtenerCatalogoPublicoQuery());
        return Ok(planes);
    }
}
