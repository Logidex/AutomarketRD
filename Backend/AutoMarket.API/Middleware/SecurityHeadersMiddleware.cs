namespace AutoMarket.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Evita que el navegador intente "adivinar" el Content-Type de un
        // archivo y lo interprete como otra cosa (XSS por MIME confusion).
        headers["X-Content-Type-Options"] = "nosniff";

        // Impide incrustar la API en iframes de otros sitios (clickjacking).
        headers["X-Frame-Options"] = "DENY";

        // Limita la información que se envía en el header Referer.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // HSTS: solo cuando la petición llegó por HTTPS. Detrás de un proxy
        // inverso, ForwardedHeaders ya marcó Request.IsHttps con
        // X-Forwarded-Proto. El CSP se deja a nginx (frontend).
        if (!_env.IsDevelopment() && context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] =
                "max-age=31536000; includeSubDomains";
        }

        return _next(context);
    }
}