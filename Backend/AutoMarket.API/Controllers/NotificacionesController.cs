using System.Security.Claims;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionRepository _notificacionRepo;

    public NotificacionesController(INotificacionRepository notificacionRepo)
    {
        _notificacionRepo = notificacionRepo;
    }

    private int UsuarioId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// Lista notificaciones del usuario actual con paginación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        var (items, total) = await _notificacionRepo.ObtenerPorUsuarioAsync(
            UsuarioId, pagina, tamanoPagina);

        return Ok(new
        {
            items,
            totalRegistros = total,
            paginaActual = pagina,
            tamanoPagina,
            totalPaginas = (int)Math.Ceiling((double)total / tamanoPagina)
        });
    }

    /// <summary>
    /// Cantidad de notificaciones no leídas (para badge/campana).
    /// </summary>
    [HttpGet("no-leidas")]
    public async Task<IActionResult> NoLeidas()
    {
        var total = await _notificacionRepo.ContarNoLeidasAsync(UsuarioId);
        return Ok(new { total });
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [HttpPut("{id}/leida")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        await _notificacionRepo.MarcarComoLeidaAsync(id, UsuarioId);
        return Ok();
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas.
    /// </summary>
    [HttpPut("leer-todas")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        await _notificacionRepo.MarcarTodasComoLeidasAsync(UsuarioId);
        return Ok();
    }
}
