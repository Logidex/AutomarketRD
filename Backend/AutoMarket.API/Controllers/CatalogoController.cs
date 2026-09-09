using AutoMarket.Application.Features.Catalogo.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CatalogoController : BaseApiController
{
    public CatalogoController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> ObtenerAnuncios([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
    {
        var resultado = await Mediator.Send(new ObtenerCatalogoPaginadoQuery(pagina, tamanoPagina));
        return Ok(resultado);
    }
}
