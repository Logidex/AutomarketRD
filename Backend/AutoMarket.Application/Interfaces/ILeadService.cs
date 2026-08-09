using AutoMarket.Application.DTOs;
using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ILeadService
{
    // Método principal para el comprador
    Task CrearLeadAsync(LeadCreateDto dto);

    // Métodos de consulta para el Dashboard del Dealer
    Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorAnuncioAsync(int anuncioId, int usuarioId);
    Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorDealerAsync(int dealerId);

    // Marcar un lead como leído (el dealer lo atendió)
    Task<bool> MarcarLeidoAsync(int leadId, int usuarioId);
}