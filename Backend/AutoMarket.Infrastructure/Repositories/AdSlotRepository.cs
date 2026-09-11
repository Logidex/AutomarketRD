using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class AdSlotRepository : IAdSlotRepository
{
    private readonly ApplicationDbContext _context;

    public AdSlotRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // AD SLOTS
    // ==========================================

    public async Task<AdSlot?> ObtenerSlotPorIdAsync(int id)
    {
        return await _context.AdSlots
            .Include(s => s.Precios)
            .Include(s => s.Anuncios)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<AdSlot>> ObtenerSlotsActivosAsync()
    {
        return await _context.AdSlots
            .Include(s => s.Precios)
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ToListAsync();
    }

    public async Task<List<AdSlot>> ObtenerSlotsPorUbicacionAsync(UbicacionAdSlot ubicacion)
    {
        return await _context.AdSlots
            .Include(s => s.Precios)
            .Include(s => s.Anuncios)
                .ThenInclude(a => a.PerfilDealer)
            .Where(s => s.Activo && s.Ubicacion == ubicacion)
            .OrderBy(s => s.Orden)
            .ToListAsync();
    }

    public async Task<List<AdSlot>> ObtenerTodosLosSlotsAsync()
    {
        return await _context.AdSlots
            .Include(s => s.Precios)
            .Include(s => s.Anuncios)
            .OrderBy(s => s.Orden)
            .ToListAsync();
    }

    public async Task AgregarSlotAsync(AdSlot slot)
    {
        await _context.AdSlots.AddAsync(slot);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarSlotAsync(AdSlot slot)
    {
        _context.AdSlots.Update(slot);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarSlotAsync(int id)
    {
        var slot = await _context.AdSlots.FindAsync(id);
        if (slot != null)
        {
            slot.Activo = false;
            await _context.SaveChangesAsync();
        }
    }

    // ==========================================
    // AD SLOT PRECIOS
    // ==========================================

    public async Task<List<AdSlotPrecio>> ObtenerPreciosPorSlotAsync(int adSlotId)
    {
        return await _context.AdSlotsPrecios
            .Where(p => p.AdSlotId == adSlotId)
            .OrderBy(p => p.DuracionDias)
            .ToListAsync();
    }

    public async Task AgregarPrecioAsync(AdSlotPrecio precio)
    {
        await _context.AdSlotsPrecios.AddAsync(precio);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarPrecioAsync(AdSlotPrecio precio)
    {
        _context.AdSlotsPrecios.Update(precio);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarPrecioAsync(int precioId)
    {
        var precio = await _context.AdSlotsPrecios.FindAsync(precioId);
        if (precio != null)
        {
            precio.Activo = false;
            await _context.SaveChangesAsync();
        }
    }

    // ==========================================
    // AD SLOT ANUNCIOS
    // ==========================================

    public async Task<AdSlotAnuncio?> ObtenerAnuncioPorIdAsync(int id)
    {
        return await _context.AdSlotsAnuncios
            .Include(a => a.AdSlot)
            .Include(a => a.PerfilDealer)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AdSlotAnuncio>> ObtenerAnunciosActivosPorSlotAsync(int adSlotId)
    {
        return await _context.AdSlotsAnuncios
            .Include(a => a.PerfilDealer)
            .Where(a => a.AdSlotId == adSlotId && a.Estado == EstadoAdSlot.Activo)
            .OrderByDescending(a => a.Prioridad)
            .ToListAsync();
    }

    public async Task<List<AdSlotAnuncio>> ObtenerAnunciosActivosPorUbicacionAsync(UbicacionAdSlot ubicacion)
    {
        return await _context.AdSlotsAnuncios
            .Include(a => a.AdSlot)
            .Include(a => a.PerfilDealer)
            .Where(a => a.Estado == EstadoAdSlot.Activo && a.AdSlot.Ubicacion == ubicacion)
            .OrderByDescending(a => a.Prioridad)
            .ToListAsync();
    }

    public async Task<List<AdSlotAnuncio>> ObtenerAnunciosPorDealerAsync(int perfilDealerId)
    {
        return await _context.AdSlotsAnuncios
            .Include(a => a.AdSlot)
            .Include(a => a.PerfilDealer)
            .Where(a => a.PerfilDealerId == perfilDealerId)
            .OrderByDescending(a => a.FechaCreacionUtc)
            .ToListAsync();
    }

    public async Task<List<AdSlotAnuncio>> ObtenerAnunciosVencidosAsync()
    {
        return await _context.AdSlotsAnuncios
            .Include(a => a.AdSlot)
            .Where(a => a.Estado == EstadoAdSlot.Activo && a.FechaFinUtc < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<List<AdSlotAnuncio>> ObtenerAnunciosPorVencerAsync(int diasAntes)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(diasAntes);
        return await _context.AdSlotsAnuncios
            .Include(a => a.AdSlot)
            .Include(a => a.PerfilDealer)
            .Where(a => a.Estado == EstadoAdSlot.Activo
                        && a.FechaFinUtc <= fechaLimite
                        && a.FechaFinUtc > DateTime.UtcNow
                        && a.FechaRecordatorioEnviadoUtc == null)
            .ToListAsync();
    }

    public async Task AgregarAnuncioAsync(AdSlotAnuncio anuncio)
    {
        await _context.AdSlotsAnuncios.AddAsync(anuncio);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAnuncioAsync(AdSlotAnuncio anuncio)
    {
        _context.AdSlotsAnuncios.Update(anuncio);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteCapturaTransferenciaAdSlotAsync(string clave)
    {
        return await _context.AdSlotsAnuncios
            .AnyAsync(a => a.UrlCapturaTransferencia != null &&
                           (a.UrlCapturaTransferencia == clave ||
                            a.UrlCapturaTransferencia.Contains(clave)));
    }
}
