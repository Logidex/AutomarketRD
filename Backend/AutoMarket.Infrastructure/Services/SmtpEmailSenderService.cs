using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Infrastructure.Services;

public class SmtpEmailSenderService : IEmailSenderService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSenderService(IConfiguration configuration)
    {
        _configuration = configuration; // Para leer el appsettings.json
    }

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var host = _configuration["SmtpSettings:Host"];
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var user = _configuration["SmtpSettings:User"];
        var password = _configuration["SmtpSettings:Password"];
        var senderName = _configuration["SmtpSettings:SenderName"];

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(user, password)
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(user!, senderName),
            Subject = asunto,
            Body = cuerpoHtml,
            IsBodyHtml = true // Permite enviar correos con diseño en lugar de texto plano
        };

        mailMessage.To.Add(destinatario);

        await client.SendMailAsync(mailMessage);
    }
}