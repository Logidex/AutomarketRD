namespace AutoMarket.Application.Interfaces;

public interface IPayPalService
{
    // 1. Le pide a PayPal que cree una orden y nos devuelva el link para que el Dealer pague
    Task<string> CrearOrdenDeSuscripcionAsync(int dealerId, decimal monto, string nombrePlan, string ciclo);
    
    // 2. Verifica que el dinero haya entrado realmente cuando PayPal nos avise (Webhook)
    Task<bool> CapturarOrdenAsync(string idOrden);
}