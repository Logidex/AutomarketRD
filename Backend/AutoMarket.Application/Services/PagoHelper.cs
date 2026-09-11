using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Core.Entities.Enums;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace AutoMarket.Application.Services;

/// <summary>
/// Utilidades compartidas para el módulo de pagos.
/// Elimina la lógica de tipo de cambio y precio de plan duplicada en PagosController.
/// </summary>
public static class PagoHelper
{
    private const decimal TasaPorDefecto = 0.017m;

    /// <summary>
    /// Obtiene la tasa de cambio RD→USD desde la configuración.
    /// Si no está configurada o es inválida, retorna 0.017.
    /// </summary>
    public static decimal ObtenerTasaCambio(IConfiguration configuration)
    {
        var tasaStr = configuration["Pago:TasaCambioRD_USD"];
        var tasa = decimal.TryParse(tasaStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var tasaParseada)
            ? tasaParseada
            : TasaPorDefecto;

        return tasa > 0m ? tasa : TasaPorDefecto;
    }

    /// <summary>
    /// Calcula el precio en RD$ según el ciclo de facturación.
    /// </summary>
    public static decimal ObtenerPrecioEnRD(PlanCatalogoDto plan, CicloFacturacion ciclo) => ciclo switch
    {
        CicloFacturacion.Mensual => plan.PrecioMensual,
        CicloFacturacion.Trimestral => plan.PrecioTrimestral,
        CicloFacturacion.Anual => plan.PrecioAnual,
        _ => 0m
    };

    /// <summary>
    /// Convierte un precio en RD$ a USD usando la tasa de cambio configurada.
    /// </summary>
    public static decimal ConvertirAUSD(decimal precioRD, decimal tasaCambio)
        => Math.Round(precioRD * tasaCambio, 2);
}
