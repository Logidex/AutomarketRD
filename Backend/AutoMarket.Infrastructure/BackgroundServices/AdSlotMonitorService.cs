using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que monitorea los anuncios publicitarios.
/// Ejecuta tres tareas cada 24 horas:
/// 1. Desactiva anuncios vencidos
/// 2. Envía recordatorios de vencimiento a dealers
/// 3. Activa anuncios cuya fecha de inicio ya comenzó (si estaban pendientes)
/// </summary>
public class AdSlotMonitorService : BackgroundService
{
    private readonly ILogger<AdSlotMonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AdSlotMonitorService(
        ILogger<AdSlotMonitorService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AdSlotMonitorService ha iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DesactivarAnunciosVencidosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar anuncios vencidos.");
            }

            try
            {
                await EnviarRecordatoriosDeVencimientoAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar recordatorios de vencimiento de anuncios.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    /// <summary>
    /// Desactiva anuncios cuya FechaFinUtc ya pasó.
    /// </summary>
    private async Task DesactivarAnunciosVencidosAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var ahora = DateTime.UtcNow;

        _logger.LogInformation("Ejecutando barrido de anuncios vencidos.");

        var anunciosVencidos = await dbContext.AdSlotsAnuncios
            .Where(a => a.Estado == EstadoAdSlot.Activo && a.FechaFinUtc < ahora)
            .ToListAsync(stoppingToken);

        if (anunciosVencidos.Count == 0)
        {
            _logger.LogInformation("No hay anuncios vencidos para desactivar.");
            return;
        }

        foreach (var anuncio in anunciosVencidos)
        {
            anuncio.Estado = EstadoAdSlot.Vencido;
        }

        await dbContext.SaveChangesAsync(stoppingToken);

        _logger.LogInformation("Se desactivaron {Cantidad} anuncios vencidos.", anunciosVencidos.Count);
    }

    /// <summary>
    /// Envía recordatorios a dealers cuyos anuncios vencen en los próximos N días.
    /// </summary>
    private async Task EnviarRecordatoriosDeVencimientoAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var diasAntes = configuration.GetValue<int?>("Recordatorios:DiasAntesRenovacionAdSlot") ?? 3;

        if (diasAntes <= 0)
        {
            _logger.LogInformation("Recordatorios de vencimiento de anuncios desactivados.");
            return;
        }

        var ahora = DateTime.UtcNow;
        var fechaLimite = ahora.AddDays(diasAntes);

        _logger.LogInformation(
            "Buscando anuncios que vencen dentro de {Dias} días para enviar recordatorio.",
            diasAntes);

        var porVencer = await dbContext.AdSlotsAnuncios
            .Include(a => a.AdSlot)
            .Include(a => a.PerfilDealer)
                .ThenInclude(p => p.Usuario)
            .Where(a => a.Estado == EstadoAdSlot.Activo
                        && a.FechaFinUtc > ahora
                        && a.FechaFinUtc <= fechaLimite
                        && a.FechaRecordatorioEnviadoUtc == null)
            .ToListAsync(stoppingToken);

        if (porVencer.Count == 0)
        {
            _logger.LogInformation("No hay anuncios por vencer que requieran recordatorio.");
            return;
        }

        var frontendUrl = configuration["App:FrontendUrl"];
        var rutaMisAds = string.IsNullOrWhiteSpace(frontendUrl)
            ? "/dashboard/ads"
            : $"{frontendUrl.TrimEnd('/')}/dashboard/ads";

        var enviados = 0;

        foreach (var anuncio in porVencer)
        {
            var email = anuncio.PerfilDealer?.Usuario?.Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning(
                    "No se pudo enviar recordatorio: anuncio {AnuncioId} sin correo asociado.",
                    anuncio.Id);
                continue;
            }

            try
            {
                var diasRestantes = Math.Max(0, (anuncio.FechaFinUtc.Date - ahora.Date).Days);
                var ubicacion = NombreUbicacion(anuncio.AdSlot?.Ubicacion ?? UbicacionAdSlot.HomepageLateral);
                var fechaVencimiento = anuncio.FechaFinUtc.ToString("dd/MM/yyyy");

                var asunto = $"Tu anuncio publicitario en \"{ubicacion}\" vence pronto";

                var cuerpoHtml = $@"
                    <p>Hola <strong>{AutoMarket.Application.Helpers.PlantillaCorreoHelper.EscaparHtml(anuncio.PerfilDealer?.Usuario?.Nombre)}</strong>,</p>
                    <p>Tu anuncio publicitario en <strong>{ubicacion}</strong> vence el
                    <strong>{fechaVencimiento}</strong> (quedan {diasRestantes} día(s)).</p>
                    <p>Renueva a tiempo para que tu negocio siga apareciendo en la vitrina
                    de AutoMarket RD.</p>
                    <p>
                        <a href='{rutaMisAds}' style='background-color:#3b82f6;color:#ffffff;
                        padding:12px 24px;text-decoration:none;border-radius:8px;
                        display:inline-block;'>Renovar anuncio</a>
                    </p>";

                var cuerpoFinal = AutoMarket.Application.Helpers.PlantillaCorreoHelper.Envolver(
                    frontendUrl,
                    "Tu anuncio publicitario está por vencer",
                    cuerpoHtml);

                await emailSender.EnviarCorreoAsync(email, asunto, cuerpoFinal);

                anuncio.MarcarRecordatorioEnviado();
                await dbContext.SaveChangesAsync(stoppingToken);

                enviados++;
                _logger.LogInformation(
                    "Recordatorio de vencimiento enviado. AnuncioId {AnuncioId}, Email {Email}",
                    anuncio.Id, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error enviando recordatorio de vencimiento. AnuncioId {AnuncioId}",
                    anuncio.Id);
            }
        }

        _logger.LogInformation("Recordatorios de vencimiento de anuncios enviados: {Total}", enviados);
    }

    private static string NombreUbicacion(UbicacionAdSlot ubicacion)
    {
        return ubicacion switch
        {
            UbicacionAdSlot.HomepageLateral => "Homepage - Lateral",
            UbicacionAdSlot.HomepageBuscador => "Homepage - Buscador",
            UbicacionAdSlot.HomepageFooter => "Homepage - Footer",
            UbicacionAdSlot.VehiculosLateral => "Vehículos - Lateral",
            UbicacionAdSlot.VehiculosGrid => "Vehículos - Grid",
            UbicacionAdSlot.VehiculosFooter => "Vehículos - Footer",
            UbicacionAdSlot.DetalleLateral => "Detalle de Anuncio - Lateral",
            UbicacionAdSlot.DetalleFooter => "Detalle de Anuncio - Footer",
            UbicacionAdSlot.AgenciasLateral => "Agencias - Lateral",
            UbicacionAdSlot.AgenciasFooter => "Agencias - Footer",
            _ => ubicacion.ToString()
        };
    }
}
