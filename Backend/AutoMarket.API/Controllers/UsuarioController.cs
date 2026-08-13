using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Controlador para gestionar las operaciones relacionadas con la cuenta de usuario.
/// Permite obtener y actualizar datos del perfil, cambiar contraseña, email, ascender rol, etc.
/// Requiere autenticación.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
/// <summary>
/// Controlador para gestionar Usuario.
/// </summary>
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioCuentaService _cuentaService;

/// <summary>
/// Inicializa una nueva instancia de la clase UsuarioController. Parámetro cuentaService (IUsuarioCuentaService)
/// </summary>
    public UsuarioController(IUsuarioCuentaService cuentaService)
    {
        _cuentaService = cuentaService;
    }

     /// <summary>
     /// Obtiene los datos de la cuenta del usuario autenticado.
     /// </summary>
     /// <returns>Datos de la cuenta del usuario.</returns>
     [HttpGet("me")]
     public async Task<IActionResult> ObtenerCuenta()
     {
         var usuarioId = User.ObtenerUsuarioId();
         var cuenta = await _cuentaService.ObtenerCuentaAsync(usuarioId);
         return Ok(cuenta);
     }

     /// <summary>
     /// Actualiza los datos del perfil del usuario autenticado.
     /// </summary>
     /// <param name="dto">Datos a actualizar.</param>
     /// <returns>Datos actualizados de la cuenta.</returns>
     [HttpPut("me")]
     public async Task<IActionResult> ActualizarDatos([FromBody] ActualizarDatosDto dto)
     {
         if (!ModelState.IsValid)
             return BadRequest(ModelState);

         var usuarioId = User.ObtenerUsuarioId();
         var cuenta = await _cuentaService.ActualizarDatosAsync(usuarioId, dto);
         return Ok(cuenta);
     }

     /// <summary>
     /// Asciende el rol de la cuenta (Comprador → Vendedor/Dealer, Vendedor → Dealer).
     /// Devuelve la sesión actualizada (nuevo token con el nuevo rol).
     /// </summary>
     /// <param name="dto">Datos para ascender el rol.</param>
     /// <returns>Sesión actualizada con nuevo token.</returns>
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

     /// <summary>
     /// Solicita un cambio de contraseña para el usuario autenticado.
     /// Envía un código de verificación al correo electrónico asociado.
     /// </summary>
     /// <param name="dto">Datos necesarios para solicitar el cambio de contraseña.</param>
     /// <returns>Resultado de la solicitud.</returns>
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

     /// <summary>
     /// Confirma el cambio de contraseña usando el código enviado al correo.
     /// </summary>
     /// <param name="dto">Datos de confirmación (código y nueva contraseña).</param>
     /// <returns>Resultado de la confirmación.</returns>
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

     /// <summary>
     /// Solicita un cambio de correo electrónico para el usuario autenticado.
     /// Envía un código de verificación al nuevo correo electrónico.
     /// </summary>
     /// <param name="dto">Datos necesarios para solicitar el cambio de email.</param>
     /// <returns>Resultado de la solicitud.</returns>
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

     /// <summary>
     /// Confirma el cambio de correo electrónico usando el código enviado al nuevo correo.
     /// </summary>
     /// <param name="dto">Datos de confirmación (código y nuevo correo).</param>
     /// <returns>Resultado de la confirmación.</returns>
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
