using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly ISuscripcionService _suscripcionService;
    private readonly IPlanCatalogoService _planCatalogoService;

    public AdminController(
        IDashboardService dashboardService,
        IUsuarioRepository usuarioRepository,
        IAnuncioRepository anuncioRepository,
        IAlmacenadorArchivos almacenadorArchivos,
        ISuscripcionService suscripcionService,
        IPlanCatalogoService planCatalogoService)
    {
        _dashboardService = dashboardService;
        _usuarioRepository = usuarioRepository;
        _anuncioRepository = anuncioRepository;
        _almacenadorArchivos = almacenadorArchivos;
        _suscripcionService = suscripcionService;
        _planCatalogoService = planCatalogoService;
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
                // Eliminamos el objeto en S3 usando la URL pública completa
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
        try
        {
            await _suscripcionService.CambiarPlanAsync(dealerId, dto.NuevoNivel, CicloFacturacion.Mensual);
            return Ok(new { exito = true, mensaje = $"Plan del dealer {dealerId} actualizado a {dto.NuevoNivel}." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    [HttpPut("suscripciones/{dealerId:int}/renovar")]
    public async Task<IActionResult> RenovarSuscripcionManual(int dealerId, [FromBody] RenovarSuscripcionDto dto)
    {
        try
        {
            var fechaUtc = dto.NuevaFechaVencimiento.ToUniversalTime();

            await _suscripcionService.RenovarManualAsync(dealerId, fechaUtc);
            return Ok(new { exito = true, mensaje = $"Suscripción extendida y activada hasta {fechaUtc:dd/MM/yyyy}." });
        }
        catch (Exception ex)
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
        try
        {
            var plan = await _planCatalogoService.CrearPlanAsync(dto);
            return CreatedAtAction(nameof(ListarPlanes), new { }, plan);
        }
        catch (Exception ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
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
        catch (Exception ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
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
        catch (Exception ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }
}