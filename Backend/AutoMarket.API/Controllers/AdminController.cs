using AutoMarket.API.Constants;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IPlanCatalogoService _planCatalogoService;
    private readonly IUsuarioCuentaService _usuarioCuentaService;

    public AdminController(
        IDashboardService dashboardService,
        IUsuarioRepository usuarioRepository,
        IAnuncioRepository anuncioRepository,
        IAlmacenadorArchivos almacenadorArchivos,
        ISuscripcionService suscripcionService,
        IPlanCatalogoService planCatalogoService,
        IUsuarioCuentaService usuarioCuentaService)
    {
        _dashboardService = dashboardService;
        _usuarioRepository = usuarioRepository;
        _anuncioRepository = anuncioRepository;
        _almacenadorArchivos = almacenadorArchivos;
        _suscripcionService = suscripcionService;
        _planCatalogoService = planCatalogoService;
        _usuarioCuentaService = usuarioCuentaService;
    }

    [HttpGet("dashboard/resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var resumen = await _dashboardService.ObtenerResumenAsync();
        return Ok(resumen);
    }

    [HttpPatch("usuarios/{id:int}/suspender")]
    public async Task<IActionResult> SuspenderUsuario(int id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (usuario.Rol == "Admin")
            return BadRequest(new { mensaje = "No puedes suspender a otro administrador." });

        if (!usuario.IsActivo)
            return BadRequest(new { mensaje = "El usuario ya se encuentra suspendido." });

        usuario.Suspender();

        await _usuarioRepository.GuardarCambiosAsync();

        return Ok(new
        {
            exito = true,
            mensaje = $"El usuario {usuario.Email} ha sido suspendido exitosamente."
        });
    }

    [HttpPatch("usuarios/{id:int}/reactivar")]
    public async Task<IActionResult> ReactivarUsuario(int id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (usuario.Rol == "Admin")
            return BadRequest(new { mensaje = "No puedes modificar a otro administrador." });

        if (usuario.IsActivo)
            return BadRequest(new { mensaje = "El usuario ya se encuentra activo." });

        usuario.Reactivar();

        await _usuarioRepository.GuardarCambiosAsync();

        return Ok(new
        {
            exito = true,
            mensaje = $"El usuario {usuario.Email} ha sido reactivado exitosamente."
        });
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync();

        var resultado = usuarios.Select(u => new
        {
            u.UsuarioId,
            u.Nombre,
            u.Apellido,
            u.Email,
            u.Rol,
            u.IsActivo,
            FechaRegistro = u.CreatedAt
        });

        return Ok(resultado);
    }

    [HttpPut("usuarios/{id:int}/rol")]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolAdminDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (usuario.Rol == "Admin")
            return BadRequest(new { mensaje = "No puedes modificar el rol de otro administrador." });

        var cuenta = await _usuarioCuentaService.CambiarRolAdminAsync(id, dto);

        return Ok(new
        {
            exito = true,
            mensaje = $"El rol de {cuenta.Email} ahora es {cuenta.Rol}.",
            usuario = cuenta
        });
    }

    // ==========================================
    // 3. MODERACIÓN DE CATÁLOGO (ANUNCIOS)
    // ==========================================

    [HttpGet("anuncios")]
    public async Task<IActionResult> ListarAnuncios()
    {
        var anuncios = await _anuncioRepository.ObtenerTodosParaAdminAsync();

        var resultado = anuncios.Select(a => new
        {
            a.Id,
            a.Marca,
            a.Modelo,
            a.Precio,
            a.Moneda,
            a.UsuarioId
        });

        return Ok(resultado);
    }

    [HttpDelete("anuncios/{id:int}")]
    public async Task<IActionResult> EliminarAnuncioForzoso(int id)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(id);

        if (anuncio == null)
            return NotFound(new { mensaje = "Anuncio no encontrado." });

        if (anuncio.Fotos != null && anuncio.Fotos.Any())
        {
            foreach (var urlFoto in anuncio.Fotos)
            {
                // Eliminamos el objeto en S3 (clave u URL legada)
                await _almacenadorArchivos.EliminarArchivoAsync(urlFoto);
            }
        }

        _anuncioRepository.Eliminar(anuncio);
        await _anuncioRepository.GuardarCambiosAsync();

        return Ok(new { exito = true, mensaje = "Proceso terminado." });
    }

    // ==========================================
    // 4. MODERACIÓN DE SUSCRIPCIONES
    // ==========================================

    [HttpPut("suscripciones/{dealerId:int}/plan")]
    public async Task<IActionResult> CambiarPlanForzoso(int dealerId, [FromBody] CambiarPlanAdminDto dto)
    {
        await _suscripcionService.CambiarPlanAsync(dealerId, dto.NuevoNivel, CicloFacturacion.Mensual);
        return Ok(new { exito = true, mensaje = $"Plan del dealer {dealerId} actualizado a {dto.NuevoNivel}." });
    }

    [HttpPut("suscripciones/{dealerId:int}/renovar")]
    public async Task<IActionResult> RenovarSuscripcionManual(int dealerId, [FromBody] RenovarSuscripcionDto dto)
    {
        var fechaUtc = dto.NuevaFechaVencimiento.ToUniversalTime();

        await _suscripcionService.RenovarManualAsync(dealerId, fechaUtc);
        return Ok(new { exito = true, mensaje = $"Suscripción extendida y activada hasta {fechaUtc:dd/MM/yyyy}." });
    }

    // ==========================================
    // 4b. PAGOS Y REEMBOLSOS
    // ==========================================

    [HttpGet("pagos")]
    public async Task<IActionResult> ListarPagos()
    {
        var pagos = await _suscripcionService.ObtenerPagosAdminAsync();
        return Ok(pagos);
    }

    [HttpPost("pagos/{id:int}/reembolsar")]
    public async Task<IActionResult> ReembolsarPago(int id)
    {
        try
        {
            await _suscripcionService.ReembolsarPagoAsync(id);
            return Ok(new { exito = true, mensaje = $"El pago {id} fue reembolsado correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    // ==========================================
    // 5. CATÁLOGO DE PLANES
    // ==========================================

    [HttpGet("planes")]
    public async Task<IActionResult> ListarPlanes()
    {
        var planes = await _planCatalogoService.ObtenerCatalogoAdminAsync();
        return Ok(planes);
    }

    [HttpPost("planes")]
    public async Task<IActionResult> CrearPlan([FromBody] PlanCatalogoCreateDto dto)
    {
        var plan = await _planCatalogoService.CrearPlanAsync(dto);
        return CreatedAtAction(nameof(ListarPlanes), new { }, plan);
    }

    [HttpPut("planes/{id:int}")]
    public async Task<IActionResult> ActualizarPlan(int id, [FromBody] PlanCatalogoUpdateDto dto)
    {
        try
        {
            var plan = await _planCatalogoService.ActualizarPlanAsync(id, dto);
            return Ok(plan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpDelete("planes/{id:int}")]
    public async Task<IActionResult> EliminarPlan(int id)
    {
        try
        {
            await _planCatalogoService.EliminarPlanAsync(id);
            return Ok(new { exito = true, mensaje = "Plan desactivado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
    }
}