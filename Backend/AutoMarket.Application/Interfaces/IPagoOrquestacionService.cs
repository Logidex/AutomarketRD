using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Core.Entities.Enums;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Application.Interfaces;

public interface IPagoOrquestacionService
{
    Task<(string Url, decimal MontoUsd)> GenerarLinkDePagoAsync(int usuarioId, CrearOrdenDto request);
    Task<ConfirmarPagoResult> ConfirmarPagoAsync(int usuarioId, ConfirmarPagoDto dto);
    Task<WebhookResult> ProcesarWebhookAsync(string jsonBody, Dictionary<string, string> headers);
    Task<TransferenciaResult> RegistrarTransferenciaAsync(int usuarioId, string nombrePlan, string ciclo, IFormFile imagen);
}

public class ConfirmarPagoResult
{
    public bool Exito { get; set; }
    public bool YaProcesado { get; set; }
    public string? Mensaje { get; set; }
}

public class WebhookResult
{
    public int StatusCode { get; set; }
    public string? Mensaje { get; set; }
}

public class TransferenciaResult
{
    public bool Exito { get; set; }
    public int? PagoId { get; set; }
    public string? Mensaje { get; set; }
}
