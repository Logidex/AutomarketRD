using Microsoft.AspNetCore.Mvc;
using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Services;
using Microsoft.AspNetCore.Authorization;
using AutoMarket.Application.Interfaces;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Controlador para gestionar las operaciones relacionadas con los anuncios de vehículos.
/// Incluye creación, lectura, actualización, eliminación y otras acciones como subir imágenes,
/// publicar, cambiar estado, etc.
/// </summary>
[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador para gestionar Anuncios.
/// </summary>
public class AnunciosController : ControllerBase
{
    private readonly IAnuncioService _anuncioService;

/// <summary>
/// Inicializa una nueva instancia de la clase AnunciosController. Parámetro anuncioService (IAnuncioService)
/// </summary>
    public AnunciosController(IAnuncioService anuncioService)
    {
        _anuncioService = anuncioService;
    }

     /// <summary>
     /// Crea un nuevo anuncio para el usuario autenticado.
     /// Requiere rol DealerVendedor.
     /// </summary>
     /// <param name="dto">Datos para crear el anuncio.</param>
     /// <returns>Resultado de la creación con mensaje y ID del anuncio creado.</returns>
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

     /// <summary>
     /// Obtiene un anuncio por su ID.
     /// Acceso público para anuncios publicados; para otros estados, solo el propietario puede acceder.
     /// </summary>
     /// <param name="id">ID del anuncio.</param>
     /// <returns>Datos del anuncio si se encuentra y el usuario tiene permiso; de lo contrario, NotFound.</returns>
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

     /// <summary>
     /// Obtiene todos los anuncios publicados (vitrina pública).
     /// </summary>
     /// <returns>Lista de anuncios publicados.</returns>
     [HttpGet]
     public async Task<IActionResult> ObtenerTodosLosAnuncios()
     {
         var anuncios = await _anuncioService.ObtenerTodosLosAnuncios();
         return Ok(anuncios);
     }

     /// <summary>
     /// Actualiza un anuncio existente.
     /// Solo el propietario del anuncio puede realizar esta acción.
     /// Requiere rol DealerVendedor.
     /// </summary>
     /// <param name="id">ID del anuncio a actualizar.</param>
     /// <param name="updateDto">Datos actualizados del anuncio.</param>
     /// <returns>Anuncio actualizado si se encuentra y el usuario tiene permiso; de lo contrario, NotFound.</returns>
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

     /// <summary>
     /// Publica un anuncio (cambia su estado a Publicado).
     /// Verifica que el usuario sea el propietario y que cumpla con los requisitos de su plan.
     /// Requiere rol DealerVendedor.
     /// </summary>
     /// <param name="id">ID del anuncio a publicar.</param>
     /// <returns>Resultado de la operación de publicación.</returns>
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

     /// <summary>
     /// Sube imágenes para un anuncio específico.
     /// Valida que el usuario sea el propietario y que las imágenes cumplan con los requisitos (máximo 10 imágenes, cada una ≤5 MB, formato PNG o JPEG).
     /// Requiere rol DealerVendedor.
     /// </summary>
     /// <param name="id">ID del anuncio al que se subirán las imágenes.</param>
     /// <param name="imagenes">Lista de archivos de imagen a subir.</param>
     /// <returns>Resultado de la subida con las URLs de las imágenes guardadas.</returns>
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
            var rutasGuardadas = await _anuncioService.SubirImagenesAsync(dto);
            return Ok(new { mensaje = "Imágenes subidas correctamente.", imagenes = rutasGuardadas });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); } // 403 Forbidden si no es el dueño
    }

    // =========================================================================
    // PUT: api/anuncios/{id}/foto-principal
    // Establece cuál foto es la portada del anuncio moviéndola al inicio.
    // =========================================================================
    [HttpPut("{id:int}/foto-principal")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> EstablecerFotoPrincipal(int id, [FromBody] AnuncioFotoPrincipalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UrlImagen))
            return BadRequest(new { error = "Debes indicar la imagen que será la principal." });

        int usuarioId = User.ObtenerUsuarioId();

        try
        {
            var ok = await _anuncioService.EstablecerFotoPrincipalAsync(id, usuarioId, dto.UrlImagen);
            if (!ok) return NotFound(new { mensaje = $"El vehículo con ID {id} no fue encontrado." });
            return Ok(new { mensaje = "Foto principal actualizada correctamente." });
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
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
