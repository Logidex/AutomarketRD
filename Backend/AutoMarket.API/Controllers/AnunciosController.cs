using Microsoft.AspNetCore.Mvc;
using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Services;
using Microsoft.AspNetCore.Authorization;
using AutoMarket.Application.Interfaces;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnunciosController : ControllerBase
{
    private readonly IAnuncioService _anuncioService;

    public AnunciosController(IAnuncioService anuncioService)
    {
        _anuncioService = anuncioService;
    }

    // ==========================================
    // 1. CREAR: Necesitamos saber quién lo crea
    // ==========================================
    [HttpPost]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> CrearAnuncio([FromBody] AnuncioCreateDto dto)
    {
        dto.UsuarioId = User.ObtenerUsuarioId();

        // Capturamos el ID recién creado
        int nuevoId = await _anuncioService.CrearAnuncioAsync(dto);

        // Devolvemos el ID al frontend junto con el mensaje
        return Ok(new { mensaje = "Anuncio creado correctamente.", id = nuevoId });
    }

    // ==========================================
    // 2. OBTENER: Dejamos esto público (sin Authorize) 
    // para que cualquier visitante vea la vitrina
    // ==========================================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var usuarioId = ObtenerUsuarioIdSiAutenticado();

        var anuncioDto = await _anuncioService.ObtenerAnuncioPorIdAsync(id, usuarioId);

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
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> ActualizarAnuncio(int id, [FromBody] AnuncioUpdateDto updateDto)
    {
        int usuarioId = User.ObtenerUsuarioId();

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
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> Publicar(int id)
    {
        int usuarioId = User.ObtenerUsuarioId();

        // El servicio debe verificar que este usuarioId es el dueño del anuncio 'id'
        var publicado = await _anuncioService.PublicarAnuncioAsync(id, usuarioId);

        if (!publicado) return NotFound(new { mensaje = "No se encontró el anuncio o no tienes permisos." });

        return Ok(new { mensaje = "Anuncio publicado con éxito." });
    }

    // ==========================================
    // 5. SUBIR IMÁGENES: Validación estricta
    // ==========================================
    [HttpPost("{id}/imagenes")]
    [Authorize(Roles = Roles.DealerVendedor)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirImagenes(int id, [FromForm] List<IFormFile> imagenes)
    {
        if (imagenes == null || !imagenes.Any())
            return BadRequest(new { error = "Debes seleccionar al menos una imagen." });

        if (imagenes.Count > 10)
            return BadRequest(new { error = "No puedes subir más de 10 imágenes en una sola petición." });

        int usuarioId = User.ObtenerUsuarioId();

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

    private int? ObtenerUsuarioIdSiAutenticado()
    {
        return User.Identity?.IsAuthenticated == true
            ? User.ObtenerUsuarioId()
            : null;
    }

    // =========================================================================
    // GET: api/anuncios/buscar
    // =========================================================================
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarAnuncios([FromQuery] AnuncioSearchDto dto)
    {
        // El filtro por UsuarioId busca anuncios privados (incluidos borradores),
        // así que solo se permite si el usuario autenticado es el dueño de ese inventario.
        if (dto.UsuarioId.HasValue)
        {
            var usuarioSesion = ObtenerUsuarioIdSiAutenticado();

            if (usuarioSesion != dto.UsuarioId)
                dto.UsuarioId = null;
        }

        // El servicio procesa los filtros y nos devuelve el resultado paginado
        var resultado = await _anuncioService.BuscarAnunciosAsync(dto);

        // Devolvemos el HTTP 200 OK junto con el JSON estructurado
        return Ok(resultado);
    }

    [HttpPatch("{id}/estado")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] AnuncioEstadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Estado))
            return BadRequest(new { mensaje = "El estado es obligatorio." });

        int usuarioId = User.ObtenerUsuarioId();

        var cambiado = await _anuncioService.CambiarEstadoAsync(id, usuarioId, dto.Estado);

        if (!cambiado)
            return NotFound(new { mensaje = "No se encontró el anuncio o no tienes permisos." });

        return Ok(new { mensaje = "Estado actualizado correctamente." });
    }

    // ==========================================
    // 6. ELIMINAR IMAGEN: Seguridad y limpieza
    // ==========================================
    [HttpDelete("{id}/imagenes")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> EliminarImagen(int id, [FromBody] EliminarImagenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UrlImagen))
            return BadRequest(new { error = "La URL de la imagen es obligatoria." });

        int usuarioId = User.ObtenerUsuarioId();

        try
        {
            await _anuncioService.EliminarImagenAsync(id, usuarioId, dto.UrlImagen);
            return Ok(new { mensaje = "Imagen eliminada de la base de datos y de S3 correctamente." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:int}/registrar-vista")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistrarVista(int id)
    {
        try
        {
            await _anuncioService.RegistrarVistaAsync(id);
            return Ok(new { mensaje = "Vista registrada." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Anuncio no encontrado." });
        }
    }

    // ==========================================
    // 7. ELIMINAR ANUNCIO COMPLETO
    // ==========================================
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> EliminarAnuncio(int id)
    {
        int usuarioId = User.ObtenerUsuarioId();

        try
        {
            var eliminado = await _anuncioService.EliminarAnuncioAsync(id, usuarioId);

            if (!eliminado)
                return NotFound(new { mensaje = "No se encontró el anuncio o no tienes permisos." });

            return Ok(new { mensaje = "Anuncio eliminado correctamente." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }
}