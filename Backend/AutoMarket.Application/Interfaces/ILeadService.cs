using AutoMarket.Application.DTOs;
using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ILeadService
{
    // Método principal para el comprador
    Task CrearLeadAsync(LeadCreateDto dto);

    // Métodos de consulta para el Dashboard del Dealer
    Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorAnuncioAsync(int anuncioId);
    Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorDealerAsync(int dealerId);
}