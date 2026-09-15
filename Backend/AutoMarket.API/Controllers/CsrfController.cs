using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Endpoint para inicializar el token CSRF (patrón double-submit cookie).
/// El frontend lo invoca al arrancar para que el navegador tenga la cookie
/// antes de realizar mutaciones (login, registro, etc.).
/// La cookie la establece automáticamente el CsrfMiddleware en cada petición
/// si no existe; este endpoint simplemente confirma que existe.
/// </summary>
[ApiController]
[Route("api/csrf")]
public class CsrfController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerToken()
    {
        return Ok(new { mensaje = "Cookie CSRF lista." });
    }
}