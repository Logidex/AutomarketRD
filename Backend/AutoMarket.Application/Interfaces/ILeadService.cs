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

    // Resumen de notificaciones para el campanario del dealer/vendedor
    Task<LeadNoLeidosResumenDto> ObtenerResumenNoLeidosAsync(int usuarioId);

    // Historial del comprador: vehículos que contactó
    Task<IReadOnlyCollection<LeadContactoUsuarioDto>> ObtenerMisContactosAsync(int usuarioId);

    // Marcar un lead como leído (el dealer lo atendió)
    Task<bool> MarcarLeidoAsync(int leadId, int usuarioId);

    // Marcar todos los leads del dealer como leídos de una vez
    Task<int> MarcarTodosLeidosAsync(int usuarioId);
}