using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;
using System.Text.Json;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PagosController : ControllerBase
{
    private readonly IPayPalService _payPalService;
    private readonly ISuscripcionService _suscripcionService;

    public PagosController(IPayPalService payPalService, ISuscripcionService suscripcionService)
    {
        _payPalService = payPalService;
        _suscripcionService = suscripcionService;
    }

    // 🔒 1. Endpoint para tu Frontend (Requiere que el Dealer esté logueado)
    [Authorize]
    [HttpPost("generar-link")]
    public async Task<IActionResult> GenerarLinkDePago([FromBody] CrearOrdenDto request)
    {
        try
        {
            // Extraemos quién es el Dealer desde el Token de seguridad
            var dealerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Le pedimos a PayPal la URL mágica de cobro
            var linkPago = await _payPalService.CrearOrdenDeSuscripcionAsync(dealerId, request.Monto, request.NombrePlan, request.Ciclo);

            return Ok(new { url = linkPago });
        }
        catch (Exception ex)
        {
            // Si algo falla con las llaves o la red, le avisamos al frontend
            return BadRequest(new { mensaje = "Error al conectar con PayPal.", detalle = ex.Message });
        }
    }

    // 🔓 2. Endpoint para PayPal (Debe ser público)
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> PayPalWebhook()
    {
        try
        {
            // 1. Leemos el mensaje JSON que nos manda PayPal
            using var reader = new StreamReader(Request.Body);
            var jsonBody = await reader.ReadToEndAsync();
            using var document = JsonDocument.Parse(jsonBody);

            var root = document.RootElement;
            var eventType = root.GetProperty("event_type").GetString();

            // 2. Si el evento es que el cliente aprobó el pago con su tarjeta...
            if (eventType == "CHECKOUT.ORDER.APPROVED")
            {
                var resource = root.GetProperty("resource");
                var orderId = resource.GetProperty("id").GetString();

                // Extraemos nuestro código secreto (Ej: "DEALER_5_PLAN_PRO")
                var referenceId = resource.GetProperty("purchase_units")[0].GetProperty("reference_id").GetString();

                // 3. ¡Cobramos el dinero! (Llamamos a nuestro servicio)
                var cobroExitoso = await _payPalService.CapturarOrdenAsync(orderId!);

                if (cobroExitoso)
                {
                    var partes = referenceId!.Split('_');
                    var dealerId = int.Parse(partes[1]);
                    var nombrePlanString = partes[3];
                    var cicloString = partes[5];      

                    // ✅ Ahora sí validamos el Plan por un lado y el Ciclo por el otro
                    if (Enum.TryParse<PlanNivel>(nombrePlanString, true, out var planEnum) &&
                        Enum.TryParse<CicloFacturacion>(cicloString, true, out var cicloEnum))
                    {
                        // Le pasamos los Enums correctos a tu servicio (fíjate que cicloEnum va con minúscula)
                        await _suscripcionService.CambiarPlanAsync(dealerId, planEnum, cicloEnum);

                        Console.WriteLine($"[ÉXITO] 💰 Dinero cobrado. Plan {planEnum} ({cicloEnum}) activado en BD para Dealer {dealerId}");
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] ❌ PayPal mandó el plan {nombrePlanString} o el ciclo {cicloString}, pero no coinciden con tus Enums.");
                    }
                }
            }

            // Siempre debemos responder 200 OK rápido para que PayPal no reintente enviar el mensaje
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOK ERROR] {ex.Message}");
            return Ok(); // Respondemos OK incluso si falla nuestro código, para que PayPal no nos haga spam
        }
    }
}