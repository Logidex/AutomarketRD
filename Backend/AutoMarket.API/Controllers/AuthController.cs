using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

/// <summary>
/// Controlador para manejar la autenticación de usuarios: registro, inicio de sesión,
/// recuperación y restablecimiento de contraseña.
/// </summary>
[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador para gestionar Auth.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

/// <summary>
/// Inicializa una nueva instancia de la clase AuthController. Parámetro authService (IAuthService)
/// </summary>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

     /// <summary>
     /// Registra un nuevo usuario en el sistema.
     /// Acceso anónimo permitido.
     /// </summary>
     /// <param name="dto">Datos de registro del usuario.</param>
     /// <returns>Resultado del registro.</returns>
     [HttpPost("registrar")]
     [AllowAnonymous]
     public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
     {
         var resultado = await _authService.RegistrarUsuarioAsync(dto);

         if (!resultado.Exito)
         {
             return BadRequest(new { mensaje = resultado.Mensaje });
         }

         return Ok(new { exito = true, mensaje = resultado.Mensaje });
     }

     /// <summary>
     /// Inicia sesión de un usuario y devuelve un token JWT.
     /// Acceso anónimo permitido. Aplica rate limiting por IP (o intenta por email si se implementa).
     /// </summary>
     /// <param name="dto">Credenciales de inicio de sesión.</param>
     /// <returns>Resultado del login incluyendo token y datos de usuario.</returns>
     [HttpPost("login")]
     [AllowAnonymous]
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

     /// <summary>
     /// Envía un código de recuperación al correo del usuario.
     /// Responde igual si el correo existe o no, para no revelar cuentas registradas.
     /// Aplica rate limiting por IP (o intenta por email si se implementa).
     /// </summary>
     /// <param name="dto">Datos que contienen el correo electrónico.</param>
     /// <returns>Mensaje indicando que si el correo está registrado se enviará un código.</returns>
     [HttpPost("recuperar-password")]
     [AllowAnonymous]
     [EnableRateLimiting("PoliticaLogin")]
     public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
     {
         if (string.IsNullOrWhiteSpace(dto.Email))
             return BadRequest(new { mensaje = "El correo es obligatorio." });

         await _authService.SolicitarRecuperacionAsync(dto.Email);

         return Ok(new { exito = true, mensaje = "Si el correo está registrado, recibirás un código para restablecer tu contraseña." });
     }

     /// <summary>
     /// Valida el código de recuperación y aplica la nueva contraseña.
     /// Acceso anónimo permitido. Aplica rate limiting por IP (o intenta por email si se implementa).
     /// </summary>
     /// <param name="dto">Datos para restablecer la contraseña (código y nueva contraseña).</param>
     /// <returns>Resultado del restablecimiento de contraseña.</returns>
     [HttpPost("restablecer-password")]
     [AllowAnonymous]
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

     /// <summary>
     /// Endpoint de prueba que lanza una excepción para verificar el middleware de manejo de errores.
     /// </summary>
     /// <returns>No devuelve nada; lanza una excepción intencionalmente.</returns>
     [HttpGet("test-error")]
/// <summary>
/// TestError Test error.. Retorna: IActionResult.
/// </summary>
     public IActionResult TestError()
     {
         throw new InvalidOperationException("Este es un error de prueba del middleware");
     }
}


