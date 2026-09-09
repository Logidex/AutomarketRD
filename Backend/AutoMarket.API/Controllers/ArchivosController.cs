using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/archivos")]
[AllowAnonymous]
public class ArchivosController : ControllerBase
{
    private readonly IArchivoService _archivoService;

    public ArchivosController(IArchivoService archivoService)
    {
        _archivoService = archivoService;
    }

    [HttpGet("{**clave}")]
    public async Task<IActionResult> Obtener(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return BadRequest(new { mensaje = "Clave de archivo no válida." });

        var urlFirmada = await _archivoService.ObtenerUrlFirmadaSiExisteAsync(clave);

        if (urlFirmada is null)
            return NotFound(new { mensaje = "Archivo no encontrado." });

        return Redirect(urlFirmada);
    }
}
