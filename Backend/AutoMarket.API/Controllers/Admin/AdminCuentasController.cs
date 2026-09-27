using AutoMarket.API.Constants;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Features.CuentasBancarias.Commands;
using AutoMarket.Application.Features.CuentasBancarias.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/cuentas-bancarias")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminCuentasController : BaseApiController
{
    public AdminCuentasController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> ListarCuentasBancarias()
    {
        var cuentas = await Mediator.Send(new ObtenerTodasCuentasQuery());
        return Ok(cuentas);
    }

    [HttpPost]
    public async Task<IActionResult> CrearCuentaBancaria([FromBody] CrearCuentaBancariaDto dto)
    {
        var cuenta = await Mediator.Send(new CrearCuentaBancariaCommand(
            dto.Banco, dto.NombreTitular, dto.NumeroCuenta,
            dto.TipoCuenta, dto.Documento, dto.ConceptoReferencia));
        return CreatedAtAction(nameof(ListarCuentasBancarias), new { }, cuenta);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarCuentaBancaria(int id, [FromBody] CrearCuentaBancariaDto dto)
    {
        try
        {
            var cuenta = await Mediator.Send(new ActualizarCuentaBancariaCommand(
                id, dto.NombreTitular, dto.NumeroCuenta,
                dto.TipoCuenta, dto.Documento, dto.ConceptoReferencia));
            return Ok(cuenta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> ToggleCuentaBancaria(int id)
    {
        try
        {
            var cuenta = await Mediator.Send(new ToggleCuentaBancariaCommand(id));
            return Ok(cuenta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }
}
