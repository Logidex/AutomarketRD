using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

/// <summary>
/// Endpoint para inicializar el token CSRF (patrón double-submit cookie).
/// Genera un token, lo guarda como cookie Y lo retorna en el body para que
/// el frontend lo almacene en memoria y lo envíe en X-CSRF-Token.
/// Esto permite que funcione cross-origin (document.cookie no puede leer
/// cookies de otro origen, pero sí podemos leer el body de la respuesta).
/// </summary>
[ApiController]
[Route("api/csrf")]
public class CsrfController : ControllerBase
{
    private const string COOKIE_NAME = "automarket_csrf";
    private const int TOKEN_LENGTH = 32;

    [HttpGet]
    public IActionResult ObtenerToken()
    {
        var token = RandomNumberGenerator.GetBytes(TOKEN_LENGTH);
        var tokenBase64 = Convert.ToBase64String(token)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var forwardedProto = HttpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
        var isSecure = HttpContext.Request.IsHttps
            || string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase);

        HttpContext.Response.Cookies.Append(COOKIE_NAME, tokenBase64, new CookieOptions
        {
            HttpOnly = false,
            Secure = isSecure,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromHours(2)
        });

        return Ok(new { token = tokenBase64 });
    }
}