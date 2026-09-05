using AutoMarket.API.Fakes;
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
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Events;

using System.IO.Compression;
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

    // Almacenamiento de archivos: S3/R2 en todos los entornos reales. En
    // Development con credenciales "dummy" (E2E / local sin nube) se usa un
    // almacenador local servido por UseStaticFiles (ver pipeline).
    if (builder.Environment.IsDevelopment()
        && string.Equals(builder.Configuration["AWS:AccessKey"], "dummy", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
    }
    else
    {
        builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorS3>();
    }

    builder.Services.AddScoped<IAnuncioService, AnuncioService>();
    builder.Services.AddScoped<IReporteAnuncioService, ReporteAnuncioService>();
    builder.Services.AddScoped<IAnuncioRepository, AnuncioRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IReporteAnuncioRepository, ReporteAnuncioRepository>();

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

    builder.Services.AddScoped<ICuponRepository, CuponRepository>();
    builder.Services.AddScoped<ICuponService, CuponService>();

    builder.Services.AddScoped<IEncuestaRepository, EncuestaRepository>();
    builder.Services.AddScoped<IEncuestaService, EncuestaService>();

    builder.Services.AddScoped<IContactoService, ContactoService>();

    builder.Services.AddScoped<ITicketRepository, TicketRepository>();
    builder.Services.AddScoped<ITicketService, TicketService>();

    builder.Services.AddScoped<IEmailSenderService, SmtpEmailSenderService>();

    builder.Services.AddScoped<ICuentasBancariasRepository, CuentasBancariasRepository>();
    builder.Services.AddScoped<ICuentasBancariasService, CuentasBancariasService>();

    builder.Services.AddHostedService<SuscripcionMonitorService>();
    builder.Services.AddHostedService<AnuncioVencimientoService>();

    // PayPal real en todos los entornos reales. En Development con ClientId
    // "dummy" (docker-compose.e2e.yml) se usa un simulador que evita llamar a
    // PayPal: el flujo generar-link → pago-exitoso → confirmar-pago funciona
    // de punta a punta sin red externa. Gate doble: entorno + credencial.
    if (builder.Environment.IsDevelopment()
        && string.Equals(builder.Configuration["PayPal:ClientId"], "dummy", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddSingleton<IPayPalService, FakePayPalService>();
    }
    else
    {
        builder.Services.AddHttpClient<IPayPalService, PayPalService>();
    }


    // =======================================================
    // BASE DE DATOS
    // =======================================================

    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Falta ConnectionStrings:DefaultConnection");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.CommandTimeout(30);
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        }));


    // =======================================================
    // REDIS CACHE
    // =======================================================

    var redisHost = builder.Configuration["Redis:Host"] ?? "redis";
    var redisPort = builder.Configuration["Redis:Port"] ?? "6379";
    var redisPassword = builder.Configuration["Redis:Password"] ?? "";
    var redisInstanceName = builder.Configuration["Redis:InstanceName"] ?? "automarket_";

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = $"{redisHost}:{redisPort},password={redisPassword},abortConnect=false";
        options.InstanceName = redisInstanceName;
    });


    // =======================================================
    // RESPONSE COMPRESSION
    // =======================================================

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });


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
    // Un desajuste entre PayPal:Mode y PayPal:UrlBase (Sandbox/Live) es un
    // error de configuración que podría cobrar/cancelar pagos contra el
    // entorno equivocado. Se valida en TODOS los entornos (no solo prod):
    // la verificación local con credenciales Live también debe fallar rápido
    // si se apunta a Sandbox por error.
    // =======================================================

    var paypalUrlBase =
        builder.Configuration["PayPal:UrlBase"];

    var paypalMode =
        builder.Configuration["PayPal:Mode"];

    var esUrlLive =
        !string.IsNullOrWhiteSpace(paypalUrlBase)
        && paypalUrlBase.StartsWith(
            "https://api-m.paypal.com",
            StringComparison.OrdinalIgnoreCase);

    var esUrlSandbox =
        !string.IsNullOrWhiteSpace(paypalUrlBase)
        && paypalUrlBase.StartsWith(
            "https://api-m.sandbox.paypal.com",
            StringComparison.OrdinalIgnoreCase);

    var esModeLive = string.Equals(
        paypalMode, "Live", StringComparison.OrdinalIgnoreCase);

    var esModeSandbox = string.Equals(
        paypalMode, "Sandbox", StringComparison.OrdinalIgnoreCase);

    if (esModeLive && !esUrlLive)
    {
        throw new InvalidOperationException(
            "PayPal:Mode es 'Live' pero PayPal:UrlBase no apunta a https://api-m.paypal.com. Verifica PAYPAL_MODE y PAYPAL_URL_BASE.");
    }

    if (esModeSandbox && !esUrlSandbox)
    {
        throw new InvalidOperationException(
            "PayPal:Mode es 'Sandbox' pero PayPal:UrlBase no apunta a https://api-m.sandbox.paypal.com. Verifica PAYPAL_MODE y PAYPAL_URL_BASE.");
    }

    if (!string.IsNullOrWhiteSpace(paypalMode) && !esModeLive && !esModeSandbox)
    {
        throw new InvalidOperationException(
            "PayPal:Mode debe ser 'Live' o 'Sandbox'. Verifica PAYPAL_MODE.");
    }

    if (builder.Environment.IsProduction() && (!esModeLive || !esUrlLive))
    {
        throw new InvalidOperationException(
            "En producción, PayPal debe estar en Live (PAYPAL_MODE=Live, PAYPAL_URL_BASE=https://api-m.paypal.com).");
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

    // Soporta ambas formas de config:
    //   - arreglo (Cors__AllowedOrigins__0/__1/... o JSON "AllowedOrigins": [...])
    //   - string único separado por comas (Cors__AllowedOrigins)
    var originsDeSeccion = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .GetChildren()
        .ToList();

    var allowedOrigins = originsDeSeccion.Count > 0
        ? originsDeSeccion
            .Where(child => !string.IsNullOrWhiteSpace(child.Value))
            .Select(child => child.Value!)
            .ToArray()
        : builder.Configuration["Cors:AllowedOrigins"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

    if (builder.Environment.IsDevelopment())
    {
        allowedOrigins = new[]
        {
        "http://localhost:5173",
        "http://127.0.0.1:5173"
    };
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(frontendPolicy, policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
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

        // Reportes de anuncios: anónimo, así que límite más estricto
        // (3 reportes por hora por IP) para frenar abuso/spam.
        options.AddFixedWindowLimiter(
            "PoliticaReportes",
            limiterOptions =>
            {
                limiterOptions.PermitLimit = 3;
                limiterOptions.Window =
                    TimeSpan.FromHours(1);

                limiterOptions.QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst;

                limiterOptions.QueueLimit = 0;
            });

        // Registro de cuentas: limita creación de cuentas por IP para
        // prevenir spam/bots (5 registros cada 15 min por IP).
        // Ajustable vía RateLimiting__RegistroPermitLimit (el suite E2E
        // lo eleva: registra varios usuarios por corrida desde una sola IP).
        var limiteRegistro = builder.Configuration
            .GetValue("RateLimiting:RegistroPermitLimit", 5);

        options.AddFixedWindowLimiter(
            "PoliticaRegistro",
            limiterOptions =>
            {
                limiterOptions.PermitLimit = limiteRegistro;
                limiterOptions.Window =
                    TimeSpan.FromMinutes(15);

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

                // Partición por IP en todos los entornos. Un límite por cuenta
                // requeriría leer el email del body (ya consumido por el model
                // binder) o los claims JWT, generando consumos múltiples e
                // inconsistencia. Con ForwardedHeaders activo, RemoteIpAddress
                // es la IP real del cliente detrás de nginx/proxy.
                //
                // Este límite es SOLO un respaldo anti fuerza bruta masiva: el
                // bloqueo real de cuentas vive en la entidad Usuario (por cuenta,
                // no por red). El default de 30/15min no interfiere con el uso
                // normal (incluso fallando en varias cuentas distintas).
                // Ajustable vía RateLimiting__LoginPermitLimit (el suite E2E
                // lo eleva: registra y loguea varios usuarios por corrida).
                var limiteLogin = builder.Configuration
                    .GetValue("RateLimiting:LoginPermitLimit", 30);

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        clave,
                        _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = limiteLogin,
                                Window =
                                    TimeSpan.FromMinutes(15),
                                QueueLimit = 0
                            });
            });

        // Límite global por IP para toda la API (defensa contra abuso general).
        // Los endpoints sensibles llevan además sus políticas específicas
        // (login, leads, contacto). Los health checks quedan exentos porque
        // los sondean Docker y el balanceador cada pocos segundos.
        // Ajustable vía RateLimiting__GlobalPermitLimit (p. ej. para pruebas
        // de carga); el default de 300/min queda como comportamiento normal.
        var limiteGlobal = builder.Configuration
            .GetValue("RateLimiting:GlobalPermitLimit", 300);

        options.GlobalLimiter =
            PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (context.Request.Path.StartsWithSegments("/health"))
                {
                    return RateLimitPartition.GetNoLimiter("sin-limite-health");
                }

                var ipGlobal =
                    context.Connection.RemoteIpAddress?.ToString()
                    ?? "desconocido";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"global:{ipGlobal}",
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = limiteGlobal,
                            Window = TimeSpan.FromMinutes(1),
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
            tags: ["database", "ready"])
        .AddRedis(
            $"{redisHost}:{redisPort},password={redisPassword},abortConnect=false",
            name: "redis",
            failureStatus: HealthStatus.Degraded,
            tags: ["cache", "ready"]);


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
        // Sirve wwwroot/e2e-archivos: el AlmacenadorArchivosLocal de E2E
        // expone las fotos subidas como rutas estáticas.
        app.UseStaticFiles();

        app.MapOpenApi();
        app.MapScalarApiReference();
    }


    // =======================================================
    // PIPELINE HTTP
    // =======================================================

    app.UseResponseCompression();

    app.UseMiddleware<SecurityHeadersMiddleware>();

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


