using AutoMarket.API.Constants;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Features.PlanCatalogo.Commands;
using AutoMarket.Application.Features.PlanCatalogo.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminPlanesController : BaseApiController
{
    public AdminPlanesController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> ListarPlanes()
    {
        var planes = await Mediator.Send(new ObtenerCatalogoAdminQuery());
        return Ok(planes);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPlan([FromBody] PlanCatalogoCreateDto dto)
    {
        var plan = await Mediator.Send(new CrearPlanCommand(dto));
        return CreatedAtAction(nameof(ListarPlanes), new { }, plan);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarPlan(int id, [FromBody] PlanCatalogoUpdateDto dto)
    {
        try
        {
            var plan = await Mediator.Send(new ActualizarPlanCommand(id, dto));
            return Ok(plan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarPlan(int id)
    {
        try
        {
            await Mediator.Send(new EliminarPlanCommand(id));
            return Ok(new { exito = true, mensaje = "Plan desactivado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }
}
