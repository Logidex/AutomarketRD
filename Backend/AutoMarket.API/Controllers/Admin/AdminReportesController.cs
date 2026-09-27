using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.Features.ReportesAnuncio.Commands;
using AutoMarket.Application.Features.ReportesAnuncio.Queries;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/reportes")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminReportesController : BaseApiController
{
    public AdminReportesController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> ListarReportes([FromQuery] string estado = "Pendiente")
    {
        if (!Enum.TryParse<ReporteEstado>(estado, ignoreCase: true, out var reporteEstado))
            return BadRequest(new { mensaje = $"Estado inválido: {estado}. Usa Pendiente, Descartado o Resuelto." });

        var reportes = await Mediator.Send(new ListarReportesPorEstadoQuery(reporteEstado));
        return Ok(reportes);
    }

    [HttpGet("pendientes/contador")]
    public async Task<IActionResult> ContarReportesPendientes()
    {
        var total = await Mediator.Send(new ContarReportesPendientesQuery());
        return Ok(new { total });
    }

    [HttpPatch("{id:int}/descartar")]
    public async Task<IActionResult> DescartarReporte(int id)
    {
        try
        {
            await Mediator.Send(new DescartarReporteCommand(id, ObtenerUsuarioIdRequerido()));
            return Ok(new { exito = true, mensaje = "Reporte descartado." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:int}/resolver")]
    public async Task<IActionResult> ResolverReporte(int id)
    {
        try
        {
            await Mediator.Send(new ResolverReporteCommand(id, ObtenerUsuarioIdRequerido()));
            return Ok(new { exito = true, mensaje = "Reporte resuelto: el anuncio fue eliminado." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
