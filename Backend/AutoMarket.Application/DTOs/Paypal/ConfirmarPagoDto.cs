using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Application.DTOs.Paypal;

/// <summary>
/// Confirmación del pago al regresar de PayPal (la orden trae el plan/ciclo en su reference_id).
/// </summary>
public class ConfirmarPagoDto
{
    [Required(ErrorMessage = "El identificador de la orden de PayPal es requerido.")]
    public string OrderId { get; set; } = string.Empty;
}