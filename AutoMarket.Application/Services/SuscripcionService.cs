using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class SuscripcionService : ISuscripcionService
{
    private readonly ISuscripcionRepository _repository;

    public SuscripcionService(ISuscripcionRepository repository)
    {
        _repository = repository;
    }

    public async Task AsignarPlanInicialAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo)
    {
        // Verificamos que no tenga una suscripción previa para evitar duplicados
        var suscripcionExistente = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);
        if (suscripcionExistente != null)
            throw new BusinessRuleException("El dealer ya posee una suscripción registrada.");

        var nuevaSuscripcion = new SuscripcionDealer(perfilDealerId, nivel, ciclo);
        
        await _repository.AgregarAsync(nuevaSuscripcion);
    }

    public async Task CambiarPlanAsync(int perfilDealerId, PlanNivel nuevoNivel)
    {
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        // Regla 1: El Fantasma (Debe existir)
        if (suscripcion == null)
            throw new KeyNotFoundException("No se encontró una suscripción activa para este dealer.");

        // Regla 2: El Cobro Doble (No puede comprar lo que ya tiene)
        if (suscripcion.Nivel == nuevoNivel)
            throw new BusinessRuleException("El dealer ya se encuentra suscrito a este plan. No se requiere actualización.");

        // Regla 3: El Moroso (Debe reactivar, no solo cambiar de plan)
        if (suscripcion.Estado == EstadoSuscripcion.Cancelada)
            throw new BusinessRuleException("La suscripción está cancelada. Debe adquirir una nueva en lugar de cambiar de plan.");

        // ==========================================
        // LA MUTACIÓN SEGURA
        // ==========================================
        suscripcion.ActualizarNivel(nuevoNivel);
        
        await _repository.ActualizarAsync(suscripcion);
    }

    public async Task RenovarManualAsync(int perfilDealerId, DateTime nuevaFechaVencimiento)
    {
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcion == null)
            throw new KeyNotFoundException("No se encontró una suscripción para este dealer.");

        suscripcion.RenovarManualmente(nuevaFechaVencimiento);
        
        await _repository.ActualizarAsync(suscripcion);
    }
}