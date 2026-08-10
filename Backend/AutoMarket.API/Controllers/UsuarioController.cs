using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioCuentaService _cuentaService;

    public UsuarioController(IUsuarioCuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> ObtenerCuenta()
    {
        var usuarioId = User.ObtenerUsuarioId();
        var cuenta = await _cuentaService.ObtenerCuentaAsync(usuarioId);
        return Ok(cuenta);
    }

    [HttpPut("me")]
    public async Task<IActionResult> ActualizarDatos([FromBody] ActualizarDatosDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioId = User.ObtenerUsuarioId();
        var cuenta = await _cuentaService.ActualizarDatosAsync(usuarioId, dto);
        return Ok(cuenta);
    }

    [HttpPost("cambiar-password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            await _cuentaService.CambiarPasswordAsync(usuarioId, dto);
            return Ok(new { exito = true, mensaje = "Contraseña actualizada correctamente." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("cambiar-email")]
    public async Task<IActionResult> SolicitarCambioEmail([FromBody] SolicitarCambioEmailDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            await _cuentaService.SolicitarCambioEmailAsync(usuarioId, dto);
            return Ok(new { exito = true, mensaje = "Te enviamos un código al nuevo correo. Revisa tu bandeja de entrada." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("confirmar-email")]
    public async Task<IActionResult> ConfirmarCambioEmail([FromBody] ConfirmarEmailDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            await _cuentaService.ConfirmarCambioEmailAsync(usuarioId, dto);
            return Ok(new { exito = true, mensaje = "Tu correo electrónico fue actualizado exitosamente." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}