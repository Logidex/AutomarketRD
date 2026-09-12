using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Interfaces;

public interface IAdSlotRepository
{
    // AdSlots
    Task<AdSlot?> ObtenerSlotPorIdAsync(int id);
    Task<List<AdSlot>> ObtenerSlotsActivosAsync();
    Task<List<AdSlot>> ObtenerSlotsPorUbicacionAsync(UbicacionAdSlot ubicacion);
    Task<List<AdSlot>> ObtenerTodosLosSlotsAsync();
    Task AgregarSlotAsync(AdSlot slot);
    Task ActualizarSlotAsync(AdSlot slot);
    Task EliminarSlotAsync(int id);

    // AdSlotPrecios
    Task<List<AdSlotPrecio>> ObtenerPreciosPorSlotAsync(int adSlotId);
    Task AgregarPrecioAsync(AdSlotPrecio precio);
    Task ActualizarPrecioAsync(AdSlotPrecio precio);
    Task EliminarPrecioAsync(int precioId);

    // AdSlotAnuncios
    Task<AdSlotAnuncio?> ObtenerAnuncioPorIdAsync(int id);
    Task<List<AdSlotAnuncio>> ObtenerAnunciosActivosPorSlotAsync(int adSlotId);
    Task<List<AdSlotAnuncio>> ObtenerAnunciosActivosPorUbicacionAsync(UbicacionAdSlot ubicacion);
    Task<List<AdSlotAnuncio>> ObtenerAnunciosPorDealerAsync(int perfilDealerId);
    Task<List<AdSlotAnuncio>> ObtenerAnunciosVencidosAsync();
    Task<List<AdSlotAnuncio>> ObtenerAnunciosPorVencerAsync(int diasAntes);
    Task AgregarAnuncioAsync(AdSlotAnuncio anuncio);
    Task ActualizarAnuncioAsync(AdSlotAnuncio anuncio);
    Task EliminarAnuncioAsync(int anuncioId);
    Task<bool> ExisteCapturaTransferenciaAdSlotAsync(string clave);
}
