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
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;


// =======================================================
// CONFIGURACIÓN DE SERILOG
// =======================================================

var esDesarrollo = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Development",
    StringComparison.OrdinalIgnoreCase);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(esDesarrollo ? LogEventLevel.Debug : LogEventLevel.Information)
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

    builder.Services.AddScoped<ITicketRepository, TicketRepository>();
    builder.Services.AddScoped<ITicketService, TicketService>();

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


    // =======================================================
    // VALIDACIÓN DE PAYPAL
    // En producción la API nunca debe apuntar a Sandbox: un error de
    // configuración podría cobrar/cancelar pagos contra el entorno equivocado.
    // =======================================================

    if (builder.Environment.IsProduction())
    {
        var paypalUrlBase =
            builder.Configuration["PayPal:UrlBase"];

        var esLive =
            !string.IsNullOrWhiteSpace(paypalUrlBase)
            && paypalUrlBase.StartsWith(
                "https://api-m.paypal.com",
                StringComparison.OrdinalIgnoreCase);

        if (!esLive)
        {
            throw new InvalidOperationException(
                "En producción, PayPal:UrlBase debe ser https://api-m.paypal.com (Live). Verifica PAYPAL_URL_BASE.");
        }

        var paypalMode =
            builder.Configuration["PayPal:Mode"];

        if (!string.Equals(
                paypalMode,
                "Live",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "En producción, PayPal:Mode debe ser 'Live'. Verifica PAYPAL_MODE.");
        }
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token =
                    context.Request.Cookies["automarket_token"];

                var logger =
                    context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearer");

                logger.LogDebug(
                    "[JWT] Cookie recibida: {TieneCookie}",
                    !string.IsNullOrWhiteSpace(token));

                context.Token = token;

                return Task.CompletedTask;
            }
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

    // En desarrollo el SPA corre en el dev server de Vite (puerto 5173).
    // AllowCredentials exige orígenes explícitos (AllowAnyOrigin + AllowCredentials
    // es una combinación inválida y el navegador rechaza la cookie con credenciales).
    string[] devOrigins =
    [
        "http://localhost:5173",
        "http://127.0.0.1:5173"
    ];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(frontendPolicy, policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy
                    .WithOrigins(devOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
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

        options.AddPolicy<string>(
            "PoliticaLogin",
            context =>
            {
                var ip =
                    context.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "desconocido";

                var clave = $"ip:{ip}";

                if (!builder.Environment.IsProduction())
                {
                    // En Dev/Staging: usar partición por IP simple.
                    // La extracción de email del body causa consumos múltiples
                    // y rate limiting inconsistente (ya consumido por model binder).
                    // Si se necesita límites por cuenta, usar JWT claims en lugar
                    // de leer el body consumido.
                }

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        clave,
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

            options.ForwardLimit = 2;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            // Solo confiar en headers provenientes de proxies privados
            // (red de Docker/nginx). Si la lista está vacía, .NET confía en
            // TODOS los proxies, lo cual permite spoofear X-Forwarded-For.
            options.KnownIPNetworks.Add(
                new System.Net.IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
            options.KnownIPNetworks.Add(
                new System.Net.IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
            options.KnownIPNetworks.Add(
                new System.Net.IPNetwork(IPAddress.Parse("192.168.0.0"), 16));
            options.KnownIPNetworks.Add(
                new System.Net.IPNetwork(IPAddress.Loopback, 8));
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


