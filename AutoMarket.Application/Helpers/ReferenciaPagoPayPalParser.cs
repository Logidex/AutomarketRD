using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Helpers;

public static class ReferenciaPagoPayPalParser
{
    public static bool TryParse(string? referenceId, out ReferenciaPagoPayPal? referencia)
    {
        referencia = null;

        if (string.IsNullOrWhiteSpace(referenceId))
            return false;

        var partes = referenceId.Split('_', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length != 6) return false;
        if (!partes[0].Equals("PERFIL", StringComparison.OrdinalIgnoreCase)) return false;
        if (!partes[2].Equals("PLAN", StringComparison.OrdinalIgnoreCase)) return false;
        if (!partes[4].Equals("CICLO", StringComparison.OrdinalIgnoreCase)) return false;

        if (!int.TryParse(partes[1], out var perfilDealerId)) return false;
        if (!Enum.TryParse<PlanNivel>(partes[3], true, out var plan)) return false;
        if (!Enum.TryParse<CicloFacturacion>(partes[5], true, out var ciclo)) return false;

        referencia = new ReferenciaPagoPayPal
        {
            PerfilDealerId = perfilDealerId,
            Plan = plan,
            Ciclo = ciclo
        };

        return true;
    }
}