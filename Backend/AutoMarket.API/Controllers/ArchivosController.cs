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
    private readonly IUsuarioRepository _usuarioRepository;

/// <summary>
/// Inicializa una nueva instancia de la clase ArchivosController. Parámetros almacenadorArchivos (IAlmacenadorArchivos), anuncioRepository (IAnuncioRepository), usuarioRepository (IUsuarioRepository)
/// </summary>
    public ArchivosController(
        IAlmacenadorArchivos almacenadorArchivos,
        IAnuncioRepository anuncioRepository,
        IUsuarioRepository usuarioRepository)
    {
        _almacenadorArchivos = almacenadorArchivos;
        _anuncioRepository = anuncioRepository;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Sirve un archivo privado de S3 redirigiendo (302 Found) a una URL firmada de corta duración.
    /// Acepta tanto claves ("uploads/x.jpg") como URLs públicas legadas del formato anterior.
    /// Solo emite URLs para claves registradas: fotos de anuncios o logos de
    /// dealers; cualquier otra clave del bucket se rechaza con 404.
    /// </summary>
    [HttpGet("{**clave}")]
    public async Task<IActionResult> Obtener(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return BadRequest(new { mensaje = "Clave de archivo no válida." });

        var esFotoDeAnuncio = await _anuncioRepository.ExisteFotoAsync(clave);
        var esLogoDeDealer = !esFotoDeAnuncio &&
                             await _usuarioRepository.ExisteLogoDealerAsync(clave);

        if (!esFotoDeAnuncio && !esLogoDeDealer)
            return NotFound(new { mensaje = "Archivo no encontrado." });

        var urlFirmada = await _almacenadorArchivos.GenerarUrlFirmadaAsync(clave);

        return Redirect(urlFirmada);
    }
}
