using AutoMarket.API.Extensions;
using AutoMarket.API.Helpers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.Auth.Commands;
using AutoMarket.Application.Features.Auth.Queries;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    public AuthController(IMediator mediator) : base(mediator) { }

    [HttpPost("registrar")]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaRegistro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        var resultado = await Mediator.Send(new RegistrarUsuarioCommand(dto));

        if (!resultado.Exito)
            return BadRequest(new { mensaje = resultado.Mensaje });

        return Ok(new { exito = true, mensaje = resultado.Mensaje });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await Mediator.Send(new LoginCommand(dto));

        if (!resultado.Exito)
            return BadRequest(new { mensaje = resultado.Mensaje });

        AuthCookieHelper.EstablecerTokenCookie(Response, resultado.Token!, Request);
        AuthCookieHelper.EstablecerRefreshCookie(Response, resultado.RefreshToken!, Request);

        return Ok(new
        {
            resultado.Exito,
            resultado.Mensaje,
            resultado.Usuario
        });
    }

    [HttpPost("refrescar")]
    [AllowAnonymous]
    public async Task<IActionResult> Refrescar()
    {
        var refreshToken = Request.Cookies[AuthCookieHelper.RefreshCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { mensaje = "Sesión inválida." });

        try
        {
            var resultado = await Mediator.Send(new RefrescarSesionCommand(refreshToken));

            AuthCookieHelper.EstablecerTokenCookie(Response, resultado.Token!, Request);
            AuthCookieHelper.EstablecerRefreshCookie(Response, resultado.RefreshToken!, Request);

            return Ok(new
            {
                resultado.Exito,
                resultado.Usuario
            });
        }
        catch (UnauthorizedAccessException)
        {
            AuthCookieHelper.LimpiarTokenCookie(Response, Request);
            return Unauthorized(new { mensaje = "Sesión inválida." });
        }
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await Mediator.Send(new LogoutCommand(
            Request.Cookies[AuthCookieHelper.RefreshCookieName]));

        AuthCookieHelper.LimpiarTokenCookie(Response, Request);
        return Ok(new { exito = true, mensaje = "Sesión cerrada." });
    }

    [HttpPost("recuperar-password")]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { mensaje = "El correo es obligatorio." });

        await Mediator.Send(new SolicitarRecuperacionCommand(dto.Email));

        return Ok(new { exito = true, mensaje = "Si el correo está registrado, recibirás un código para restablecer tu contraseña." });
    }

    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await Mediator.Send(new RestablecerPasswordCommand(dto));
            return Ok(new { exito = true, mensaje = "Tu contraseña fue restablecida. Ya puedes iniciar sesión." });
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

    [HttpPost("confirmar-correo")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarCorreo([FromBody] ConfirmarCorreoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await Mediator.Send(new ConfirmarCorreoCommand(dto.Token));
            return Ok(new { exito = true, mensaje = "Tu correo fue confirmado exitosamente." });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("reenviar-confirmacion")]
    [AllowAnonymous]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> ReenviarConfirmacion([FromBody] ReenviarConfirmacionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { mensaje = "El correo es obligatorio." });

        await Mediator.Send(new ReenviarConfirmacionCommand(dto.Email));

        return Ok(new { exito = true, mensaje = "Si el correo está registrado y sin confirmar, recibirás un nuevo enlace de confirmación." });
    }
}
