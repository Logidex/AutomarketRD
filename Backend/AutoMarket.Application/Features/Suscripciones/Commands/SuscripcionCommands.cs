using AutoMarket.Core.Entities.Enums;
using MediatR;

namespace AutoMarket.Application.Features.Suscripciones.Commands;

public record AsignarPlanInicialCommand(int PerfilDealerId, PlanNivel Nivel, CicloFacturacion Ciclo) : IRequest;
public record CambiarPlanCommand(int DealerId, PlanNivel NuevoPlan, CicloFacturacion Ciclo) : IRequest;
public record RenovarManualCommand(int PerfilDealerId, DateTime NuevaFechaVencimiento) : IRequest;
public record ProcesarPagoSuscripcionCommand(int PerfilDealerId, PlanNivel Nivel, CicloFacturacion Ciclo) : IRequest;
public record CancelarSuscripcionCommand(int PerfilDealerId) : IRequest;
public record ReembolsarPagoCommand(int PagoId) : IRequest<MetodoPago>;

public record RegistrarPagoCommand(
    int PerfilDealerId, PlanNivel Nivel, CicloFacturacion Ciclo,
    decimal Monto, string Moneda, string? OrderIdPayPal,
    string? EventoIdPayPal, string? CaptureIdPayPal, string? Referencia) : IRequest;

public record RegistrarPagoTransferenciaCommand(
    int PerfilDealerId, PlanNivel Nivel, CicloFacturacion Ciclo,
    decimal Monto, string Moneda, string UrlCaptura) : IRequest<int>;

public record AprobarTransferenciaCommand(int PagoId, string? Notas) : IRequest;
public record RechazarTransferenciaCommand(int PagoId, string? Notas) : IRequest;
