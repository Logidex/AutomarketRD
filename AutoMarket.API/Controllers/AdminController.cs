using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IUsuarioRepository _usuarioRepository;

    public AdminController(IDashboardService dashboardService, IUsuarioRepository usuarioRepository)
    {
        _dashboardService = dashboardService;
        _usuarioRepository = usuarioRepository;
    }

    [HttpGet("dashboard/resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resumen = await _dashboardService.ObtenerResumenAsync();
        return Ok(resumen);
    }

    [HttpPatch("usuarios/{id:int}/suspender")]
    public async Task<IActionResult> SuspenderUsuario(int id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (usuario.Rol == "Admin")
            return BadRequest(new { mensaje = "No puedes suspender a otro administrador." });

        if (!usuario.IsActivo)
            return BadRequest(new { mensaje = "El usuario ya se encuentra suspendido." });

        usuario.Suspender();

        await _usuarioRepository.GuardarCambiosAsync();

        return Ok(new
        {
            exito = true,
            mensaje = $"El usuario {usuario.Email} ha sido suspendido exitosamente."
        });
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync();

        // Filtramos los datos sensibles antes de enviarlos
        var resultado = usuarios.Select(u => new
        {
            u.UsuarioId,
            u.Nombre,
            u.Apellido,
            u.Email,
            u.Rol,
            u.IsActivo,
            FechaRegistro = u.CreatedAt
        });

        return Ok(resultado);
    }
}