using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        var resultado = await _authService.RegistrarUsuarioAsync(dto);

        if (!resultado.Exito)
        {
            return BadRequest(new { mensaje = resultado.Mensaje });
        }

        return Ok(new { exito = true, mensaje = resultado.Mensaje });
    }

    [HttpPost("login")]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        if (!resultado.Exito)
        {
            return BadRequest(new { mensaje = resultado.Mensaje });
        }

        return Ok(resultado);
    }

    // Envía un código de recuperación al correo del usuario.
    // Responde igual si el correo existe o no, para no revelar cuentas registradas.
    [HttpPost("recuperar-password")]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { mensaje = "El correo es obligatorio." });

        await _authService.SolicitarRecuperacionAsync(dto.Email);

        return Ok(new { exito = true, mensaje = "Si el correo está registrado, recibirás un código para restablecer tu contraseña." });
    }

    // Valida el código y aplica la nueva contraseña.
    [HttpPost("restablecer-password")]
    [EnableRateLimiting("PoliticaLogin")]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _authService.RestablecerPasswordAsync(dto);
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

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException("Este es un error de prueba del middleware");
    }
}

