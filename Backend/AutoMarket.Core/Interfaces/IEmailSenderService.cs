namespace AutoMarket.Core.Interfaces;

public interface IEmailSenderService
{
    Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml);
}