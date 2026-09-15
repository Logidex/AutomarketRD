using System.Security.Cryptography;
using System.Text;

namespace AutoMarket.API.Middleware;

/// <summary>
/// Protección CSRF mediante patrón double-submit cookie.
/// Genera un token SHA-256 y lo guarda en una cookie (SameSite=Strict).
/// El frontend debe enviar el mismo valor en el header "X-CSRF-Token".
/// Solo aplica a métodos mutantes (POST, PUT, PATCH, DELETE).
/// </summary>
public sealed class CsrfMiddleware
{
    private const string COOKIE_NAME = "automarket_csrf";
    private const string HEADER_NAME = "X-CSRF-Token";
    private const int TOKEN_LENGTH = 32;

    private readonly RequestDelegate _next;

    public CsrfMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;

        if (HttpMethods.IsGet(method) ||
            HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method))
        {
            await _next(context);
            return;
        }

        // Exentar webhooks externos (PayPal, etc.) que no tienen la cookie CSRF
        if (context.Request.Path.StartsWithSegments("/api/pagos/webhook"))
        {
            await _next(context);
            return;
        }

        var cookieToken = context.Request.Cookies[COOKIE_NAME];
        var headerToken = context.Request.Headers[HEADER_NAME].FirstOrDefault();

        if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(cookieToken),
                Encoding.UTF8.GetBytes(headerToken)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                exito = false,
                mensaje = "Token CSRF inválido."
            });
            return;
        }

        await _next(context);
    }
}
