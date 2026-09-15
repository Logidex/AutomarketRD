using AutoMarket.API.Fakes;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.BackgroundServices;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Repositories;
using AutoMarket.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AutoMarket.Application.Features.Anuncios.Handlers.AnuncioCommandHandler).Assembly));

        // Caché distribuida (Redis en producción, memoria en desarrollo)
        services.AddSingleton<ICacheService, RedisCacheService>();

        if (builder.Environment.IsDevelopment()
            && string.Equals(builder.Configuration["AWS:AccessKey"], "dummy", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
        }
        else
        {
            services.AddScoped<IAlmacenadorArchivos, AlmacenadorS3>();
        }

        services.AddScoped<IAnuncioService, AnuncioService>();
        services.AddScoped<IAdminAnuncioService, AdminAnuncioService>();
        services.AddScoped<IReporteAnuncioService, ReporteAnuncioService>();
        services.AddScoped<IAnuncioRepository, AnuncioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IReporteAnuncioRepository, ReporteAnuncioRepository>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioCuentaService, UsuarioCuentaService>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IPerfilDealerService, PerfilDealerService>();

        services.AddScoped<ISuscripcionRepository, SuscripcionRepository>();
        services.AddScoped<ISuscripcionService, SuscripcionService>();

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ILeadService, LeadService>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFavoritoService, FavoritoService>();
        services.AddScoped<IFavoritoRepository, FavoritoRepository>();

        services.AddScoped<IComparadorService, ComparadorService>();
        services.AddScoped<ICatalogoService, CatalogoService>();

        services.AddScoped<IHistorialVistaService, HistorialVistaService>();
        services.AddScoped<IHistorialVistaRepository, HistorialVistaRepository>();

        services.AddScoped<IPlanCatalogoRepository, PlanCatalogoRepository>();
        services.AddScoped<IPlanCatalogoService, PlanCatalogoService>();

        services.AddScoped<ICuponRepository, CuponRepository>();
        services.AddScoped<ICuponService, CuponService>();

        services.AddScoped<IEncuestaRepository, EncuestaRepository>();
        services.AddScoped<IEncuestaService, EncuestaService>();

        services.AddScoped<IContactoService, ContactoService>();

        services.AddScoped<IVendedorService, VendedorService>();

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketService, TicketService>();

        services.AddScoped<SmtpEmailSenderService>();
        // ResilientEmailSender wrappear el servicio SMTP real con cola de reintentos
        services.AddScoped<ResilientEmailSender>();
        services.AddScoped<IEmailSenderService>(sp =>
            new ResilientEmailSender(
                sp.GetRequiredService<SmtpEmailSenderService>(),
                sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>(),
                sp.GetRequiredService<ILogger<ResilientEmailSender>>()));

        services.AddScoped<ICuentasBancariasRepository, CuentasBancariasRepository>();
        services.AddScoped<ICuentasBancariasService, CuentasBancariasService>();

        services.AddScoped<IAdSlotRepository, AdSlotRepository>();
        services.AddScoped<IAdSlotService, AdSlotService>();
        services.AddScoped<AdSlotImageService>();

        services.AddScoped<IArchivoService, ArchivoService>();
        services.AddScoped<IAdminUsuarioService, AdminUsuarioService>();
        services.AddScoped<IPagoOrquestacionService, PagoOrquestacionService>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<NotificacionService>();

        services.AddHostedService<SuscripcionMonitorService>();
        services.AddHostedService<AnuncioVencimientoService>();
        services.AddHostedService<AdSlotMonitorService>();
        services.AddHostedService<EmailRetryService>();

        if (builder.Environment.IsDevelopment()
            && string.Equals(builder.Configuration["PayPal:ClientId"], "dummy", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IPayPalService, FakePayPalService>();
        }
        else
        {
            services.AddHttpClient<IPayPalService, PayPalService>();
        }
    }
}