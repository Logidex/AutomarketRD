using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepo;

    public AdminAuditLogsController(IAuditLogRepository auditLogRepo)
    {
        _auditLogRepo = auditLogRepo;
    }

    /// <summary>
    /// Lista logs de auditoría con filtros opcionales.
    /// Solo accesible para administradores.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarLogs(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] string? entidad = null,
        [FromQuery] int? usuarioId = null)
    {
        var (items, total) = await _auditLogRepo.ObtenerPaginadosAsync(
            pagina, tamanoPagina, entidad, usuarioId);

        return Ok(new
        {
            items,
            totalRegistros = total,
            paginaActual = pagina,
            tamanoPagina,
            totalPaginas = (int)Math.Ceiling((double)total / tamanoPagina)
        });
    }
}
