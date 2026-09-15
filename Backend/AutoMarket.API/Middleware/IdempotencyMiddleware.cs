using System.Net;
using System.Text.Json;
using AutoMarket.API.Attributes;
using Microsoft.Extensions.Caching.Distributed;

namespace AutoMarket.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;

        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IdempotentAttribute>() is null)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) || string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                exito = false,
                mensaje = "El header Idempotency-Key es requerido para esta operación.",
                error = "MissingIdempotencyKey"
            }, JsonOptions);
            return;
        }

        var idempotencyKey = keyValues.First()!;
        var cacheKey = $"idempotency:{method}:{idempotencyKey}";

        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();

        var cachedBytes = await cache.GetAsync(cacheKey, context.RequestAborted);
        if (cachedBytes is not null)
        {
            var cached = System.Text.Encoding.UTF8.GetString(cachedBytes);
            _logger.LogDebug("Idempotency cache hit para key {Key}", idempotencyKey);

            var entry = JsonSerializer.Deserialize<IdempotencyEntry>(cached, JsonOptions)!;
            context.Response.StatusCode = entry.StatusCode;
            context.Response.ContentType = entry.ContentType ?? "application/json";

            if (entry.Body is not null)
            {
                await context.Response.WriteAsync(entry.Body, context.RequestAborted);
            }
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var replacementStream = new MemoryStream();
        context.Response.Body = replacementStream;

        try
        {
            await _next(context);

            replacementStream.Position = 0;
            var responseBody = await new StreamReader(replacementStream).ReadToEndAsync(context.RequestAborted);

            var entryToCache = new IdempotencyEntry
            {
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                Body = responseBody
            };

            var serialized = JsonSerializer.Serialize(entryToCache, JsonOptions);
            var bytesToCache = System.Text.Encoding.UTF8.GetBytes(serialized);
            await cache.SetAsync(cacheKey, bytesToCache, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            }, context.RequestAborted);

            _logger.LogDebug("Idempotency cache stored para key {Key} con status {Status}", idempotencyKey, context.Response.StatusCode);

            replacementStream.Position = 0;
            await replacementStream.CopyToAsync(originalBodyStream, context.RequestAborted);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private sealed class IdempotencyEntry
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public string? Body { get; set; }
    }
}
