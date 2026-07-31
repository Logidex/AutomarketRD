using System.Net;
using System.Text;
using AutoMarket.Application.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AutoMarket.Tests.Services;

public class PayPalServiceTests
{
    private static IConfiguration CrearConfiguracion(bool incluirWebhookId = true)
    {
        var datos = new Dictionary<string, string?>
        {
            ["PayPal:UrlBase"] = "https://api-m.sandbox.paypal.com",
            ["PayPal:ClientId"] = "client-id-test",
            ["PayPal:ClientSecret"] = "client-secret-test",
            ["PayPal:ReturnUrl"] = "https://localhost:3000/pago-exitoso",
            ["PayPal:CancelUrl"] = "https://localhost:3000/pago-cancelado"
        };

        if (incluirWebhookId)
            datos["PayPal:WebhookId"] = "webhook-id-test";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(datos)
            .Build();
    }

    [Fact]
    public async Task CrearOrdenDeSuscripcionAsync_RespuestaValida_DebeRetornarApproveUrl()
    {
        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "access_token": "token-test"
                }
                """, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "links": [
                    { "rel": "self", "href": "https://api.paypal.com/self" },
                    { "rel": "approve", "href": "https://paypal.com/checkoutnow?token=abc123" }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            }
        });

        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion();
        var service = new PayPalService(httpClient, config);

        var url = await service.CrearOrdenDeSuscripcionAsync(15, 100m, "Pro", "Mensual");

        Assert.Equal("https://paypal.com/checkoutnow?token=abc123", url);
    }

    [Fact]
    public async Task CrearOrdenDeSuscripcionAsync_SinApproveUrl_DebeLanzarExcepcion()
    {
        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "access_token": "token-test"
                }
                """, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "links": [
                    { "rel": "self", "href": "https://api.paypal.com/self" }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            }
        });

        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion();
        var service = new PayPalService(httpClient, config);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CrearOrdenDeSuscripcionAsync(15, 100m, "Pro", "Mensual"));

        Assert.Equal("No se encontró el link de aprobación de pago en la respuesta de PayPal.", ex.Message);
    }

    [Fact]
    public async Task CapturarOrdenAsync_StatusExitoso_DebeRetornarTrue()
    {
        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "access_token": "token-test"
                }
                """, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }
        });

        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion();
        var service = new PayPalService(httpClient, config);

        var resultado = await service.CapturarOrdenAsync("orden-123");

        Assert.True(resultado);
    }

    [Fact]
    public async Task CapturarOrdenAsync_StatusFallido_DebeRetornarFalse()
    {
        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "access_token": "token-test"
                }
                """, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }
        });

        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion();
        var service = new PayPalService(httpClient, config);

        var resultado = await service.CapturarOrdenAsync("orden-123");

        Assert.False(resultado);
    }

    [Fact]
    public async Task VerificarFirmaWebhookAsync_StatusSuccess_DebeRetornarTrue()
    {
        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "access_token": "token-test"
                }
                """, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "verification_status": "SUCCESS"
                }
                """, Encoding.UTF8, "application/json")
            }
        });

        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion();
        var service = new PayPalService(httpClient, config);

        var jsonBody = """
        {
          "id": "evt-1",
          "event_type": "CHECKOUT.ORDER.APPROVED"
        }
        """;

        var resultado = await service.VerificarFirmaWebhookAsync(
            jsonBody,
            "transmission-id",
            "2026-07-31T12:00:00Z",
            "firma",
            "https://paypal.com/cert",
            "SHA256withRSA");

        Assert.True(resultado);
    }

    [Fact]
    public async Task VerificarFirmaWebhookAsync_StatusDistintoDeSuccess_DebeRetornarFalse()
    {
        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "access_token": "token-test"
                }
                """, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "verification_status": "FAILURE"
                }
                """, Encoding.UTF8, "application/json")
            }
        });

        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion();
        var service = new PayPalService(httpClient, config);

        var jsonBody = """
        {
          "id": "evt-1",
          "event_type": "CHECKOUT.ORDER.APPROVED"
        }
        """;

        var resultado = await service.VerificarFirmaWebhookAsync(
            jsonBody,
            "transmission-id",
            "2026-07-31T12:00:00Z",
            "firma",
            "https://paypal.com/cert",
            "SHA256withRSA");

        Assert.False(resultado);
    }

    [Fact]
    public async Task VerificarFirmaWebhookAsync_SinWebhookId_DebeLanzarInvalidOperationException()
    {
        var handler = new FakeHttpMessageHandler(Array.Empty<HttpResponseMessage>());
        var httpClient = new HttpClient(handler);
        var config = CrearConfiguracion(incluirWebhookId: false);
        var service = new PayPalService(httpClient, config);

        var jsonBody = """
        {
          "id": "evt-1",
          "event_type": "CHECKOUT.ORDER.APPROVED"
        }
        """;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.VerificarFirmaWebhookAsync(
                jsonBody,
                "transmission-id",
                "2026-07-31T12:00:00Z",
                "firma",
                "https://paypal.com/cert",
                "SHA256withRSA"));

        Assert.Equal("Falta PayPal:WebhookId.", ex.Message);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public FakeHttpMessageHandler(IEnumerable<HttpResponseMessage> responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
                throw new InvalidOperationException("No hay más respuestas configuradas para este test.");

            return Task.FromResult(_responses.Dequeue());
        }
    }
}