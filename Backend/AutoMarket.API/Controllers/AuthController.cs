using AutoMarket.API.Helpers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Auth;
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
     /// Inicia sesión de un usuario. El JWT se devuelve como cookie HttpOnly
     /// (no legible por JavaScript); el cuerpo solo incluye datos del usuario.
     /// Acceso anónimo permitido. Aplica rate limiting por IP.
     /// </summary>
     /// <param name="dto">Credenciales de inicio de sesión.</param>
     /// <returns>Datos del usuario autenticado (sin token en el cuerpo).</returns>
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

         AuthCookieHelper.EstablecerTokenCookie(
             Response,
             resultado.Token!,
             Request);

         return Ok(new
         {
             resultado.Exito,
             resultado.Mensaje,
             resultado.Usuario
         });
     }

     /// <summary>
     /// Cierra la sesión eliminando la cookie del token.
     /// Acceso anónimo permitido (solo invalida la cookie del navegador).
     /// </summary>
     /// <returns>Confirmación de cierre de sesión.</returns>
     [HttpPost("logout")]
     [AllowAnonymous]
     public IActionResult Logout()
     {
         AuthCookieHelper.LimpiarTokenCookie(Response, Request);
         return Ok(new { exito = true, mensaje = "Sesión cerrada." });
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
     /// Confirma el correo de una cuenta usando el token del enlace enviado al registrarse.
     /// Acceso anónimo permitido (el token es la credencial).
     /// </summary>
     /// <param name="dto">Datos que contienen el token de confirmación.</param>
     /// <returns>Resultado de la confirmación.</returns>
     [HttpPost("confirmar-correo")]
     [AllowAnonymous]
     public async Task<IActionResult> ConfirmarCorreo([FromBody] ConfirmarCorreoDto dto)
     {
         if (!ModelState.IsValid)
             return BadRequest(ModelState);

         try
         {
             await _authService.ConfirmarCorreoAsync(dto.Token);
             return Ok(new { exito = true, mensaje = "Tu correo fue confirmado exitosamente." });
         }
         catch (BusinessRuleException ex)
         {
             return BadRequest(new { mensaje = ex.Message });
         }
     }

     /// <summary>
     /// Reenvía el correo de confirmación a un dealer con correo sin confirmar.
     /// Responde igual si el correo existe o no, para no revelar cuentas registradas.
     /// Aplica rate limiting por IP.
     /// </summary>
     /// <param name="dto">Datos que contienen el correo electrónico.</param>
     /// <returns>Mensaje genérico de reenvío.</returns>
     [HttpPost("reenviar-confirmacion")]
     [AllowAnonymous]
     [EnableRateLimiting("PoliticaLogin")]
     public async Task<IActionResult> ReenviarConfirmacion([FromBody] ReenviarConfirmacionDto dto)
     {
         if (string.IsNullOrWhiteSpace(dto.Email))
             return BadRequest(new { mensaje = "El correo es obligatorio." });

         await _authService.ReenviarConfirmacionCorreoAsync(dto.Email);

         return Ok(new { exito = true, mensaje = "Si el correo está registrado y sin confirmar, recibirás un nuevo enlace de confirmación." });
     }
 }



