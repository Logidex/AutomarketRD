using AutoMarket.API.Constants;
using AutoMarket.Application.DTOs.AdSlots;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdSlotsController : BaseApiController
{
    private readonly IAdSlotService _adSlotService;
    private readonly AdSlotImageService _imageService;

    public AdSlotsController(
        IMediator mediator,
        IAdSlotService adSlotService,
        AdSlotImageService imageService) : base(mediator)
    {
        _adSlotService = adSlotService;
        _imageService = imageService;
    }

    /// <summary>
    /// Obtiene los slots disponibles para una ubicación específica.
    /// Endpoint público.
    /// </summary>
    [HttpGet("publico")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerSlotsPublicos([FromQuery] UbicacionAdSlot ubicacion)
    {
        var slots = await _adSlotService.ObtenerSlotsPublicosPorUbicacionAsync(ubicacion);
        return Ok(slots);
    }

    /// <summary>
    /// Registra una impresión de un anuncio.
    /// </summary>
    [HttpPost("track/impresion/{anuncioId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistrarImpresion(int anuncioId)
    {
        await _adSlotService.RegistrarImpresionAsync(anuncioId);
        return Ok(new { exito = true });
    }

    /// <summary>
    /// Registra un click en un anuncio.
    /// </summary>
    [HttpPost("track/click/{anuncioId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistrarClick(int anuncioId)
    {
        await _adSlotService.RegistrarClickAsync(anuncioId);
        return Ok(new { exito = true });
    }

    /// <summary>
    /// Obtiene los slots disponibles para que un dealer compre un anuncio.
    /// </summary>
    [HttpGet("disponibles")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> ObtenerSlotsDisponibles()
    {
        var slots = await _adSlotService.ObtenerSlotsDisponiblesAsync();
        return Ok(slots);
    }

    /// <summary>
    /// Obtiene los anuncios del dealer autenticado.
    /// </summary>
    [HttpGet("mis-anuncios")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> ObtenerMisAnuncios()
    {
        var usuarioId = ObtenerUsuarioIdRequerido();
        var anuncios = await _adSlotService.ObtenerMisAnunciosAsync(usuarioId);
        return Ok(anuncios);
    }

    /// <summary>
    /// Crea un nuevo anuncio publicitario.
    /// </summary>
    [HttpPost("crear")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> CrearAnuncio([FromBody] CrearAdSlotAnuncioDto dto)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdRequerido();
            var anuncio = await _adSlotService.CrearAnuncioAsync(usuarioId, dto);
            return CreatedAtAction(nameof(ObtenerMisAnuncios), new { }, anuncio);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Sube y procesa una imagen para un anuncio publicitario.
    /// </summary>
    [HttpPost("subir-imagen")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> SubirImagen(
        [FromForm] IFormFile imagen,
        [FromQuery] UbicacionAdSlot ubicacion)
    {
        if (imagen == null || imagen.Length == 0)
            return BadRequest(new { exito = false, mensaje = "Debe seleccionar una imagen." });

        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
            return BadRequest(new { exito = false, mensaje = "Formato no permitido. Use JPG, PNG o WebP." });

        if (imagen.Length > 5 * 1024 * 1024)
            return BadRequest(new { exito = false, mensaje = "La imagen no puede exceder 5 MB." });

        var usuarioId = ObtenerUsuarioIdRequerido();

        using var stream = imagen.OpenReadStream();
        var (original, redimensionada) = await _imageService.ProcesarImagenAsync(
            stream,
            imagen.FileName,
            imagen.ContentType,
            usuarioId,
            ubicacion);

        return Ok(new
        {
            exito = true,
            imagenOriginal = original,
            imagenRedimensionada = redimensionada
        });
    }

    /// <summary>
    /// Cancela un anuncio publicitario del dealer.
    /// </summary>
    [HttpDelete("cancelar/{anuncioId:int}")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> CancelarAnuncio(int anuncioId)
    {
        try
        {
            var usuarioId = ObtenerUsuarioIdRequerido();
            await _adSlotService.CancelarAnuncioAsync(usuarioId, anuncioId);
            return Ok(new { exito = true, mensaje = "Anuncio cancelado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { exito = false, mensaje = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { exito = false, mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de los anuncios del dealer.
    /// </summary>
    [HttpGet("mis-estadisticas")]
    [Authorize(Roles = Roles.DealerVendedor)]
    public async Task<IActionResult> ObtenerEstadisticas()
    {
        var usuarioId = ObtenerUsuarioIdRequerido();
        var stats = await _adSlotService.ObtenerEstadisticasAsync(usuarioId);
        return Ok(stats);
    }
}
