using AutoMarket.Application.Features.CuentasBancarias.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/cuentas-bancarias")]
[ApiController]
public class CuentasBancariasController : BaseApiController
{
    public CuentasBancariasController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> ObtenerCuentasActivas()
    {
        var cuentas = await Mediator.Send(new ObtenerCuentasActivasQuery());
        return Ok(cuentas);
    }
}
