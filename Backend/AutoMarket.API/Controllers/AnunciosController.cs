using Microsoft.AspNetCore.Mvc;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnunciosController : ControllerBase
{
    private readonly AnuncioService _anuncioService;

    public AnunciosController(AnuncioService anuncioService)
    {
        _anuncioService = anuncioService;
    }

    // ==========================================
    // 1. CREAR: Necesitamos saber quién lo crea
    // ==========================================
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrearAnuncio([FromBody] AnuncioCreateDto dto)
    {
        int usuarioId = ObtenerUsuarioIdDelToken();

        // Asignamos el ID del creador al DTO antes de enviarlo al servicio
        dto.UsuarioId = usuarioId;

        await _anuncioService.CrearAnuncioAsync(dto);
        return Ok(new { mensaje = "Anuncio creado correctamente." });
    }

    // ==========================================
    // 2. OBTENER: Dejamos esto público (sin Authorize) 
    // para que cualquier visitante vea la vitrina
    // ==========================================
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var anuncioDto = await _anuncioService.ObtenerAnuncioPorIdAsync(id);

        if (anuncioDto == null)
            return NotFound(new { mensaje = $"El vehículo con ID {id} no fue encontrado." });

        return Ok(anuncioDto);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodosLosAnuncios()
    {
        var anuncios = await _anuncioService.ObtenerTodosLosAnuncios();
        return Ok(anuncios);
    }

    // ==========================================
    // 3. ACTUALIZAR: Protegido y validando propiedad
    // ==========================================
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> ActualizarAnuncio(int id, [FromBody] AnuncioUpdateDto updateDto)
    {
        int usuarioId = ObtenerUsuarioIdDelToken();

        // Le pasamos al servicio: "El usuario X quiere actualizar el anuncio Y"
        var resultado = await _anuncioService.ActualizarAsync(id, usuarioId, updateDto);

        if (resultado == null)
            return NotFound(new { mensaje = "El vehículo no existe o no tienes permisos para editarlo." });

        return Ok(resultado);
    }

    // ==========================================
    // 4. PUBLICAR: Añadimos Authorize
    // ==========================================
    [HttpPatch("{id}/publicar")]
    [Authorize]
    public async Task<IActionResult> Publicar(int id)
    {
        int usuarioId = ObtenerUsuarioIdDelToken();

        // El servicio debe verificar que este usuarioId es el dueño del anuncio 'id'
        var publicado = await _anuncioService.PublicarAnuncioAsync(id, usuarioId);

        if (!publicado) return NotFound(new { mensaje = "No se encontró el anuncio o no tienes permisos." });

        return Ok(new { mensaje = "Anuncio publicado con éxito." });
    }

    // ==========================================
    // 5. SUBIR IMÁGENES: Validación estricta
    // ==========================================
    [HttpPost("{id}/imagenes")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirImagenes(int id, [FromForm] List<IFormFile> imagenes)
    {
        if (imagenes == null || !imagenes.Any())
            return BadRequest(new { error = "Debes seleccionar al menos una imagen." });

        if (imagenes.Count > 10)
            return BadRequest(new { error = "No puedes subir más de 10 imágenes en una sola petición." });

        int usuarioId = ObtenerUsuarioIdDelToken();

        var dto = new AnuncioImagenUploadDto
        {
            AnuncioId = id,
            UsuarioId = usuarioId, // Pasamos el ID para verificar propiedad
            Imagenes = imagenes
        };

        try
        {
            await _anuncioService.SubirImagenesAsync(dto);
            return Ok(new { mensaje = "Imágenes subidas correctamente." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); } // 403 Forbidden si no es el dueño
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ==========================================
    // MÉTODO AUXILIAR PRIVADO
    // ==========================================
    private int ObtenerUsuarioIdDelToken()
    {
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
        {
            // Si llega aquí, significa que el token es inválido o no tiene el claim.
            // Aunque [Authorize] debería detenerlo antes, es una buena práctica de seguridad.
            throw new UnauthorizedAccessException("Token inválido o usuario no identificado.");
        }

        return usuarioId;
    }

    // =========================================================================
    // GET: api/anuncios/buscar
    // =========================================================================
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarAnuncios([FromQuery] AnuncioSearchDto dto)
    {
        // El servicio procesa los filtros y nos devuelve el resultado paginado
        var resultado = await _anuncioService.BuscarAnunciosAsync(dto);

        // Devolvemos el HTTP 200 OK junto con el JSON estructurado
        return Ok(resultado);
    }
}