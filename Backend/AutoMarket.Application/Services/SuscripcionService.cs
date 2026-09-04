using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Suscripcion;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Suscripcion.
/// </summary>
public class SuscripcionService : ISuscripcionService
{
    private readonly ISuscripcionRepository _repository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IPayPalService _payPalService;
    private readonly IPlanCatalogoRepository _planCatalogoRepository;

/// <summary>
/// Inicializa una nueva instancia de la clase SuscripcionService.
/// </summary>
    public SuscripcionService(
        ISuscripcionRepository repository,
        IAnuncioRepository anuncioRepository,
        IPayPalService payPalService,
        IPlanCatalogoRepository planCatalogoRepository)
    {
        _repository = repository;
        _anuncioRepository = anuncioRepository;
        _payPalService = payPalService;
        _planCatalogoRepository = planCatalogoRepository;
    }

/// <summary>
/// AsignarPlanInicialAsync Asignar plan inicial async. Parámetros: Parámetro perfilDealerId (int), Parámetro nivel (PlanNivel), Parámetro ciclo (CicloFacturacion). Retorna: Task.
/// </summary>
    public async Task AsignarPlanInicialAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo)
    {
        // Verificamos que no tenga una suscripción previa para evitar duplicados
        var suscripcionExistente = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);
        if (suscripcionExistente != null)
            throw new BusinessRuleException("El dealer ya posee una suscripción registrada.");

        var nuevaSuscripcion = new SuscripcionDealer(perfilDealerId, nivel, ciclo);

        nuevaSuscripcion.VincularPlanCatalogo(
            await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));

        await _repository.AgregarAsync(nuevaSuscripcion);
    }

/// <summary>
/// CambiarPlanAsync Cambiar plan async. Parámetros: Parámetro perfilDealerId (int), Parámetro nuevoNivel (PlanNivel), Parámetro ciclo (CicloFacturacion). Retorna: Task.
/// </summary>
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

        await ValidarInventarioContraNuevoPlanAsync(perfilDealerId, nuevoNivel);

        suscripcion.CambiarPlan(nuevoNivel, ciclo);

        suscripcion.VincularPlanCatalogo(
            await _planCatalogoRepository.ObtenerPorNivelAsync(nuevoNivel));

        await _repository.ActualizarAsync(suscripcion);
    }

/// <summary>
/// RenovarManualAsync Renovar manual async. Parámetros: Parámetro perfilDealerId (int), Parámetro nuevaFechaVencimiento (DateTime). Retorna: Task.
/// </summary>
    public async Task RenovarManualAsync(int perfilDealerId, DateTime nuevaFechaVencimiento)
    {
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcion == null)
            throw new KeyNotFoundException("No se encontró una suscripción para este dealer.");

        suscripcion.RenovarManualmente(nuevaFechaVencimiento);

        suscripcion.VincularPlanCatalogo(
            await _planCatalogoRepository.ObtenerPorNivelAsync(suscripcion.Nivel));

        await _repository.ActualizarAsync(suscripcion);
    }

/// <summary>
/// ProcesarPagoSuscripcionAsync Procesar pago suscripcion async. Parámetros: Parámetro perfilDealerId (int), Parámetro nivel (PlanNivel), Parámetro ciclo (CicloFacturacion). Retorna: Task.
/// </summary>
    public async Task ProcesarPagoSuscripcionAsync(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo)
    {
        var suscripcionExistente = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcionExistente == null)
        {
            await ValidarInventarioContraNuevoPlanAsync(perfilDealerId, nivel);

            var nuevaSuscripcion = new SuscripcionDealer(perfilDealerId, nivel, ciclo);
            nuevaSuscripcion.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));
            await _repository.AgregarAsync(nuevaSuscripcion);
            return;
        }

        if (suscripcionExistente.Estado == EstadoSuscripcion.Cancelada)
        {
            await ValidarInventarioContraNuevoPlanAsync(perfilDealerId, nivel);

            suscripcionExistente.ActivarConPlan(nivel, ciclo);

            suscripcionExistente.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));

            await _repository.ActualizarAsync(suscripcionExistente);
            return;
        }

        if (suscripcionExistente.Nivel == nivel && suscripcionExistente.Ciclo == ciclo)
        {
            var nuevaFechaVencimiento = CalcularNuevaVigenciaDesdePago(suscripcionExistente, ciclo);
            suscripcionExistente.RenovarManualmente(nuevaFechaVencimiento);

            suscripcionExistente.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));

            await _repository.ActualizarAsync(suscripcionExistente);
            return;
        }

        await ValidarInventarioContraNuevoPlanAsync(perfilDealerId, nivel);

        suscripcionExistente.CambiarPlan(nivel, ciclo);

        suscripcionExistente.VincularPlanCatalogo(
            await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));

        await _repository.ActualizarAsync(suscripcionExistente);
    }

    /// <summary>
    /// Impide bajar de plan (o reactivar uno menor) si el dealer mantiene más anuncios
    /// activos en la vitrina de los que permite el nuevo plan.
    /// </summary>
    private async Task ValidarInventarioContraNuevoPlanAsync(int perfilDealerId, PlanNivel nuevoNivel)
    {
        var limite = (int)nuevoNivel;
        var anunciosActivos = await _anuncioRepository.ContarAnunciosPorUsuarioAsync(perfilDealerId);

        if (anunciosActivos > limite)
        {
            throw new BusinessRuleException(
                $"No puedes bajar a este plan: tienes {anunciosActivos} anuncios activos y el plan " +
                $"{nuevoNivel} permite hasta {limite}. Pausa o elimina el excedente antes de continuar."
            );
        }
    }

/// <summary>
/// RegistrarPagoAsync Registrar pago async.. Retorna: Task.
/// </summary>
    public async Task RegistrarPagoAsync(
        int perfilDealerId,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        decimal monto,
        string moneda,
        string? orderIdPayPal,
        string? eventoIdPayPal,
        string? captureIdPayPal,
        string? referencia)
    {
        var pago = new PagoSuscripcion(
            perfilDealerId,
            nivel,
            ciclo,
            monto,
            moneda,
            MetodoPago.PayPal,
            orderIdPayPal,
            eventoIdPayPal,
            captureIdPayPal,
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
                CaptureIdPayPal = p.CaptureIdPayPal,
                Referencia = p.Referencia,
                Metodo = p.Metodo,
                EstadoTransferencia = p.EstadoTransferencia,
                MotivoRechazo = p.EstadoTransferencia == Core.Entities.Enums.EstadoTransferencia.Rechazada
                    ? p.NotasAdmin
                    : null,
                FechaUtc = p.FechaUtc
            })
            .ToList();
    }

    public async Task<IReadOnlyList<PagoAdminDto>> ObtenerPagosAdminAsync()
    {
        var pagos = await _repository.ObtenerTodosLosPagosAsync();

        return pagos
            .Select(p => new PagoAdminDto
            {
                Id = p.Id,
                PerfilDealerId = p.PerfilDealerId,
                DealerNombreAgencia = p.PerfilDealer?.NombreAgencia ?? $"Dealer {p.PerfilDealerId}",
                DealerEmail = p.PerfilDealer?.Usuario.Email ?? string.Empty,
                Nivel = p.Nivel,
                Ciclo = p.Ciclo,
                Estado = p.Estado,
                Monto = p.Monto,
                Moneda = p.Moneda,
                OrdenIdPayPal = p.OrderIdPayPal,
                CaptureIdPayPal = p.CaptureIdPayPal,
                Metodo = p.Metodo,
                EstadoTransferencia = p.EstadoTransferencia,
                UrlCapturaTransferencia = p.UrlCapturaTransferencia,
                NotasAdmin = p.NotasAdmin,
                FechaConfirmacionUtc = p.FechaConfirmacionUtc,
                FechaUtc = p.FechaUtc
            })
            .ToList();
    }

/// <summary>
/// ReembolsarPagoAsync Reembolsar pago async. Parámetros: Parámetro pagoId (int). Retorna: Task.
/// Para transferencias bancarias no se llama a PayPal: se marca en el sistema
/// y la devolución del dinero se realiza manualmente por el administrador.
/// En ambos métodos la suscripción del dealer queda cancelada.
/// </summary>
    public async Task<MetodoPago> ReembolsarPagoAsync(int pagoId)
    {
        var pago = await _repository.ObtenerPagoPorIdAsync(pagoId);

        if (pago == null)
            throw new KeyNotFoundException("No se encontró el pago solicitado.");

        if (pago.Estado == EstadoPago.Reembolsado)
            throw new BusinessRuleException("El pago ya se encuentra reembolsado.");

        if (pago.Metodo == MetodoPago.PayPal)
        {
            var captureId = pago.CaptureIdPayPal;

            // Pago registrado antes de guardar el CaptureId: se intenta recuperar de PayPal.
            if (string.IsNullOrWhiteSpace(captureId) && !string.IsNullOrWhiteSpace(pago.OrderIdPayPal))
            {
                captureId = await _payPalService.ObtenerCaptureIdDeOrdenAsync(pago.OrderIdPayPal);
            }

            if (string.IsNullOrWhiteSpace(captureId))
            {
                throw new BusinessRuleException(
                    "Este pago no tiene una captura de PayPal vinculada y no es posible reembolsarlo.");
            }

            var reembolsado = await _payPalService.ReembolsarAsync(captureId, pago.Monto, pago.Moneda);

            if (!reembolsado)
                throw new BusinessRuleException("PayPal rechazó el reembolso. Verifica el estado de la captura.");

            pago.RegistrarCaptureId(captureId);
        }

        pago.MarcarComoReembolsado();
        await _repository.ActualizarPagoAsync(pago);

        // El dinero se devolvió (PayPal automático o transferencia manual):
        // la suscripción asociada queda cancelada.
        var suscripcion = await _repository.ObtenerPorDealerIdAsync(pago.PerfilDealerId);

        if (suscripcion != null && suscripcion.Estado != EstadoSuscripcion.Cancelada)
        {
            suscripcion.Cancelar();
            await _repository.ActualizarAsync(suscripcion);
        }

        return pago.Metodo;
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
            DiasRestantes = Math.Max(0, (suscripcion.FechaVencimientoUtc.Date - DateTime.UtcNow.Date).Days),
            Activa = suscripcion.FechaVencimientoUtc > DateTime.UtcNow
        };
    }

    public async Task<SuscripcionDealerDto?> ObtenerSuscripcionPorUsuarioIdAsync(int usuarioId)
    {
        var suscripcion = await _repository.ObtenerPorUsuarioIdAsync(usuarioId);

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
            DiasRestantes = Math.Max(0, (suscripcion.FechaVencimientoUtc.Date - DateTime.UtcNow.Date).Days),
            Activa = suscripcion.FechaVencimientoUtc > DateTime.UtcNow
        };
    }

    /// <summary>
    /// CancelarSuscripcionAsync Cancelar suscripcion async. Parámetros: Parámetro perfilDealerId (int). Retorna: Task.
    /// </summary>
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

    public async Task<int> RegistrarPagoTransferenciaAsync(
        int perfilDealerId,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        decimal monto,
        string moneda,
        string urlCaptura)
    {
        var pago = new PagoSuscripcion(
            perfilDealerId,
            nivel,
            ciclo,
            monto,
            moneda,
            MetodoPago.Transferencia,
            urlCapturaTransferencia: urlCaptura);

        await _repository.AgregarPagoAsync(pago);

        var suscripcionExistente = await _repository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcionExistente == null)
        {
            await ValidarInventarioContraNuevoPlanAsync(perfilDealerId, nivel);

            var nuevaSuscripcion = new SuscripcionDealer(perfilDealerId, nivel, ciclo);
            nuevaSuscripcion.ActivarTemporalmente(nivel, ciclo, TimeSpan.FromDays(1));
            nuevaSuscripcion.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));
            await _repository.AgregarAsync(nuevaSuscripcion);
        }
        else if (suscripcionExistente.Estado == Core.Entities.Enums.EstadoSuscripcion.Cancelada)
        {
            await ValidarInventarioContraNuevoPlanAsync(perfilDealerId, nivel);
            suscripcionExistente.ActivarTemporalmente(nivel, ciclo, TimeSpan.FromDays(1));
            suscripcionExistente.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));
            await _repository.ActualizarAsync(suscripcionExistente);
        }
        else
        {
            suscripcionExistente.ActivarTemporalmente(nivel, ciclo, TimeSpan.FromDays(1));
            suscripcionExistente.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(nivel));
            await _repository.ActualizarAsync(suscripcionExistente);
        }

        return pago.Id;
    }

    public async Task AprobarTransferenciaAsync(int pagoId, string? notas = null)
    {
        var pago = await _repository.ObtenerPagoPorIdAsync(pagoId);

        if (pago == null)
            throw new KeyNotFoundException("No se encontró el pago solicitado.");

        if (pago.Metodo != Core.Entities.Enums.MetodoPago.Transferencia)
            throw new BusinessRuleException("Este pago no es una transferencia bancaria.");

        if (pago.EstadoTransferencia != Core.Entities.Enums.EstadoTransferencia.Pendiente)
            throw new BusinessRuleException("La transferencia no está pendiente de aprobación.");

        pago.AprobarTransferencia(notas);
        await _repository.ActualizarPagoAsync(pago);

        var suscripcion = await _repository.ObtenerPorDealerIdAsync(pago.PerfilDealerId);

        if (suscripcion != null)
        {
            var nuevaFechaVencimiento = CalcularNuevaVigenciaDesdePago(suscripcion, pago.Ciclo);
            suscripcion.RenovarManualmente(nuevaFechaVencimiento);
            suscripcion.VincularPlanCatalogo(
                await _planCatalogoRepository.ObtenerPorNivelAsync(pago.Nivel));
            await _repository.ActualizarAsync(suscripcion);
        }
    }

    public async Task RechazarTransferenciaAsync(int pagoId, string? notas = null)
    {
        var pago = await _repository.ObtenerPagoPorIdAsync(pagoId);

        if (pago == null)
            throw new KeyNotFoundException("No se encontró el pago solicitado.");

        if (pago.Metodo != Core.Entities.Enums.MetodoPago.Transferencia)
            throw new BusinessRuleException("Este pago no es una transferencia bancaria.");

        if (pago.EstadoTransferencia != Core.Entities.Enums.EstadoTransferencia.Pendiente)
            throw new BusinessRuleException("La transferencia no está pendiente de aprobación.");

        pago.RechazarTransferencia(notas);
        await _repository.ActualizarPagoAsync(pago);

        var suscripcion = await _repository.ObtenerPorDealerIdAsync(pago.PerfilDealerId);

        if (suscripcion != null && suscripcion.Estado != Core.Entities.Enums.EstadoSuscripcion.Cancelada)
        {
            suscripcion.Cancelar();
            await _repository.ActualizarAsync(suscripcion);
        }
    }

    public async Task<IReadOnlyList<PagoAdminDto>> ObtenerTransferenciasPendientesAsync()
    {
        var pagos = await _repository.ObtenerTransferenciasPendientesAsync();

        return pagos
            .Select(p => new PagoAdminDto
            {
                Id = p.Id,
                PerfilDealerId = p.PerfilDealerId,
                DealerNombreAgencia = p.PerfilDealer?.NombreAgencia ?? $"Dealer {p.PerfilDealerId}",
                DealerEmail = p.PerfilDealer?.Usuario.Email ?? string.Empty,
                Nivel = p.Nivel,
                Ciclo = p.Ciclo,
                Estado = p.Estado,
                Monto = p.Monto,
                Moneda = p.Moneda,
                Metodo = p.Metodo,
                EstadoTransferencia = p.EstadoTransferencia,
                UrlCapturaTransferencia = p.UrlCapturaTransferencia,
                NotasAdmin = p.NotasAdmin,
                FechaConfirmacionUtc = p.FechaConfirmacionUtc,
                FechaUtc = p.FechaUtc
            })
            .ToList();
    }
}
