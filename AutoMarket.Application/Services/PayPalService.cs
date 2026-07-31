using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AutoMarket.Application.Interfaces;

namespace AutoMarket.Application.Services;

public class PayPalService : IPayPalService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public PayPalService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        
        // Leemos de la configuración (appsettings + User Secrets fusionados)
        _baseUrl = configuration["PayPal:UrlBase"] ?? throw new ArgumentNullException("PayPal:UrlBase no configurada");
        _clientId = configuration["PayPal:ClientId"] ?? throw new ArgumentNullException("Falta ClientId");
        _clientSecret = configuration["PayPal:ClientSecret"] ?? throw new ArgumentNullException("Falta ClientSecret");
    }

    // 🔑 Método privado para conseguir nuestro "Gafete de acceso"
    private async Task<string> ObtenerTokenDeAccesoAsync()
    {
        var authBytes = Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
        var authBase64 = Convert.ToBase64String(authBytes);
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authBase64);
        
        // PayPal exige que enviemos este string exacto para darnos el token
        request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        
        return document.RootElement.GetProperty("access_token").GetString()!;
    }

    // 💸 Método 1: Crear la orden para que el Dealer pague
    public async Task<string> CrearOrdenDeSuscripcionAsync(int dealerId, decimal monto, string nombrePlan, string ciclo)
    {
        var token = await ObtenerTokenDeAccesoAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Estructura oficial requerida por PayPal
        var orderPayload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = $"DEALER_{dealerId}_PLAN_{nombrePlan.ToUpper()}_CICLO_{ciclo.ToUpper()}",
                    amount = new
                    {
                        currency_code = "USD", // Cobraremos en dólares por ahora
                        value = monto.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    },
                    description = $"Suscripción AutoMarket - {nombrePlan}"
                }
            },
            application_context = new
            {
                // Cuando terminemos el frontend, PayPal enviará al usuario a estas rutas
                return_url = "http://localhost:3000/pago-exitoso", 
                cancel_url = "http://localhost:3000/pago-cancelado"
            }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        
        // PayPal devuelve varios links. Buscamos el de "approve" (donde el cliente pone la tarjeta)
        var links = document.RootElement.GetProperty("links").EnumerateArray();
        foreach (var link in links)
        {
            if (link.GetProperty("rel").GetString() == "approve")
            {
                return link.GetProperty("href").GetString()!;
            }
        }

        throw new Exception("No se encontró el link de aprobación de pago en la respuesta de PayPal.");
    }

    // ✅ Método 2: Capturar el dinero cuando el webhook nos avise
    public async Task<bool> CapturarOrdenAsync(string idOrden)
    {
        var token = await ObtenerTokenDeAccesoAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders/{idOrden}/capture");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Este endpoint requiere un body vacío con content-type application/json
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        return response.IsSuccessStatusCode;
    }
}