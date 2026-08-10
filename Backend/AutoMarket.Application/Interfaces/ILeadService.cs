using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Lead;
using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ILeadService
{
    // Método principal para el comprador
    // usuarioIdRemitente: identidad autenticada del que envía (null si es anónimo).
    Task CrearLeadAsync(LeadCreateDto dto, int? usuarioIdRemitente = null);

    // Métodos de consulta para el Dashboard del Dealer
    Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorAnuncioAsync(int anuncioId, int usuarioId);
    Task<IReadOnlyCollection<LeadDealerDto>> ObtenerLeadsPorDealerAsync(int dealerId);

    // Marcar un lead como leído (el dealer lo atendió)
    Task<bool> MarcarLeidoAsync(int leadId, int usuarioId);
}