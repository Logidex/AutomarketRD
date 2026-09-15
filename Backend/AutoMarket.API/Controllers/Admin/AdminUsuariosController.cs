using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/usuarios")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminUsuariosController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IAdminUsuarioService _adminUsuarioService;

    public AdminUsuariosController(
        IDashboardService dashboardService,
        IAdminUsuarioService adminUsuarioService)
    {
        _dashboardService = dashboardService;
        _adminUsuarioService = adminUsuarioService;
    }

    [HttpGet("~/api/admin/dashboard/resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resumen = await _dashboardService.ObtenerResumenAsync();
        return Ok(resumen);
    }

    [HttpGet]
    public async Task<IActionResult> ListarUsuarios([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
    {
        var (items, total) = await _adminUsuarioService.ListarUsuariosPaginadosAsync(pagina, tamanoPagina);
        return Ok(new
        {
            items,
            totalRegistros = total,
            paginaActual = pagina,
            cantidadPorPagina = tamanoPagina,
            totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina)
        });
    }

    [HttpPatch("{id:int}/suspender")]
    public async Task<IActionResult> SuspenderUsuario(int id)
    {
        try
        {
            var adminId = User.ObtenerUsuarioId();
            var usuario = await _adminUsuarioService.SuspenderUsuarioAsync(id, adminId);
            if (usuario is null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(new { exito = true, mensaje = $"El usuario {usuario.Email} ha sido suspendido exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:int}/reactivar")]
    public async Task<IActionResult> ReactivarUsuario(int id)
    {
        try
        {
            var usuario = await _adminUsuarioService.ReactivarUsuarioAsync(id);
            if (usuario is null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(new { exito = true, mensaje = $"El usuario {usuario.Email} ha sido reactivado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        try
        {
            var adminId = User.ObtenerUsuarioId();
            var usuario = await _adminUsuarioService.EliminarUsuarioAsync(id, adminId);
            if (usuario is null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(new { exito = true, mensaje = $"El usuario {usuario.Email} fue eliminado definitivamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}/rol")]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolAdminDto dto)
    {
        try
        {
            var usuario = await _adminUsuarioService.CambiarRolAsync(id, dto);
            if (usuario is null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(new
            {
                exito = true,
                mensaje = $"El rol de {usuario.Email} ahora es {usuario.Rol}.",
                usuario
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
