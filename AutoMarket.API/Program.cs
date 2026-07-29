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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorS3>();
builder.Services.AddScoped<AnuncioService>();
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

builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("Falta Jwt:Secret");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// =======================================================
// CONFIGURACIÓN DE CORS (Para el Frontend en React)
// =======================================================
var frontendPolicy = "FrontendCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: frontendPolicy,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// =======================================================
// CONFIGURACIÓN DE RATE LIMITING (Protección contra Spam)
// =======================================================
builder.Services.AddRateLimiter(options =>
{
    // Política específica para el envío de correos/leads
    options.AddFixedWindowLimiter("PoliticaLeads", limiterOptions =>
    {
        limiterOptions.PermitLimit = 3; // Máximo 3 peticiones
        limiterOptions.Window = TimeSpan.FromMinutes(5); // Cada 5 minutos
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // Si se pasa de 3, rechazar inmediatamente (no poner en cola)
    });
    
    // Devolver un error 429 (Too Many Requests) cuando se exceda el límite
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

DatabaseSeeder.SeedAsync(app.Services).Wait();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors(frontendPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();