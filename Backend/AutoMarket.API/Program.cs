using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Repositories;
using AutoMarket.Infrastructure.Services;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMarket.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using AutoMarket.API.Middleware;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using System.Security.Claims;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/api-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando AutoMarket.API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorS3>();
    builder.Services.AddScoped<IAnuncioService, AnuncioService>();
    builder.Services.AddScoped<IAnuncioRepository, AnuncioRepository>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IPerfilDealerService, PerfilDealerService>();
    builder.Services.AddScoped<ISuscripcionRepository, SuscripcionRepository>();
    builder.Services.AddScoped<ILeadRepository, LeadRepository>();
    builder.Services.AddHostedService<SuscripcionMonitorService>();
    builder.Services.AddScoped<IEmailSenderService, SmtpEmailSenderService>();
    builder.Services.AddScoped<ILeadService, LeadService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IFavoritoService, FavoritoService>();
    builder.Services.AddScoped<IComparadorService, ComparadorService>();
    builder.Services.AddScoped<ICatalogoService, CatalogoService>();
    builder.Services.AddScoped<IHistorialVistaService, HistorialVistaService>();
    builder.Services.AddHttpClient<IPayPalService, PayPalService>();
    builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
    builder.Services.AddScoped<IHistorialVistaRepository, HistorialVistaRepository>();
    builder.Services.AddScoped<IUsuarioCuentaService, UsuarioCuentaService>();
    builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();
    builder.Services.AddScoped<IPlanCatalogoRepository, PlanCatalogoRepository>();
    builder.Services.AddScoped<IPlanCatalogoService, PlanCatalogoService>();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Falta Jwt:Secret");

    builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme
)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)
        ),

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,

        ClockSkew = TimeSpan.Zero
    };
});

    builder.Services.AddAuthorization();

    // =======================================================
    // CONFIGURACIÓN DE CORS (Para el Frontend en React)
    // =======================================================
    var frontendPolicy = "FrontendCorsPolicy";
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(frontendPolicy, policy =>
        {
            // En desarrollo permitimos cualquier origen: cubre túneles
            // tipo VS Code Dev Tunnels con subdominios dinámicos
            // (ej. https://xxxx-5173.use2.devtunnels.ms). El frontend
            // autentica con token JWT por header, no por cookies.
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
        });
    });

    // =======================================================
    // CONFIGURACIÓN DE RATE LIMITING (Protección contra Spam)
    // =======================================================
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("PoliticaLeads", limiterOptions =>
        {
            limiterOptions.PermitLimit = 3;
            limiterOptions.Window = TimeSpan.FromMinutes(5);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Política de protección contra fuerza bruta en el login
        options.AddPolicy("PoliticaLogin", context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

            return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
                new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0
                });
        });
    });

    // =======================================================
    // CONFIGURACIÓN DE HEALTH CHECKS (Monitoreo de Salud)
    // =======================================================
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("API está funcionando"))
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgres",
            tags: new[] { "database", "ready" }
        );

    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Aplicar migraciones automáticamente
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    // Luego seedear (solo Desarrollo, o si se fuerza con Seeder:Enabled=true)
    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Seeder:Enabled"))
    {
        DatabaseSeeder.SeedAsync(app.Services).Wait();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseCors(frontendPolicy);

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    // ===== MIDDLEWARE DE MANEJO DE ERRORES =====
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapControllers();

    // =======================================================
    // ENDPOINTS DE HEALTH CHECKS
    // =======================================================
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
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration
                })
            };

            await context.Response.WriteAsJsonAsync(result);
        }
    });

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
