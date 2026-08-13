using AutoMarket.Application.DTOs.Paypal;

namespace AutoMarket.Application.Interfaces;

public interface IPayPalService
{
    // 1. Le pide a PayPal que cree una orden y nos devuelve el link para que el Dealer pague
    Task<string> CrearOrdenDeSuscripcionAsync(int dealerId, decimal monto, string nombrePlan, string ciclo);

    // 2. Consulta el detalle de una orden (estado, monto y referencia) para confirmarla al regresar
    Task<DetalleOrdenPayPalDto> ObtenerDetalleOrdenAsync(string idOrden);

    // 3. Verifica que el dinero haya entrado realmente cuando de PayPal nos avise (Webhook)
    Task<bool> CapturarOrdenAsync(string idOrden);

    // 4. Extrae el identificador de la captura ya realizada (para poder reembolsarla)
    Task<string?> ObtenerCaptureIdDeOrdenAsync(string idOrden);

    // 5. Reembolsa un pago previamente capturado (reembolso total)
    Task<bool> ReembolsarAsync(string captureId, decimal monto, string moneda);

    Task<bool> VerificarFirmaWebhookAsync(
        string jsonBody,
        string transmissionId,
        string transmissionTime,
        string transmissionSig,
        string certUrl,
        string authAlgo);

}