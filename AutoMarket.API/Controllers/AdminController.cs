using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
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

    public AdminController(
        IDashboardService dashboardService,
        IUsuarioRepository usuarioRepository,
        IAnuncioRepository anuncioRepository,
        IAlmacenadorArchivos almacenadorArchivos)
    {
        _dashboardService = dashboardService;
        _usuarioRepository = usuarioRepository;
        _anuncioRepository = anuncioRepository;
        _almacenadorArchivos = almacenadorArchivos;
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

        // 👇 TRAMPA DE DEBUG 1: Ver cuántas fotos está leyendo EF Core
        Console.WriteLine($"\n[DEBUG S3] -> El anuncio {id} tiene {anuncio.Fotos.Count} fotos registradas.");

        if (anuncio.Fotos != null && anuncio.Fotos.Any())
        {
            foreach (var urlFoto in anuncio.Fotos)
            {
                // 1. Extraemos solo el nombre del archivo (todo lo que está después del último '/')
                var nombreArchivo = urlFoto.Split('/').Last();

                // 2. Le enviamos solo el nombre a AWS S3
                await _almacenadorArchivos.EliminarArchivoAsync(nombreArchivo);
            }
        }

        _anuncioRepository.Eliminar(anuncio);
        await _anuncioRepository.GuardarCambiosAsync();

        return Ok(new { exito = true, mensaje = "Proceso terminado." });
    }
}