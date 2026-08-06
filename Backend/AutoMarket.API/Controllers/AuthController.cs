using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        Console.WriteLine(
            $"Response: Exito={resultado.Exito}, " +
            $"Mensaje={resultado.Mensaje}, " +
            $"Token={resultado.Token?.Substring(
                0,
                Math.Min(10, resultado.Token.Length)
            )}..."
        );

        return Ok(resultado);
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException("Este es un error de prueba del middleware");
    }
}

