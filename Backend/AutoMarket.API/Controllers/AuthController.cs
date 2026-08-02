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
            return BadRequest(resultado.Mensaje);
        }

        return Ok(resultado.Mensaje);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        if (!resultado.Exito)
        {
            return BadRequest(resultado.Mensaje);
        }

        return Ok(new { resultado.Mensaje, resultado.Token });
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException("Este es un error de prueba del middleware");
    }
}

