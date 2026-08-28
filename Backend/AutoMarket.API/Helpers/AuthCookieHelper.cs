using Microsoft.AspNetCore.Http;

namespace AutoMarket.API.Helpers;

/// <summary>
/// Maneja el JWT como cookie HttpOnly (SameSite=Lax sobre HTTP, None+Secure sobre HTTPS)
/// en lugar de localStorage. El token viaja únicamente en la cookie; nunca se expone al JavaScript.
/// </summary>
public static class AuthCookieHelper
{
    public const string CookieName = "automarket_token";
    public const string RefreshCookieName = "automarket_rt";

    /// <summary>Expiración del access token (coincide con TokenService: 2 horas).</summary>
    public static readonly TimeSpan Duracion = TimeSpan.FromHours(2);

    /// <summary>Expiración de la sesión completa (refresh token: 14 días).</summary>
    public static readonly TimeSpan DuracionRefresh = TimeSpan.FromDays(14);

    public static void EstablecerTokenCookie(
        HttpResponse response,
        string token,
        HttpRequest request)
    {
        response.Cookies.Append(CookieName, token, CrearOpciones(request, Duracion));
    }

    /// <summary>
    /// El refresh token solo viaja hacia /api/auth/*: limitar el Path minimiza
    /// su exposición en cualquier petición fuera de la autenticación.
    /// </summary>
    public static void EstablecerRefreshCookie(
        HttpResponse response,
        string refreshToken,
        HttpRequest request)
    {
        var opciones = CrearOpciones(request, DuracionRefresh);
        opciones.Path = "/api/auth";
        response.Cookies.Append(RefreshCookieName, refreshToken, opciones);
    }

    public static void LimpiarTokenCookie(
        HttpResponse response,
        HttpRequest request)
    {
        response.Cookies.Delete(CookieName, CrearOpciones(request, Duracion));
        response.Cookies.Delete(RefreshCookieName, CrearOpciones(request, DuracionRefresh, "/api/auth"));
    }

    private static CookieOptions CrearOpciones(
        HttpRequest request,
        TimeSpan duracion,
        string? path = null)
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
            Path = path ?? "/",
            Expires = DateTimeOffset.UtcNow.Add(duracion)
        };
    }
}