namespace AutoMarket.API.Extensions;

public static class PayPalExtensions
{
    public static string? ValidatePayPalConfiguration(this WebApplicationBuilder builder)
    {
        var paypalUrlBase = builder.Configuration["PayPal:UrlBase"];
        var paypalMode = builder.Configuration["PayPal:Mode"];

        var esUrlLive = !string.IsNullOrWhiteSpace(paypalUrlBase)
            && paypalUrlBase.StartsWith("https://api-m.paypal.com", StringComparison.OrdinalIgnoreCase);

        var esUrlSandbox = !string.IsNullOrWhiteSpace(paypalUrlBase)
            && paypalUrlBase.StartsWith("https://api-m.sandbox.paypal.com", StringComparison.OrdinalIgnoreCase);

        var esModeLive = string.Equals(paypalMode, "Live", StringComparison.OrdinalIgnoreCase);
        var esModeSandbox = string.Equals(paypalMode, "Sandbox", StringComparison.OrdinalIgnoreCase);

        if (esModeLive && !esUrlLive)
        {
            throw new InvalidOperationException("PayPal:Mode es 'Live' pero PayPal:UrlBase no apunta a https://api-m.paypal.com. Verifica PAYPAL_MODE y PAYPAL_URL_BASE.");
        }

        if (esModeSandbox && !esUrlSandbox)
        {
            throw new InvalidOperationException("PayPal:Mode es 'Sandbox' pero PayPal:UrlBase no apunta a https://api-m.sandbox.paypal.com. Verifica PAYPAL_MODE y PAYPAL_URL_BASE.");
        }

        if (!string.IsNullOrWhiteSpace(paypalMode) && !esModeLive && !esModeSandbox)
        {
            throw new InvalidOperationException("PayPal:Mode debe ser 'Live' o 'Sandbox'. Verifica PAYPAL_MODE.");
        }

        if (builder.Environment.IsProduction() && (!esModeLive || !esUrlLive))
        {
            throw new InvalidOperationException("En producción, PayPal debe estar en Live (PAYPAL_MODE=Live, PAYPAL_URL_BASE=https://api-m.paypal.com).");
        }

        return paypalUrlBase;
    }
}