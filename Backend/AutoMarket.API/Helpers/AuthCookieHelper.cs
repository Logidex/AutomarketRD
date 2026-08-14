using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace AutoMarket.API.Helpers;

/// <summary>
/// Maneja el JWT como cookie HttpOnly (SameSite=Lax) en lugar de localStorage.
/// El token viaja únicamente en la cookie; nunca se expone al JavaScript.
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
        IWebHostEnvironment environment)
    {
        response.Cookies.Append(CookieName, token, CrearOpciones(environment));
    }

    public static void LimpiarTokenCookie(
        HttpResponse response,
        IWebHostEnvironment environment)
    {
        response.Cookies.Delete(CookieName, CrearOpciones(environment));
    }

    private static CookieOptions CrearOpciones(
        IWebHostEnvironment environment)
    {
        // En producción (HTTPS): Secure=true, SameSite=Lax
        // En desarrollo/staging (HTTP local): Secure=false, SameSite=Lax
        var esProduccion = environment.IsProduction();
        
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = esProduccion,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(Duracion)
        };
    }
}