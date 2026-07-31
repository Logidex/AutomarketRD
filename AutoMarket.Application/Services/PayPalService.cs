using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoMarket.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AutoMarket.Application.Services;

public class PayPalService : IPayPalService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;
    private readonly string _webhookId;

    public PayPalService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _baseUrl = configuration["PayPal:UrlBase"]
            ?? throw new ArgumentNullException("PayPal:UrlBase no configurada.");

        _clientId = configuration["PayPal:ClientId"]
            ?? throw new ArgumentNullException("Falta PayPal:ClientId.");

        _clientSecret = configuration["PayPal:ClientSecret"]
            ?? throw new ArgumentNullException("Falta PayPal:ClientSecret.");

        _returnUrl = configuration["PayPal:ReturnUrl"]
            ?? throw new ArgumentNullException("Falta PayPal:ReturnUrl.");

        _cancelUrl = configuration["PayPal:CancelUrl"]
            ?? throw new ArgumentNullException("Falta PayPal:CancelUrl.");

        _webhookId = configuration["PayPal:WebhookId"]
            ?? string.Empty;
    }

    private async Task<string> ObtenerTokenDeAccesoAsync()
    {
        var authBytes = Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
        var authBase64 = Convert.ToBase64String(authBytes);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authBase64);
        request.Content = new StringContent(
            "grant_type=client_credentials",
            Encoding.UTF8,
            "application/x-www-form-urlencoded");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        return document.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<string> CrearOrdenDeSuscripcionAsync(
        int dealerId,
        decimal monto,
        string nombrePlan,
        string ciclo)
    {
        var token = await ObtenerTokenDeAccesoAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var orderPayload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = $"DEALER-{dealerId}-PLAN-{nombrePlan.ToUpperInvariant()}-CICLO-{ciclo.ToUpperInvariant()}",
                    amount = new
                    {
                        currency_code = "USD",
                        value = monto.ToString("0.00", CultureInfo.InvariantCulture)
                    },
                    description = $"Suscripción AutoMarket - {nombrePlan}"
                }
            },
            application_context = new
            {
                return_url = _returnUrl,
                cancel_url = _cancelUrl
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(orderPayload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

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

    public async Task<bool> CapturarOrdenAsync(string idOrden)
    {
        var token = await ObtenerTokenDeAccesoAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/v2/checkout/orders/{idOrden}/capture");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> VerificarFirmaWebhookAsync(
    string jsonBody,
    string transmissionId,
    string transmissionTime,
    string transmissionSig,
    string certUrl,
    string authAlgo)
    {
        if (string.IsNullOrWhiteSpace(_webhookId))
            throw new InvalidOperationException("Falta PayPal:WebhookId.");

        var token = await ObtenerTokenDeAccesoAsync();

        using var webhookEvent = JsonDocument.Parse(jsonBody);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("transmission_id", transmissionId);
            writer.WriteString("transmission_time", transmissionTime);
            writer.WriteString("cert_url", certUrl);
            writer.WriteString("auth_algo", authAlgo);
            writer.WriteString("transmission_sig", transmissionSig);
            writer.WriteString("webhook_id", _webhookId);
            writer.WritePropertyName("webhook_event");
            webhookEvent.RootElement.WriteTo(writer);
            writer.WriteEndObject();
        }

        var requestBody = Encoding.UTF8.GetString(stream.ToArray());

        Console.WriteLine("=== VERIFY WEBHOOK REQUEST ===");
        Console.WriteLine(requestBody);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/v1/notifications/verify-webhook-signature");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine("=== VERIFY WEBHOOK RESPONSE ===");
        Console.WriteLine(responseBody);

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(responseBody);
        var status = doc.RootElement.GetProperty("verification_status").GetString();

        return string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase);
    }
}