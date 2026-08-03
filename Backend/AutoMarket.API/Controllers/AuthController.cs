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

        var response = new
        {
            Exito = true,
            Mensaje = resultado.Mensaje,
            Token = resultado.Token
        };

        Console.WriteLine($"Response: Exito={response.Exito}, Mensaje={response.Mensaje}, Token={response.Token?.Substring(0, 10)}...");

        return Ok(response);
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new InvalidOperationException("Este es un error de prueba del middleware");
    }
}

