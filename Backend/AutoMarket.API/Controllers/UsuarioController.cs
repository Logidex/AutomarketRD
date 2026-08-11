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

    // Asciende el rol de la cuenta (Comprador → Vendedor/Dealer, Vendedor → Dealer).
    // Devuelve la sesión actualizada (nuevo token con el nuevo rol).
    [HttpPost("ascender-rol")]
    public async Task<IActionResult> AscenderRol([FromBody] AscenderRolDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            var sesion = await _cuentaService.AscenderRolAsync(usuarioId, dto);
            return Ok(sesion);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("cambiar-password")]
    public async Task<IActionResult> SolicitarCambioPassword([FromBody] SolicitarCambioPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            await _cuentaService.SolicitarCambioPasswordAsync(usuarioId, dto);
            return Ok(new { exito = true, mensaje = "Te enviamos un código a tu correo. Revisa tu bandeja de entrada." });
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

    [HttpPost("confirmar-password")]
    public async Task<IActionResult> ConfirmarCambioPassword([FromBody] ConfirmarPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = User.ObtenerUsuarioId();
            await _cuentaService.ConfirmarCambioPasswordAsync(usuarioId, dto);
            return Ok(new { exito = true, mensaje = "Tu contraseña fue actualizada correctamente." });
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