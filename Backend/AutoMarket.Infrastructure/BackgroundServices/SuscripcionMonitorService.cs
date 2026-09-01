using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

            try
            {
                await EnviarRecordatoriosDeRenovacionAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico al enviar recordatorios de renovación.");
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

        // 1. Encontrar a los usuarios con suscripción vencida (la cancelación
        //    no retira los beneficios ya pagados: solo la fecha de vencimiento)
        // EXCLUIR plan Gratis (no vence, se renueva manualmente por anuncio)
        var usuariosMorososIds = await dbContext.Usuarios
            .Where(u => u.PerfilDealer != null && 
                        u.PerfilDealer.Suscripcion != null &&
                        u.PerfilDealer.Suscripcion.Nivel != PlanNivel.Gratis &&
                        u.PerfilDealer.Suscripcion.FechaVencimientoUtc < fechaLimite)
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

    private async Task EnviarRecordatoriosDeRenovacionAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var diasAntes = configuration.GetValue<int?>("Recordatorios:DiasAntesRenovacion") ?? 7;

        if (diasAntes <= 0)
        {
            _logger.LogInformation("Recordatorios de renovación desactivados (config inválida).");
            return;
        }

        var ahora = DateTime.UtcNow;
        var fechaLimite = ahora.AddDays(diasAntes);

        _logger.LogInformation(
            "Buscando suscripciones que vencen dentro de los próximos {Dias} días para enviar recordatorio.",
            diasAntes);

        var porVencer = await dbContext.SuscripcionDealers
            .Include(s => s.PerfilDealer)
                .ThenInclude(p => p.Usuario)
            .Where(s => s.Estado == EstadoSuscripcion.Activa &&
                        s.Nivel != PlanNivel.Gratis &&
                        s.FechaVencimientoUtc > ahora &&
                        s.FechaVencimientoUtc <= fechaLimite &&
                        s.FechaRecordatorioEnviadoUtc == null)
            .ToListAsync(stoppingToken);

        if (porVencer.Count == 0)
        {
            _logger.LogInformation("Barrido de recordatorios completado. Ninguna suscripción requiere recordatorio hoy.");
            return;
        }

        var frontendUrl = configuration["App:FrontendUrl"];
        var rutaRenovar = string.IsNullOrWhiteSpace(frontendUrl)
            ? "/dashboard/suscripcion"
            : $"{frontendUrl.TrimEnd('/')}/dashboard/suscripcion";

        var enviados = 0;

        foreach (var suscripcion in porVencer)
        {
            var email = suscripcion.PerfilDealer?.Usuario?.Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning(
                    "No se pudo enviar recordatorio: suscripción {SuscripcionId} sin correo asociado.",
                    suscripcion.Id);
                continue;
            }

            try
            {
                var diasRestantes = Math.Max(0, (suscripcion.FechaVencimientoUtc.Date - ahora.Date).Days);
                var nombrePlan = NombrePlan(suscripcion.Nivel);
                var fechaVencimiento = suscripcion.FechaVencimientoUtc.ToString("dd/MM/yyyy");

                var asunto = $"Tu suscripción {nombrePlan} de AutoMarket RD vence pronto";

                var cuerpoHtml = $@"
                    <p>Hola <strong>{suscripcion.PerfilDealer?.Usuario?.Nombre}</strong>,</p>
                    <p>Tu suscripción <strong>{nombrePlan}</strong> vence el <strong>{fechaVencimiento}</strong>
                    (quedan {diasRestantes} día(s)).</p>
                    <p>Renueva a tiempo para que tus anuncios sigan visibles en la vitrina
                    sin ninguna interrupción.</p>
                    <p>
                        <a href='{rutaRenovar}' style='background-color:#3b82f6;color:#ffffff;
                        padding:12px 24px;text-decoration:none;border-radius:8px;'>Renovar ahora</a>
                    </p>";

                var cuerpoFinal = AutoMarket.Application.Helpers.PlantillaCorreoHelper.Envolver(
                    frontendUrl,
                    "Tu suscripción está por vencer",
                    cuerpoHtml);

                await emailSender.EnviarCorreoAsync(email, asunto, cuerpoFinal);

                suscripcion.MarcarRecordatorioEnviado();
                await dbContext.SaveChangesAsync();

                enviados++;
                _logger.LogInformation(
                    "Recordatorio de renovación enviado. SuscripcionId {SuscripcionId}, Email {Email}",
                    suscripcion.Id, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error enviando recordatorio de renovación. SuscripcionId {SuscripcionId}",
                    suscripcion.Id);
            }
        }

        _logger.LogInformation("Recordatorios de renovación enviados hoy: {Total}", enviados);
    }

    private static string NombrePlan(PlanNivel nivel)
    {
        return nivel switch
        {
            PlanNivel.Basico => "Básico",
            PlanNivel.Pro => "Pro",
            PlanNivel.Elite => "Elite",
            _ => nivel.ToString()
        };
    }
}