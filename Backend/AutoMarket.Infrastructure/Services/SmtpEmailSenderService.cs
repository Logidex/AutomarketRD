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

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, string? replyTo = null)
    {
        var host = _configuration["SmtpSettings:Host"];
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var user = _configuration["SmtpSettings:User"];
        var password = _configuration["SmtpSettings:Password"];
        var senderName = _configuration["SmtpSettings:SenderName"];
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? user!;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(user, password)
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = asunto,
            Body = cuerpoHtml,
            IsBodyHtml = true
        };

        mailMessage.To.Add(destinatario);

        if (!string.IsNullOrWhiteSpace(replyTo))
            mailMessage.ReplyToList.Add(new MailAddress(replyTo));

        await client.SendMailAsync(mailMessage);
    }
}