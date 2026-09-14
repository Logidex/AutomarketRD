using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Infrastructure.Services;

/// <summary>
/// Wrapper sobre IEmailSenderService que captura fallos y los guarda
/// en la cola EmailJobs para reintentos automáticos por EmailRetryService.
/// </summary>
public class ResilientEmailSender : IEmailSenderService
{
    private readonly IEmailSenderService _inner;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<ResilientEmailSender> _logger;

    public ResilientEmailSender(
        IEmailSenderService inner,
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogger<ResilientEmailSender> logger)
    {
        _inner = inner;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, string? replyTo = null)
    {
        try
        {
            await _inner.EnviarCorreoAsync(destinatario, asunto, cuerpoHtml, replyTo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo al enviar correo a {Destinatario}, guardando en cola de reintentos", destinatario);

            await using var db = await _dbFactory.CreateDbContextAsync();

            db.EmailJobs.Add(new EmailJob
            {
                Destinatario = destinatario,
                Asunto = asunto,
                CuerpoHtml = cuerpoHtml,
                Intentos = 1,
                MaxIntentos = 3,
                UltimoError = ex.Message,
                ProximoReintentoUtc = DateTime.UtcNow.AddSeconds(30),
                FechaCreacionUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
