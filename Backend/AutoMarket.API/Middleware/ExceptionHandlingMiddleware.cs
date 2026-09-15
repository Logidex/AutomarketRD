using System.Net;
using System.Text.Json;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"] as string;

        _logger.LogError(
            exception,
            "[{CorrelationId}] Error no manejado en {Path} - {Method}",
            correlationId ?? "N/A",
            context.Request.Path,
            context.Request.Method
        );

        context.Response.ContentType = "application/json";

        // En producción NO se exponen mensajes que puedan filtrar detalles
        // internos (configuración, respuestas de terceros, EF/BD). El detalle
        // completo siempre va a Serilog (LogError de arriba). Los tipos
        // BusinessRuleException/ArgumentException/KeyNotFoundException/
        // UnauthorizedAccessException sí son el "contrato" de mensajes
        // dirigidos al usuario (validaciones y reglas de negocio), así que
        // se conservan en cualquier entorno.
        var mostrarMensaje =
            _env.IsDevelopment()
            || exception is not InvalidOperationException;

        var (statusCode, message, error) = exception switch
        {
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                exception.Message,
                "Unauthorized"
            ),
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                exception.Message,
                "NotFound"
            ),
            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                exception.Message,
                "BadRequest"
            ),
            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                mostrarMensaje
                    ? exception.Message
                    : "La operación no se pudo completar en este momento.",
                "InvalidOperation"
            ),
            BusinessRuleException => (
                (int)HttpStatusCode.BadRequest,
                exception.Message,
                "BusinessRule"
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Ocurrió un error interno. Por favor contacta al administrador.",
                "InternalServerError"
            )
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            exito = false,
            mensaje = message,
            error = error,
            stackTrace = _env.IsDevelopment() ? exception.StackTrace : null
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(response, options);
    }
}