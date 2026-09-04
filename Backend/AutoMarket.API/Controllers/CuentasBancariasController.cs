using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/cuentas-bancarias")]
[ApiController]
public class CuentasBancariasController : ControllerBase
{
    private readonly ICuentasBancariasService _cuentasBancariasService;

    public CuentasBancariasController(ICuentasBancariasService cuentasBancariasService)
    {
        _cuentasBancariasService = cuentasBancariasService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCuentasActivas()
    {
        var cuentas = await _cuentasBancariasService.ObtenerCuentasActivasAsync();
        return Ok(cuentas);
    }
}
