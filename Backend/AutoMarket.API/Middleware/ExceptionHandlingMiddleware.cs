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
        _logger.LogError(
            exception,
            "Error no manejado en {Path} - {Method}",
            context.Request.Path,
            context.Request.Method
        );

        context.Response.ContentType = "application/json";

        var (statusCode, message, error) = exception switch
        {
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "No autorizado. Debes iniciar sesión.",
                "Unauthorized"
            ),
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                "El recurso solicitado no fue encontrado.",
                "NotFound"
            ),
            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                exception.Message,
                "BadRequest"
            ),
            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                exception.Message,
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

        // En producción no exponemos el stack trace
        var response = new
        {
            success = false,
            message = message,
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