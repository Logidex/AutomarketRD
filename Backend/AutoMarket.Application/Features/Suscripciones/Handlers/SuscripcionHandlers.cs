using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Suscripcion;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Application.Features.Suscripciones.Commands;
using AutoMarket.Application.Features.Suscripciones.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Suscripciones.Handlers;

public class SuscripcionCommandHandler
    : IRequestHandler<AsignarPlanInicialCommand>,
      IRequestHandler<CambiarPlanCommand>,
      IRequestHandler<RenovarManualCommand>,
      IRequestHandler<ProcesarPagoSuscripcionCommand>,
      IRequestHandler<CancelarSuscripcionCommand>,
      IRequestHandler<ReembolsarPagoCommand, MetodoPago>,
      IRequestHandler<RegistrarPagoCommand>,
      IRequestHandler<RegistrarPagoTransferenciaCommand, int>,
      IRequestHandler<AprobarTransferenciaCommand>,
      IRequestHandler<RechazarTransferenciaCommand>
{
    private readonly ISuscripcionService _service;
    public SuscripcionCommandHandler(ISuscripcionService service) => _service = service;

    public async Task Handle(AsignarPlanInicialCommand request, CancellationToken ct)
        => await _service.AsignarPlanInicialAsync(request.PerfilDealerId, request.Nivel, request.Ciclo);

    public async Task Handle(CambiarPlanCommand request, CancellationToken ct)
        => await _service.CambiarPlanAsync(request.DealerId, request.NuevoPlan, request.Ciclo);

    public async Task Handle(RenovarManualCommand request, CancellationToken ct)
        => await _service.RenovarManualAsync(request.PerfilDealerId, request.NuevaFechaVencimiento);

    public async Task Handle(ProcesarPagoSuscripcionCommand request, CancellationToken ct)
        => await _service.ProcesarPagoSuscripcionAsync(request.PerfilDealerId, request.Nivel, request.Ciclo);

    public async Task Handle(CancelarSuscripcionCommand request, CancellationToken ct)
        => await _service.CancelarSuscripcionAsync(request.PerfilDealerId);

    public async Task<MetodoPago> Handle(ReembolsarPagoCommand request, CancellationToken ct)
        => await _service.ReembolsarPagoAsync(request.PagoId);

    public async Task Handle(RegistrarPagoCommand request, CancellationToken ct)
        => await _service.RegistrarPagoAsync(
            request.PerfilDealerId, request.Nivel, request.Ciclo,
            request.Monto, request.Moneda, request.OrderIdPayPal,
            request.EventoIdPayPal, request.CaptureIdPayPal, request.Referencia);

    public async Task<int> Handle(RegistrarPagoTransferenciaCommand request, CancellationToken ct)
        => await _service.RegistrarPagoTransferenciaAsync(
            request.PerfilDealerId, request.Nivel, request.Ciclo,
            request.Monto, request.Moneda, request.UrlCaptura);

    public async Task Handle(AprobarTransferenciaCommand request, CancellationToken ct)
        => await _service.AprobarTransferenciaAsync(request.PagoId, request.Notas);

    public async Task Handle(RechazarTransferenciaCommand request, CancellationToken ct)
        => await _service.RechazarTransferenciaAsync(request.PagoId, request.Notas);
}

public class SuscripcionQueryHandler
    : IRequestHandler<ObtenerHistorialPagosQuery, IReadOnlyList<PagoSuscripcionDto>>,
      IRequestHandler<ObtenerSuscripcionQuery, SuscripcionDealerDto?>,
      IRequestHandler<ObtenerSuscripcionPorUsuarioIdQuery, SuscripcionDealerDto?>,
      IRequestHandler<ObtenerPagosAdminQuery, IReadOnlyList<PagoAdminDto>>,
      IRequestHandler<ObtenerTransferenciasPendientesQuery, IReadOnlyList<PagoAdminDto>>,
      IRequestHandler<ExistePagoPorEventoQuery, bool>,
      IRequestHandler<ExistePagoPorOrdenQuery, bool>
{
    private readonly ISuscripcionService _service;
    public SuscripcionQueryHandler(ISuscripcionService service) => _service = service;

    public async Task<IReadOnlyList<PagoSuscripcionDto>> Handle(ObtenerHistorialPagosQuery request, CancellationToken ct)
        => await _service.ObtenerHistorialPagosAsync(request.PerfilDealerId);

    public async Task<SuscripcionDealerDto?> Handle(ObtenerSuscripcionQuery request, CancellationToken ct)
        => await _service.ObtenerSuscripcionAsync(request.PerfilDealerId);

    public async Task<SuscripcionDealerDto?> Handle(ObtenerSuscripcionPorUsuarioIdQuery request, CancellationToken ct)
        => await _service.ObtenerSuscripcionPorUsuarioIdAsync(request.UsuarioId);

    public async Task<IReadOnlyList<PagoAdminDto>> Handle(ObtenerPagosAdminQuery request, CancellationToken ct)
        => await _service.ObtenerPagosAdminAsync();

    public async Task<IReadOnlyList<PagoAdminDto>> Handle(ObtenerTransferenciasPendientesQuery request, CancellationToken ct)
        => await _service.ObtenerTransferenciasPendientesAsync();

    public async Task<bool> Handle(ExistePagoPorEventoQuery request, CancellationToken ct)
        => await _service.ExistePagoPorEventoAsync(request.EventoId);

    public async Task<bool> Handle(ExistePagoPorOrdenQuery request, CancellationToken ct)
        => await _service.ExistePagoPorOrdenAsync(request.OrderId);
}
