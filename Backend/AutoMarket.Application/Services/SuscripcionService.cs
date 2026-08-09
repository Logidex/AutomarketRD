using AutoMarket.Application.DTOs.Suscripcion;
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
        {
            suscripcionExistente.ActivarConPlan(nivel, ciclo);

            await _repository.ActualizarAsync(suscripcionExistente);
            return;
        }

        if (suscripcionExistente.Nivel == nivel && suscripcionExistente.Ciclo == ciclo)
        {
            var nuevaFechaVencimiento = CalcularNuevaVigenciaDesdePago(suscripcionExistente, ciclo);
            suscripcionExistente.RenovarManualmente(nuevaFechaVencimiento);

            await _repository.ActualizarAsync(suscripcionExistente);
            return;
        }

        suscripcionExistente.CambiarPlan(nivel, ciclo);
        await _repository.ActualizarAsync(suscripcionExistente);
    }

    public async Task RegistrarPagoAsync(
        int perfilDealerId,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        decimal monto,
        string moneda,
        string? orderIdPayPal,
        string? eventoIdPayPal,
        string? referencia)
    {
        var pago = new PagoSuscripcion(
            perfilDealerId,
            nivel,
            ciclo,
            monto,
            moneda,
            orderIdPayPal,
            eventoIdPayPal,
            referencia);

        await _repository.AgregarPagoAsync(pago);
    }

    public async Task<IReadOnlyList<PagoSuscripcionDto>> ObtenerHistorialPagosAsync(int perfilDealerId)
    {
        var pagos = await _repository.ObtenerHistorialPagosAsync(perfilDealerId);

        return pagos
            .Select(p => new PagoSuscripcionDto
            {
                Id = p.Id,
                PerfilDealerId = p.PerfilDealerId,
                Nivel = p.Nivel,
                Ciclo = p.Ciclo,
                Estado = p.Estado,
                Monto = p.Monto,
                Moneda = p.Moneda,
                OrdenIdPayPal = p.OrderIdPayPal,
                Referencia = p.Referencia,
                FechaUtc = p.FechaUtc
            })
            .ToList();
    }

    public async Task<SuscripcionDealerDto?> ObtenerSuscripcionAsync(int perfilDealerId)
    {
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcion == null)
            return null;

        return new SuscripcionDealerDto
        {
            PerfilDealerId = suscripcion.PerfilDealerId,
            Nivel = suscripcion.Nivel,
            Ciclo = suscripcion.Ciclo,
            Estado = suscripcion.Estado,
            LimiteAnuncios = suscripcion.LimiteAnuncios,
            FechaInicioUtc = suscripcion.FechaInicioUtc,
            FechaVencimientoUtc = suscripcion.FechaVencimientoUtc,
            DiasRestantes = Math.Max(0, (int)Math.Ceiling((suscripcion.FechaVencimientoUtc - DateTime.UtcNow).TotalDays)),
            Activa = suscripcion.FechaVencimientoUtc > DateTime.UtcNow
        };
    }

    public async Task CancelarSuscripcionAsync(int perfilDealerId)
    {
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcion == null)
            throw new KeyNotFoundException("No se encontró una suscripción para este dealer.");

        if (suscripcion.Estado == EstadoSuscripcion.Cancelada)
            throw new BusinessRuleException("La suscripción ya se encuentra cancelada.");

        suscripcion.Cancelar();

        await _repository.ActualizarAsync(suscripcion);
    }

    public async Task<bool> ExistePagoPorEventoAsync(string eventoId)
    {
        return await _repository.ExistePagoPorEventoAsync(eventoId);
    }

    public async Task<bool> ExistePagoPorOrdenAsync(string orderId)
    {
        return await _repository.ExistePagoPorOrdenAsync(orderId);
    }

    private static DateTime CalcularNuevaVigenciaDesdePago(SuscripcionDealer suscripcion, CicloFacturacion ciclo)
    {
        var ahora = DateTime.UtcNow;

        var baseFecha = suscripcion.FechaVencimientoUtc > ahora
            ? suscripcion.FechaVencimientoUtc
            : ahora;

        return ciclo switch
        {
            CicloFacturacion.Mensual => baseFecha.AddMonths(1),
            CicloFacturacion.Trimestral => baseFecha.AddMonths(3),
            CicloFacturacion.Anual => baseFecha.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(ciclo), "Ciclo de facturación no válido.")
        };
    }
}