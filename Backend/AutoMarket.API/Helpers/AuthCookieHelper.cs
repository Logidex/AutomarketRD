using Microsoft.AspNetCore.Http;

namespace AutoMarket.API.Helpers;

/// <summary>
/// Maneja el JWT como cookie HttpOnly (SameSite=Lax sobre HTTP, None+Secure sobre HTTPS)
/// en lugar de localStorage. El token viaja únicamente en la cookie; nunca se expone al JavaScript.
/// </summary>
public static class AuthCookieHelper
{
    public const string CookieName = "automarket_token";

    /// <summary>
    /// Expiración de la cookie. Debe coincidir con la expiración del token
    /// (TokenService: 2 horas).
    /// </summary>
    public static readonly TimeSpan Duracion = TimeSpan.FromHours(2);

    public static void EstablecerTokenCookie(
        HttpResponse response,
        string token,
        HttpRequest request)
    {
        response.Cookies.Append(CookieName, token, CrearOpciones(request));
    }

    public static void LimpiarTokenCookie(
        HttpResponse response,
        HttpRequest request)
    {
        response.Cookies.Delete(CookieName, CrearOpciones(request));
    }

    private static CookieOptions CrearOpciones(
        HttpRequest request)
    {
        // La cookie se marca Secure+SameSite=None solo cuando la petición llegó
        // realmente por HTTPS (UseForwardedHeaders ya procesó X-Forwarded-Proto
        // detrás del proxy/túnel). Así el frontend (Cloudflare Pages) y la API
        // (Cloudflare Tunnel) cross-site pueden enviarla, mientras que en local
        // o staging por HTTP simple (SameSite=Lax) el navegador no la descarta.
        var esHttps = request.IsHttps;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = esHttps,
            SameSite = esHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(Duracion)
        };
    }
}