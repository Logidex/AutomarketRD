using AutoMarket.API.Middleware;
using AutoMarket.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

namespace AutoMarket.API.Extensions;

public static class WebApplicationExtensions
{
    public static void RunMigrationsAndSeeder(this WebApplication app)
    {
        var esProduccion = app.Environment.IsProduction();

        var migrateOnStartup =
            !esProduccion
            && (app.Environment.IsDevelopment()
                || app.Configuration.GetValue<bool>("MigrateOnStartup"));

        if (migrateOnStartup)
        {
            Log.Information("Aplicando migraciones. Entorno: {Environment}", app.Environment.EnvironmentName);

            using var migrationScope = app.Services.CreateScope();
            var dbContext = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            Log.Information("Migraciones aplicadas correctamente.");
        }
        else if (esProduccion)
        {
            Log.Warning("Migraciones automáticas BLOQUEADAS en producción. Entorno: {Environment}", app.Environment.EnvironmentName);
        }
        else
        {
            Log.Information("Migraciones automáticas deshabilitadas. Entorno: {Environment}", app.Environment.EnvironmentName);
        }

        var seederEnabled =
            !esProduccion
            && (app.Environment.IsDevelopment()
                || app.Configuration.GetValue<bool>("Seeder:Enabled"));

        if (seederEnabled)
        {
            Log.Information("Ejecutando DatabaseSeeder. Entorno: {Environment}", app.Environment.EnvironmentName);
            DatabaseSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();
            Log.Information("DatabaseSeeder ejecutado correctamente.");
        }
        else if (esProduccion)
        {
            Log.Information("DatabaseSeeder BLOQUEADO en producción. Entorno: {Environment}", app.Environment.EnvironmentName);
        }
        else
        {
            Log.Information("DatabaseSeeder deshabilitado. Entorno: {Environment}", app.Environment.EnvironmentName);
        }
    }

    public static void ConfigureHttpPipeline(this WebApplication app, string frontendPolicy)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseForwardedHeaders();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseStaticFiles();
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseResponseCompression();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseCors(frontendPolicy);
        app.UseMiddleware<CsrfMiddleware>();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();

        app.MapHealthChecks("/health");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var result = new
                {
                    status = report.Status.ToString(),
                    duration = report.TotalDuration,
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration
                    })
                };

                await context.Response.WriteAsJsonAsync(result);
            }
        });
    }
}