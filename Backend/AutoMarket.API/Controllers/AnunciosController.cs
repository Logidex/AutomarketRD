using Microsoft.AspNetCore.Mvc;
using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Anuncios.Commands;
using AutoMarket.Application.Features.Anuncios.Queries;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MediatR;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Controlador de Anuncios - Usa MediatR para CQRS.
/// Los endpoints de archivos (IFormFile) usan IAnuncioService directamente.
/// </summary>
[Authorize(Roles = Roles.DealerVendedor)]
public class AnunciosController : BaseApiController
{
    private readonly IAnuncioService _anuncioService;

    public AnunciosController(IMediator mediator, IAnuncioService anuncioService) : base(mediator)
    {
        _anuncioService = anuncioService;
    }

    // ==========================================
    // QUERIES (Lectura)
    // ==========================================

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var usuarioId = User.Identity?.IsAuthenticated == true
            ? User.ObtenerUsuarioId()
            : (int?)null;

        var resultado = await Mediator.Send(new ObtenerAnuncioPorIdQuery { Id = id, UsuarioId = usuarioId });
        return resultado is null
            ? NotFound(new { mensaje = $"Anuncio {id} no encontrado." })
            : Ok(resultado);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerTodos()
    {
        var resultado = await Mediator.Send(new ObtenerTodosLosAnunciosQuery());
        return Ok(resultado);
    }

    [HttpGet("buscar")]
    [AllowAnonymous]
    public async Task<IActionResult> Buscar([FromQuery] AnuncioSearchDto dto)
    {
        if (dto.UsuarioId.HasValue)
        {
            var usuarioSesion = User.Identity?.IsAuthenticated == true
                ? User.ObtenerUsuarioId()
                : (int?)null;

            if (usuarioSesion != dto.UsuarioId)
                dto.UsuarioId = null;
        }

        var resultado = await Mediator.Send(new BuscarAnunciosQuery { Dto = dto });
        return Ok(resultado);
    }

    [HttpGet("destacados")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerDestacados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
    {
        var resultado = await Mediator.Send(new ObtenerDestacadosQuery
        {
            Pagina = Math.Max(1, pagina),
            TamanoPagina = Math.Clamp(tamanoPagina, 1, 50)
        });
        return Ok(resultado);
    }

    // ==========================================
    // COMMANDS (Escritura)
    // ==========================================

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AnuncioCreateDto dto)
    {
        dto.UsuarioId = User.ObtenerUsuarioId();
        var nuevoId = await Mediator.Send(new CrearAnuncioCommand { Dto = dto });
        return Ok(new { mensaje = "Anuncio creado.", id = nuevoId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] AnuncioUpdateDto dto)
    {
        var resultado = await Mediator.Send(new ActualizarAnuncioCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId(),
            Dto = dto
        });
        return resultado is null
            ? NotFound(new { mensaje = "Anuncio no encontrado." })
            : Ok(resultado);
    }

    [HttpPatch("{id}/publicar")]
    public async Task<IActionResult> Publicar(int id)
    {
        var ok = await Mediator.Send(new PublicarAnuncioCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId()
        });
        return ok ? Ok(new { mensaje = "Publicado." }) : NotFound();
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] AnuncioEstadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Estado))
            return BadRequest(new { mensaje = "El estado es obligatorio." });

        var ok = await Mediator.Send(new CambiarEstadoAnuncioCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId(),
            Estado = dto.Estado
        });
        return ok ? Ok(new { mensaje = "Estado actualizado." }) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ok = await Mediator.Send(new EliminarAnuncioCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId()
        });
        return ok ? Ok(new { mensaje = "Eliminado." }) : NotFound();
    }

    [HttpPost("{id:int}/renovar-gratis")]
    public async Task<IActionResult> RenovarGratis(int id)
    {
        await Mediator.Send(new RenovarAnuncioGratisCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId()
        });
        return Ok(new { mensaje = "Renovado por 30 días." });
    }

    [HttpPost("{id:int}/registrar-vista")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistrarVista(int id)
    {
        try
        {
            await Mediator.Send(new RegistrarVistaCommand { AnuncioId = id });
            return Ok(new { mensaje = "Vista registrada." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Anuncio no encontrado." });
        }
    }

    // ==========================================
    // DESTACADOS
    // ==========================================

    [HttpPatch("{id:int}/destacar")]
    public async Task<IActionResult> Destacar(int id)
    {
        await Mediator.Send(new MarcarDestacadoCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId()
        });
        return Ok(new { mensaje = "Anuncio destacado." });
    }

    [HttpPatch("{id:int}/quitar-destacado")]
    public async Task<IActionResult> QuitarDestacado(int id)
    {
        await Mediator.Send(new QuitarDestacadoCommand
        {
            Id = id,
            UsuarioId = User.ObtenerUsuarioId()
        });
        return Ok(new { mensaje = "Destacado quitado." });
    }

    // ==========================================
    // ARCHIVOS (IFormFile - no MediatR)
    // ==========================================

    [HttpPost("{id}/imagenes")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirImagenes(int id, [FromForm] List<IFormFile> imagenes)
    {
        if (imagenes == null || !imagenes.Any())
            return BadRequest(new { error = "Debes seleccionar al menos una imagen." });

        if (imagenes.Count > 10)
            return BadRequest(new { error = "No puedes subir más de 10 imágenes." });

        int usuarioId = User.ObtenerUsuarioId();

        var dto = new AnuncioImagenUploadDto
        {
            AnuncioId = id,
            UsuarioId = usuarioId,
            Imagenes = imagenes
        };

        try
        {
            var rutasGuardadas = await _anuncioService.SubirImagenesAsync(dto);
            return Ok(new { mensaje = "Imágenes subidas.", imagenes = rutasGuardadas });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    [HttpPut("{id:int}/foto-principal")]
    public async Task<IActionResult> EstablecerFotoPrincipal(int id, [FromBody] AnuncioFotoPrincipalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UrlImagen))
            return BadRequest(new { error = "Debes indicar la imagen principal." });

        int usuarioId = User.ObtenerUsuarioId();

        try
        {
            var ok = await _anuncioService.EstablecerFotoPrincipalAsync(id, usuarioId, dto.UrlImagen);
            return ok ? Ok(new { mensaje = "Foto principal actualizada." }) : NotFound();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id}/imagenes")]
    public async Task<IActionResult> EliminarImagen(int id, [FromBody] EliminarImagenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UrlImagen))
            return BadRequest(new { error = "La URL de la imagen es obligatoria." });

        int usuarioId = User.ObtenerUsuarioId();

        try
        {
            await _anuncioService.EliminarImagenAsync(id, usuarioId, dto.UrlImagen);
            return Ok(new { mensaje = "Imagen eliminada." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }
}
