using AutoMarket.Application.DTOs.AdSlots;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Interfaces;

public interface IAdSlotService
{
    // Públicos
    Task<List<AdSlotPublicoDto>> ObtenerSlotsPublicosPorUbicacionAsync(UbicacionAdSlot ubicacion);
    Task RegistrarImpresionAsync(int anuncioId);
    Task RegistrarClickAsync(int anuncioId);

    // Dealer
    Task<List<AdSlotAnuncioPublicoDto>> ObtenerMisAnunciosAsync(int perfilDealerId);
    Task<AdSlotAnuncioPublicoDto> CrearAnuncioAsync(int perfilDealerId, CrearAdSlotAnuncioDto dto);
    Task CancelarAnuncioAsync(int perfilDealerId, int anuncioId);
    Task<AdSlotStatsDto> ObtenerEstadisticasAsync(int perfilDealerId);
    Task<List<AdSlotPublicoDto>> ObtenerSlotsDisponiblesAsync();

    // Admin
    Task<List<AdSlotDto>> ObtenerTodosLosSlotsAsync();
    Task<AdSlotDto> ObtenerSlotPorIdAsync(int id);
    Task<AdSlotDto> CrearSlotAsync(CrearAdSlotAdminDto dto);
    Task<AdSlotDto> ActualizarSlotAsync(int id, ActualizarAdSlotAdminDto dto);
    Task EliminarSlotAsync(int id);
    Task<List<AdSlotAnuncioAdminDto>> ObtenerTodosLosAnunciosAsync();
    Task<AdSlotAnuncioAdminDto> ObtenerAnuncioAdminPorIdAsync(int id);
    Task RechazarAnuncioAsync(int anuncioId);
    Task EliminarAnuncioAsync(int anuncioId);
    Task<List<AdSlotAnuncioAdminDto>> ObtenerAnunciosPorVencerAsync(int diasAntes);
}
