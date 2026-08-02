using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.DTOs.Paypal;

public sealed class ReferenciaPagoPayPal
{
    public int PerfilDealerId { get; init; }
    public PlanNivel Plan { get; init; }
    public CicloFacturacion Ciclo { get; init; }
}