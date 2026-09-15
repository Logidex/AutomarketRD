using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace AutoMarket.API.Extensions;

public static class RateLimitingExtensions
{
    public static void AddApplicationRateLimiting(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddRateLimiter(options =>
        {
            var limiteLeads = builder.Configuration.GetValue("RateLimiting:LeadsPermitLimit", 3);
            var ventanaLeads = builder.Configuration.GetValue("RateLimiting:LeadsWindowMinutes", 5);

            options.AddFixedWindowLimiter("PoliticaLeads", limiterOptions =>
            {
                limiterOptions.PermitLimit = limiteLeads;
                limiterOptions.Window = TimeSpan.FromMinutes(ventanaLeads);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            var limiteReportes = builder.Configuration.GetValue("RateLimiting:ReportesPermitLimit", 3);
            var ventanaReportes = builder.Configuration.GetValue("RateLimiting:ReportesWindowHours", 1);

            options.AddFixedWindowLimiter("PoliticaReportes", limiterOptions =>
            {
                limiterOptions.PermitLimit = limiteReportes;
                limiterOptions.Window = TimeSpan.FromHours(ventanaReportes);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            var limiteRegistro = builder.Configuration.GetValue("RateLimiting:RegistroPermitLimit", 5);
            var ventanaRegistro = builder.Configuration.GetValue("RateLimiting:RegistroWindowMinutes", 15);

            options.AddFixedWindowLimiter("PoliticaRegistro", limiterOptions =>
            {
                limiterOptions.PermitLimit = limiteRegistro;
                limiterOptions.Window = TimeSpan.FromMinutes(ventanaRegistro);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.AddPolicy<string>("PoliticaLogin", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
                var clave = $"ip:{ip}";

                var limiteLogin = builder.Configuration.GetValue("RateLimiting:LoginPermitLimit", 30);
                var ventanaLogin = builder.Configuration.GetValue("RateLimiting:LoginWindowMinutes", 15);

                return RateLimitPartition.GetFixedWindowLimiter(clave, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = limiteLogin,
                        Window = TimeSpan.FromMinutes(ventanaLogin),
                        QueueLimit = 0
                    });
            });

            var limiteGlobal = builder.Configuration.GetValue("RateLimiting:GlobalPermitLimit", 300);

            options.AddPolicy<string>("PoliticaRefresh", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
                var clave = $"refresh:{ip}";

                var limiteRefresh = builder.Configuration.GetValue("RateLimiting:RefreshPermitLimit", 20);
                var ventanaRefresh = builder.Configuration.GetValue("RateLimiting:RefreshWindowMinutes", 15);

                return RateLimitPartition.GetFixedWindowLimiter(clave, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = limiteRefresh,
                        Window = TimeSpan.FromMinutes(ventanaRefresh),
                        QueueLimit = 0
                    });
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (context.Request.Path.StartsWithSegments("/health"))
                {
                    return RateLimitPartition.GetNoLimiter("sin-limite-health");
                }

                var ipGlobal = context.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

                return RateLimitPartition.GetFixedWindowLimiter($"global:{ipGlobal}", _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = limiteGlobal,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }
}