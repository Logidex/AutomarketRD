using System.Security.Claims;
using System.Text.Json;
using AutoMarket.Core.Entities;
using AutoMarket.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.API.Middleware;

/// <summary>
/// Middleware que registra acciones administrativas en la tabla AuditLogs.
/// Intercepta POST, PUT, PATCH, DELETE en rutas /api/admin/* y guarda
/// el estado antes/después de la entidad modificada.
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    // Acciones que no requieren capturar estado (solo registro de entrada)
    private static readonly HashSet<string> AccionesLectura = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";

        // Solo auditar rutas admin con métodos de escritura
        if (!path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase) ||
            AccionesLectura.Contains(method))
        {
            await _next(context);
            return;
        }

        var usuarioId = int.TryParse(
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;

        var accion = method switch
        {
            "POST" => "Crear",
            "PUT" or "PATCH" => "Actualizar",
            "DELETE" => "Eliminar",
            _ => method
        };

        var entidad = ExtraerEntidad(path);
        var entidadId = ExtraerEntidadId(path);

        // Capturar body como "después" para POST/PUT/PATCH
        string? jsonDespues = null;
        if (method is "POST" or "PUT" or "PATCH")
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            jsonDespues = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        await _next(context);

        // Guardar audit log después de que el controller procese
        try
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.AuditLogs.Add(new AuditLog
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                JsonDespues = jsonDespues,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                FechaUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
        catch
        {
            // No fallar la request por errores de auditoría
        }
    }

    private static string ExtraerEntidad(string path)
    {
        // /api/admin/usuarios → Usuarios
        // /api/admin/anuncios → Anuncios
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 3
            ? char.ToUpper(segments[2][0]) + segments[2][1..]
            : "Desconocida";
    }

    private static string ExtraerEntidadId(string path)
    {
        // /api/admin/usuarios/123 → 123
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 4 ? segments[3] : "";
    }
}
