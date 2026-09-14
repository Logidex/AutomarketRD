using AutoMarket.API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Endpoint para inicializar el token CSRF (patrón double-submit cookie).
/// El frontend lo invoca al arrancar para que el navegador tenga la cookie
/// antes de realizar mutaciones (login, registro, etc.).
/// </summary>
[ApiController]
[Route("api/csrf")]
public class CsrfController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerToken()
    {
        CsrfMiddleware.EstablecerCookieSiNecesaria(HttpContext);
        return Ok(new { mensaje = "Cookie CSRF lista." });
    }
}