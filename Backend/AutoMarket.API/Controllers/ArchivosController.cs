using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/archivos")]
/// <summary>
/// Controlador para gestionar Archivos.
/// </summary>
public class ArchivosController : ControllerBase
{
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IAnuncioRepository _anuncioRepository;

/// <summary>
/// Inicializa una nueva instancia de la clase ArchivosController. Parámetros almacenadorArchivos (IAlmacenadorArchivos), anuncioRepository (IAnuncioRepository)
/// </summary>
    public ArchivosController(
        IAlmacenadorArchivos almacenadorArchivos,
        IAnuncioRepository anuncioRepository)
    {
        _almacenadorArchivos = almacenadorArchivos;
        _anuncioRepository = anuncioRepository;
    }

    /// <summary>
    /// Sirve un archivo privado de S3 redirigiendo (302 Found) a una URL firmada de corta duración.
    /// Acepta tanto claves ("uploads/x.jpg") como URLs públicas legadas del formato anterior.
    /// Solo emite URLs para claves que pertenecen a la galería de fotos de algún anuncio;
    /// cualquier otra clave del bucket se rechaza con 404 (el endpoint no es un proxy abierto).
    /// </summary>
    [HttpGet("{**clave}")]
    public async Task<IActionResult> Obtener(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return BadRequest(new { mensaje = "Clave de archivo no válida." });

        if (!await _anuncioRepository.ExisteFotoAsync(clave))
            return NotFound(new { mensaje = "Archivo no encontrado." });

        var urlFirmada = await _almacenadorArchivos.GenerarUrlFirmadaAsync(clave);

        return Redirect(urlFirmada);
    }
}
