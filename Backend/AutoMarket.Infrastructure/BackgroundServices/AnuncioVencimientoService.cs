using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Infrastructure.BackgroundServices;

public class AnuncioVencimientoService : BackgroundService
{
    private readonly ILogger<AnuncioVencimientoService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AnuncioVencimientoService(
        ILogger<AnuncioVencimientoService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de vencimiento de anuncios gratis iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarAnunciosVencidosGratisAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar anuncios gratis vencidos.");
            }

            // Ejecutar cada hora
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcesarAnunciosVencidosGratisAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var ahora = DateTime.UtcNow;

        _logger.LogInformation("Procesando anuncios gratis vencidos. Hora: {Ahora}", ahora);

        // Buscar anuncios gratis vencidos (FechaVencimientoGratisUtc <= ahora)
        // que estén en estado "Publicado"
        var anunciosVencidos = await dbContext.Anuncios
            .Where(a => a.Estado == "Publicado" &&
                        a.FechaVencimientoGratisUtc.HasValue &&
                        a.FechaVencimientoGratisUtc.Value <= DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        if (!anunciosVencidos.Any())
        {
            _logger.LogInformation("No hay anuncios gratis vencidos para procesar.");
            return;
        }

        int procesados = 0;

        foreach (var anuncio in anunciosVencidos)
        {
            try
            {
                // Cambiar estado a "Vencido" (no visible en vitrina, pero recuperable)
                anuncio.CambiarEstado("Vencido");
                await dbContext.SaveChangesAsync(stoppingToken);

                procesados++;
                _logger.LogInformation("Anuncio ID {AnuncioId} marcado como Vencido (plan gratis vencido).", anuncio.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar anuncio ID {AnuncioId}", anuncio.Id);
            }
        }

        _logger.LogInformation("Procesamiento de anuncios gratis vencidos finalizado. Procesados: {Total}", procesados);
    }
}

public static class AnuncioVencimientoServiceExtensions
{
    public static IServiceCollection AddAnuncioVencimientoService(this IServiceCollection services)
    {
        services.AddHostedService<AnuncioVencimientoService>();
        return services;
    }
}