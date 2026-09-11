using AutoMarket.API.Extensions;
using AutoMarket.API.Helpers;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.Auth.Commands;
using AutoMarket.Application.Features.Auth.Queries;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsuarioController : BaseApiController
{
    public UsuarioController(IMediator mediator) : base(mediator) { }

    [HttpGet("me")]
    public async Task<IActionResult> ObtenerCuenta()
    {
        var usuarioId = ObtenerUsuarioIdRequerido();
        var cuenta = await Mediator.Send(new ObtenerCuentaQuery(usuarioId));
        return Ok(cuenta);
    }

    [HttpPut("me")]
    public async Task<IActionResult> ActualizarDatos([FromBody] ActualizarDatosDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuarioId = ObtenerUsuarioIdRequerido();
        var cuenta = await Mediator.Send(new ActualizarDatosCommand(usuarioId, dto));
        return Ok(cuenta);
    }

    [HttpPost("ascender-rol")]
    public async Task<IActionResult> AscenderRol([FromBody] AscenderRolDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = ObtenerUsuarioIdRequerido();
            var sesion = await Mediator.Send(new AscenderRolCommand(usuarioId, dto));

            AuthCookieHelper.EstablecerTokenCookie(Response, sesion.Token!, Request);

            return Ok(new
            {
                sesion.Exito,
                sesion.Mensaje,
                sesion.Usuario
            });
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
            var usuarioId = ObtenerUsuarioIdRequerido();
            await Mediator.Send(new SolicitarCambioPasswordCommand(usuarioId, dto));
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
            var usuarioId = ObtenerUsuarioIdRequerido();
            await Mediator.Send(new ConfirmarCambioPasswordCommand(usuarioId, dto));
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
            var usuarioId = ObtenerUsuarioIdRequerido();
            await Mediator.Send(new SolicitarCambioEmailCommand(usuarioId, dto));
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
            var usuarioId = ObtenerUsuarioIdRequerido();
            await Mediator.Send(new ConfirmarCambioEmailCommand(usuarioId, dto));
            return Ok(new { exito = true, mensaje = "Tu correo electrónico fue actualizado exitosamente." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
