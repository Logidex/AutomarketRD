using AutoMarket.Application.DTOs;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Contacto.
/// </summary>
public class ContactoService : IContactoService
{
    private readonly IEmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactoService> _logger;

/// <summary>
/// Inicializa una nueva instancia de la clase ContactoService.
/// </summary>
    public ContactoService(
        IEmailSenderService emailSender,
        IConfiguration configuration,
        ILogger<ContactoService> logger)
    {
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ProcesarMensajeContactoAsync(ContactoCreateDto dto)
    {
        var soporteEmail = _configuration["Soporte:Email"];

        if (string.IsNullOrWhiteSpace(soporteEmail))
        {
            _logger.LogError(
                "No se pudo procesar el mensaje de contacto porque falta Soporte:Email en la configuración.");

            return false;
        }

        var asunto = $"[Contacto AutoMarket] {dto.Asunto}";

        var cuerpo = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #0c101b;'>Nuevo mensaje de contacto</h2>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; font-weight: bold; width: 120px;'>Nombre:</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{System.Net.WebUtility.HtmlEncode(dto.Nombre)}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;'>Email:</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{System.Net.WebUtility.HtmlEncode(dto.Email)}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; font-weight: bold;'>Asunto:</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{System.Net.WebUtility.HtmlEncode(dto.Asunto)}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; font-weight: bold; vertical-align: top;'>Mensaje:</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; white-space: pre-wrap;'>{System.Net.WebUtility.HtmlEncode(dto.Mensaje)}</td>
                    </tr>
                </table>
                <p style='margin-top: 24px; font-size: 12px; color: #999;'>
                    Este mensaje fue enviado desde el formulario de contacto de AutoMarket RD.
                </p>
            </body>
            </html>";

        await _emailSender.EnviarCorreoAsync(soporteEmail, asunto, cuerpo);

        _logger.LogInformation(
            "Mensaje de contacto procesado. De: {Email}, Asunto: {Asunto}",
            dto.Email, dto.Asunto);

        return true;
    }
}

