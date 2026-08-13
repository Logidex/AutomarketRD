using AutoMarket.Application.Services;
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

/// <summary>
/// Inicializa una nueva instancia de la clase ArchivosController. Parámetro almacenadorArchivos (IAlmacenadorArchivos)
/// </summary>
    public ArchivosController(IAlmacenadorArchivos almacenadorArchivos)
    {
        _almacenadorArchivos = almacenadorArchivos;
    }

    /// <summary>
    /// Sirve un archivo privado de S3 redirigiendo (302 Found) a una URL firmada de corta duración.
    /// Acepta tanto claves ("uploads/x.jpg") como URLs públicas legadas del formato anterior.
    /// </summary>
    [HttpGet("{**clave}")]
    public async Task<IActionResult> Obtener(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return BadRequest(new { mensaje = "Clave de archivo no válida." });

        var urlFirmada = await _almacenadorArchivos.GenerarUrlFirmadaAsync(clave);

        return Redirect(urlFirmada);
    }
}

