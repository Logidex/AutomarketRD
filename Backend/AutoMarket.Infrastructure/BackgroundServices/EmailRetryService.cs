using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Background service que procesa la cola de correos pendientes.
/// Reintenta correos fallidos con backoff exponencial.
/// </summary>
public class EmailRetryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailRetryService> _logger;

    public EmailRetryService(IServiceProvider serviceProvider, ILogger<EmailRetryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailRetryService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarJobsPendientesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando cola de correos");
            }

            // Verificar cada 30 segundos
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcesarJobsPendientesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var ahora = DateTime.UtcNow;

        var jobsPendientes = await db.EmailJobs
            .Where(j => j.Estado == "Pendiente"
                && j.Intentos < j.MaxIntentos
                && (j.ProximoReintentoUtc == null || j.ProximoReintentoUtc <= ahora))
            .OrderBy(j => j.FechaCreacionUtc)
            .Take(10)
            .ToListAsync(ct);

        foreach (var job in jobsPendientes)
        {
            try
            {
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();

                await emailSender.EnviarCorreoAsync(
                    job.Destinatario,
                    job.Asunto,
                    job.CuerpoHtml);

                job.Estado = "Enviado";
                _logger.LogInformation("Correo enviado a {Destinatario} (job {JobId})", job.Destinatario, job.Id);
            }
            catch (Exception ex)
            {
                job.Intentos++;
                job.UltimoError = ex.Message;

                if (job.Intentos >= job.MaxIntentos)
                {
                    job.Estado = "Fallido";
                    _logger.LogWarning("Correo fallido definitivamente after {Intentos} intentos: {Error}",
                        job.Intentos, ex.Message);
                }
                else
                {
                    // Backoff exponencial: 30s, 2m, 8m
                    var delay = job.Intentos switch
                    {
                        1 => TimeSpan.FromSeconds(30),
                        2 => TimeSpan.FromMinutes(2),
                        _ => TimeSpan.FromMinutes(8)
                    };
                    job.ProximoReintentoUtc = ahora.Add(delay);
                    _logger.LogWarning("Reintento {Intentos}/{Max} para {Destinatario} en {Delay}",
                        job.Intentos, job.MaxIntentos, job.Destinatario, delay);
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
