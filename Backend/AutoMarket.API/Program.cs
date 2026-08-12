using AutoMarket.API.Middleware;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.BackgroundServices;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Repositories;
using AutoMarket.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Events;

using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;


// =======================================================
// CONFIGURACIÓN DE SERILOG
// =======================================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override(
        "Microsoft.EntityFrameworkCore",
        LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/api-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();


try
{
    Log.Information("Iniciando AutoMarket.API");

    var builder = WebApplication.CreateBuilder(args);


    // =======================================================
    // LÍMITES DE SUBIDA DE ARCHIVOS
    // =======================================================

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize =
            50L * 1024 * 1024;
    });

    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit =
            50L * 1024 * 1024;
    });


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
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter());
        });


    // =======================================================
    // SERVICIOS DE LA APLICACIÓN
    // =======================================================

    builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorS3>();

    builder.Services.AddScoped<IAnuncioService, AnuncioService>();
    builder.Services.AddScoped<IAnuncioRepository, AnuncioRepository>();

    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IUsuarioCuentaService, UsuarioCuentaService>();

    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    builder.Services.AddScoped<IPerfilDealerService, PerfilDealerService>();

    builder.Services.AddScoped<ISuscripcionRepository, SuscripcionRepository>();
    builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();

    builder.Services.AddScoped<ILeadRepository, LeadRepository>();
    builder.Services.AddScoped<ILeadService, LeadService>();

    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IFavoritoService, FavoritoService>();
    builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();

    builder.Services.AddScoped<IComparadorService, ComparadorService>();
    builder.Services.AddScoped<ICatalogoService, CatalogoService>();

    builder.Services.AddScoped<IHistorialVistaService, HistorialVistaService>();
    builder.Services.AddScoped<IHistorialVistaRepository, HistorialVistaRepository>();

    builder.Services.AddScoped<IPlanCatalogoRepository, PlanCatalogoRepository>();
    builder.Services.AddScoped<IPlanCatalogoService, PlanCatalogoService>();

    builder.Services.AddScoped<IContactoService, ContactoService>();

    builder.Services.AddScoped<IEmailSenderService, SmtpEmailSenderService>();

    builder.Services.AddHostedService<SuscripcionMonitorService>();

    builder.Services.AddHttpClient<IPayPalService, PayPalService>();


    // =======================================================
    // BASE DE DATOS
    // =======================================================

    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Falta ConnectionStrings:DefaultConnection");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));


    // =======================================================
    // JWT
    // =======================================================

    var jwtSecret =
        builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException(
            "Falta Jwt:Secret");

    if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Secret debe tener al menos 32 bytes.");
    }

    builder.Services.AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer =
                        builder.Configuration["Jwt:Issuer"],

                    ValidAudience =
                        builder.Configuration["Jwt:Audience"],

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecret)),

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,

                    ClockSkew = TimeSpan.Zero
                };
        });

    builder.Services.AddAuthorization();


    // =======================================================
    // CORS
    // =======================================================

    const string frontendPolicy = "FrontendCorsPolicy";

    var allowedOrigins =
        builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(frontendPolicy, policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
            else if (allowedOrigins.Length > 0)
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        });
    });


    // =======================================================
    // RATE LIMITING
    // =======================================================

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter(
            "PoliticaLeads",
            limiterOptions =>
            {
                limiterOptions.PermitLimit = 3;
                limiterOptions.Window =
                    TimeSpan.FromMinutes(5);

                limiterOptions.QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst;

                limiterOptions.QueueLimit = 0;
            });

        options.AddPolicy(
            "PoliticaLogin",
            context =>
            {
                var ip =
                    context.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "desconocido";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        ip,
                        _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 5,
                                Window =
                                    TimeSpan.FromMinutes(15),
                                QueueLimit = 0
                            });
            });

        options.RejectionStatusCode =
            StatusCodes.Status429TooManyRequests;
    });


    // =======================================================
    // HEALTH CHECKS
    // =======================================================

    builder.Services
        .AddHealthChecks()
        .AddCheck(
            "self",
            () =>
                HealthCheckResult.Healthy(
                    "API está funcionando"))
        .AddNpgSql(
            connectionString,
            name: "postgres",
            tags: ["database", "ready"]);


    // =======================================================
    // OPENAPI
    // =======================================================

    builder.Services.AddOpenApi();


    // =======================================================
    // FORWARDED HEADERS
    // =======================================================

    builder.Services.Configure<ForwardedHeadersOptions>(
        options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });


    var app = builder.Build();


    // =======================================================
    // FORWARDED HEADERS
    // Debe ejecutarse antes del rate limiting.
    // =======================================================

    if (!app.Environment.IsDevelopment())
    {
        app.UseForwardedHeaders();
    }


    // =======================================================
    // MIGRACIONES Y SEEDER
    // =======================================================

    var migrateOnStartup =
        app.Environment.IsDevelopment()
        || app.Configuration.GetValue<bool>(
            "MigrateOnStartup");

    if (migrateOnStartup)
    {
        Log.Information(
            "Aplicando migraciones. Entorno: {Environment}",
            app.Environment.EnvironmentName);

        using var migrationScope =
            app.Services.CreateScope();

        var dbContext =
            migrationScope
                .ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();

        Log.Information(
            "Migraciones aplicadas correctamente.");
    }
    else
    {
        Log.Information(
            "Migraciones automáticas deshabilitadas. Entorno: {Environment}",
            app.Environment.EnvironmentName);
    }


    var seederEnabled =
        app.Environment.IsDevelopment()
        || app.Configuration.GetValue<bool>(
            "Seeder:Enabled");

    if (seederEnabled)
    {
        Log.Information(
            "Ejecutando DatabaseSeeder. Entorno: {Environment}",
            app.Environment.EnvironmentName);

        await DatabaseSeeder.SeedAsync(app.Services);

        Log.Information(
            "DatabaseSeeder ejecutado correctamente.");
    }
    else
    {
        Log.Information(
            "DatabaseSeeder deshabilitado. Entorno: {Environment}",
            app.Environment.EnvironmentName);
    }


    // =======================================================
    // OPENAPI Y SCALAR
    // =======================================================

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }


    // =======================================================
    // PIPELINE HTTP
    // =======================================================

    app.UseCors(frontendPolicy);

    app.UseRateLimiter();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapControllers();


    // =======================================================
    // HEALTH CHECK GENERAL
    // =======================================================

    app.MapHealthChecks("/health");


    // =======================================================
    // HEALTH CHECK DE DEPENDENCIAS
    // =======================================================

    app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = check =>
                check.Tags.Contains("ready"),

            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType =
                    "application/json";

                var result = new
                {
                    status = report.Status.ToString(),
                    duration = report.TotalDuration,
                    checks = report.Entries.Select(entry =>
                        new
                        {
                            name = entry.Key,
                            status =
                                entry.Value.Status.ToString(),
                            description =
                                entry.Value.Description,
                            duration =
                                entry.Value.Duration
                        })
                };

                await context.Response.WriteAsJsonAsync(result);
            }
        });


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "La aplicación terminó inesperadamente");
}
finally
{
    await Log.CloseAndFlushAsync();
}
