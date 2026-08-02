using AutoMarket.Core.Entities.Enums;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Infrastructure.BackgroundServices;

public class SuscripcionMonitorService : BackgroundService
{
    private readonly ILogger<SuscripcionMonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;

    // Inyectamos IServiceProvider porque BackgroundService es Singleton 
    // y ApplicationDbContext es Scoped. No podemos inyectarlo directamente.
    public SuscripcionMonitorService(
        ILogger<SuscripcionMonitorService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("El Ejecutor Silencioso (SuscripcionMonitorService) ha iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarMorososAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico al procesar la degradación masiva de suscripciones.");
            }

            // Para entorno de desarrollo, puedes cambiar esto a TimeSpan.FromMinutes(1) para probarlo rápido.
            // En producción, esto dormirá el hilo sin consumir CPU hasta la próxima ejecución.
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task ProcesarMorososAsync(CancellationToken stoppingToken)
    {
        // Abrimos un Scope para instanciar nuestra base de datos de forma segura
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Regla de Negocio: Más de 2 días de gracia vencidos
        var fechaLimite = DateTime.UtcNow.AddDays(-2);

        _logger.LogInformation("Ejecutando barrido de dealers morosos. Límite de gracia: {FechaLimite}", fechaLimite);

        // 1. Encontrar a los usuarios con suscripción vencida o cancelada
        var usuariosMorososIds = await dbContext.Usuarios
            .Where(u => u.PerfilDealer != null && 
                        u.PerfilDealer.Suscripcion != null &&
                        (u.PerfilDealer.Suscripcion.FechaVencimientoUtc < fechaLimite || 
                         u.PerfilDealer.Suscripcion.Estado == EstadoSuscripcion.Cancelada))
            .Select(u => u.UsuarioId)
            .ToListAsync(stoppingToken);

        if (!usuariosMorososIds.Any())
        {
            _logger.LogInformation("Barrido completado. Ningún dealer excede el periodo de gracia hoy.");
            return;
        }

        int totalAnunciosPausados = 0;

        // 2. Procesar la penalización para cada moroso
        foreach (var usuarioId in usuariosMorososIds)
        {
            // Traemos solo los IDs de los anuncios activos, ordenados del más reciente al más viejo
            var anunciosPublicadosIds = await dbContext.Anuncios
                .Where(a => a.UsuarioId == usuarioId && a.Estado == "Publicado")
                .OrderByDescending(a => a.Id) // Usamos Id asumiendo que un Id mayor equivale a un registro más nuevo
                .Select(a => a.Id)
                .ToListAsync(stoppingToken);

            // Si tiene más de un anuncio activo, lo castigamos dejando solo 1 (el más reciente)
            if (anunciosPublicadosIds.Count > 1)
            {
                // Saltamos el primero (el más reciente) y tomamos el resto
                var idsAPausar = anunciosPublicadosIds.Skip(1).ToList();

                // 3. El Ejecutor: Usamos ExecuteUpdateAsync para actualizar masivamente sin trackear entidades
                int pausados = await dbContext.Anuncios
                    .Where(a => idsAPausar.Contains(a.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(a => a.Estado, "Pausado"), stoppingToken);

                totalAnunciosPausados += pausados;
                _logger.LogWarning("Dealer ID: {UsuarioId} penalizado. Se pausaron {Cantidad} anuncios.", usuarioId, pausados);
            }
        }

        _logger.LogInformation("Barrido finalizado con éxito. Total de vehículos retirados de la vitrina pública: {Total}", totalAnunciosPausados);
    }
}