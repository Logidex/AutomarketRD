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

    public async Task CambiarPlanAsync(int perfilDealerId, PlanNivel nuevoNivel, CicloFacturacion ciclo)
    {
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcion == null)
            throw new KeyNotFoundException("No se encontró una suscripción activa para este dealer.");

        if (suscripcion.Nivel == nuevoNivel && suscripcion.Ciclo == ciclo)
            throw new BusinessRuleException(
                "El dealer ya se encuentra suscrito a este plan con ese mismo ciclo.");

        if (suscripcion.Estado == EstadoSuscripcion.Cancelada)
            throw new BusinessRuleException(
                "La suscripción está cancelada. Debe adquirir una nueva en lugar de cambiar de plan.");

        suscripcion.CambiarPlan(nuevoNivel, ciclo);

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

    public async Task ProcesarPagoSuscripcionAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo)
    {
        var suscripcionExistente = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcionExistente == null)
        {
            var nuevaSuscripcion = new SuscripcionDealer(perfilDealerId, nivel, ciclo);
            await _repository.AgregarAsync(nuevaSuscripcion);
            return;
        }

        if (suscripcionExistente.Estado == EstadoSuscripcion.Cancelada)
            throw new BusinessRuleException("La suscripción está cancelada. Debe definirse una política de reactivación.");

        if (suscripcionExistente.Nivel == nivel && suscripcionExistente.Ciclo == ciclo)
            throw new BusinessRuleException("El dealer ya posee ese mismo plan y ciclo.");

        suscripcionExistente.CambiarPlan(nivel, ciclo);
        await _repository.ActualizarAsync(suscripcionExistente);
    }
}