using AutoMarket.API.Extensions;

using Serilog;

using System.Text.Json.Serialization;

// =======================================================
// CONFIGURACIÓN DE SERILOG
// =======================================================

var esDesarrollo = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Development",
    StringComparison.OrdinalIgnoreCase);

SerilogExtensions.ConfigureSerilog(esDesarrollo);

try
{
    Log.Information("Iniciando AutoMarket.API");

    var builder = WebApplication.CreateBuilder(args);

    // =======================================================
    // LÍMITES DE SUBIDA DE ARCHIVOS
    // =======================================================
    builder.ConfigureUploadLimits();

    // =======================================================
    // SERILOG
    // =======================================================
    builder.Host.UseSerilog();

    // =======================================================
    // CONTROLLERS
    // =======================================================
    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    // =======================================================
    // SERVICIOS DE LA APLICACIÓN (DI + MediatR + PayPal)
    // =======================================================
    builder.Services.AddApplicationServices(builder);

    // =======================================================
    // BASE DE DATOS + REDIS
    // =======================================================
    var connectionString = builder.Services.AddApplicationDatabase(builder);
    var redis = builder.Services.AddRedisCache(builder);

    // =======================================================
    // RESPONSE COMPRESSION
    // =======================================================
    builder.Services.AddApplicationCompression();

    // =======================================================
    // JWT
    // =======================================================
    builder.Services.AddJwtAuthentication(builder);

    // =======================================================
    // VALIDACIÓN DE PAYPAL
    // =======================================================
    var paypalUrlBase = builder.ValidatePayPalConfiguration();

    // =======================================================
    // CORS
    // =======================================================
    var frontendPolicy = builder.Services.AddFrontendCors(builder);

    // =======================================================
    // RATE LIMITING
    // =======================================================
    builder.Services.AddApplicationRateLimiting(builder);

    // =======================================================
    // HEALTH CHECKS
    // =======================================================
    builder.Services.AddApplicationHealthChecks(
        connectionString, redis.Host, redis.Port, redis.Password, paypalUrlBase);

    // =======================================================
    // OPENAPI
    // =======================================================
    builder.Services.AddOpenApi();

    // =======================================================
    // FORWARDED HEADERS
    // =======================================================
    builder.Services.AddForwardedHeaders();

    var app = builder.Build();

    // =======================================================
    // MIGRACIONES Y SEEDER
    // =======================================================
    app.RunMigrationsAndSeeder();

    // =======================================================
    // PIPELINE HTTP + HEALTH CHECKS
    // =======================================================
    app.ConfigureHttpPipeline(frontendPolicy);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    await Log.CloseAndFlushAsync();
}