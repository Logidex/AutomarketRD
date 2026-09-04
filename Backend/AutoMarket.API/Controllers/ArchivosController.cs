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
    private readonly ISuscripcionRepository _suscripcionRepository;

/// <summary>
/// Inicializa una nueva instancia de la clase ArchivosController. Parámetros almacenadorArchivos (IAlmacenadorArchivos), anuncioRepository (IAnuncioRepository), usuarioRepository (IUsuarioRepository)
/// </summary>
    public ArchivosController(
        IAlmacenadorArchivos almacenadorArchivos,
        IAnuncioRepository anuncioRepository,
        IUsuarioRepository usuarioRepository,
        ISuscripcionRepository suscripcionRepository)
    {
        _almacenadorArchivos = almacenadorArchivos;
        _anuncioRepository = anuncioRepository;
        _usuarioRepository = usuarioRepository;
        _suscripcionRepository = suscripcionRepository;
    }

    /// <summary>
    /// Sirve un archivo privado de S3 redirigiendo (302 Found) a una URL firmada de corta duración.
    /// Acepta tanto claves ("uploads/x.jpg") como URLs públicas legadas del formato anterior.
    /// Solo emite URLs para claves registradas: fotos de anuncios, logos de
    /// dealers o capturas de transferencia; cualquier otra clave se rechaza con 404.
    /// </summary>
    [HttpGet("{**clave}")]
    public async Task<IActionResult> Obtener(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return BadRequest(new { mensaje = "Clave de archivo no válida." });

        var esFotoDeAnuncio = await _anuncioRepository.ExisteFotoAsync(clave);
        var esLogoDeDealer = !esFotoDeAnuncio &&
                             await _usuarioRepository.ExisteLogoDealerAsync(clave);
        var esCapturaTransferencia = !esFotoDeAnuncio && !esLogoDeDealer &&
                                     await _suscripcionRepository.ExisteCapturaTransferenciaAsync(clave);

        if (!esFotoDeAnuncio && !esLogoDeDealer && !esCapturaTransferencia)
            return NotFound(new { mensaje = "Archivo no encontrado." });

        var urlFirmada = await _almacenadorArchivos.GenerarUrlFirmadaAsync(clave);

        return Redirect(urlFirmada);
    }
}
