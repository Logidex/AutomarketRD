using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Suscripcion;
using MediatR;

namespace AutoMarket.Application.Features.Suscripciones.Queries;

public record ObtenerHistorialPagosQuery(int PerfilDealerId) : IRequest<IReadOnlyList<PagoSuscripcionDto>>;
public record ObtenerSuscripcionQuery(int PerfilDealerId) : IRequest<SuscripcionDealerDto?>;
public record ObtenerSuscripcionPorUsuarioIdQuery(int UsuarioId) : IRequest<SuscripcionDealerDto?>;
public record ObtenerPagosAdminQuery() : IRequest<IReadOnlyList<PagoAdminDto>>;
public record ObtenerTransferenciasPendientesQuery() : IRequest<IReadOnlyList<PagoAdminDto>>;
public record ExistePagoPorEventoQuery(string EventoId) : IRequest<bool>;
public record ExistePagoPorOrdenQuery(string OrderId) : IRequest<bool>;
