using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Core.Entities;

public class PagoSuscripcion
{
    public int Id { get; private set; }
    public int PerfilDealerId { get; private set; }
    public virtual PerfilDealer PerfilDealer { get; private set; } = null!;

    public PlanNivel Nivel { get; private set; }
    public CicloFacturacion Ciclo { get; private set; }
    public EstadoPago Estado { get; private set; }

    public decimal Monto { get; private set; }
    public string Moneda { get; private set; } = null!;

    public string? OrderIdPayPal { get; private set; }
    public string? EventoIdPayPal { get; private set; }
    public string? CaptureIdPayPal { get; private set; }
    public string? Referencia { get; private set; }

    public MetodoPago Metodo { get; private set; }
    public EstadoTransferencia? EstadoTransferencia { get; private set; }
    public string? UrlCapturaTransferencia { get; private set; }
    public string? NotasAdmin { get; private set; }
    public DateTime? FechaConfirmacionUtc { get; private set; }

    public DateTime FechaUtc { get; private set; }

    private PagoSuscripcion() { }

    public PagoSuscripcion(
        int perfilDealerId,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        decimal monto,
        string moneda,
        MetodoPago metodo = MetodoPago.PayPal,
        string? orderIdPayPal = null,
        string? eventoIdPayPal = null,
        string? captureIdPayPal = null,
        string? referencia = null,
        string? urlCapturaTransferencia = null)
    {
        if (perfilDealerId <= 0)
            throw new ArgumentException("El perfilDealerId es inválido.", nameof(perfilDealerId));

        if (monto <= 0m)
            throw new ArgumentException("El monto del pago debe ser mayor que cero.", nameof(monto));

        if (string.IsNullOrWhiteSpace(moneda))
            throw new ArgumentException("La moneda del pago es obligatoria.", nameof(moneda));

        PerfilDealerId = perfilDealerId;
        Nivel = nivel;
        Ciclo = ciclo;
        Metodo = metodo;
        Monto = monto;
        Moneda = moneda.Trim().ToUpperInvariant();
        FechaUtc = DateTime.UtcNow;

        if (metodo == MetodoPago.Transferencia)
        {
            Estado = EstadoPago.Completado;
            EstadoTransferencia = AutoMarket.Core.Entities.Enums.EstadoTransferencia.Pendiente;
            UrlCapturaTransferencia = urlCapturaTransferencia;
        }
        else
        {
            Estado = EstadoPago.Completado;
            OrderIdPayPal = orderIdPayPal;
            EventoIdPayPal = eventoIdPayPal;
            CaptureIdPayPal = captureIdPayPal;
            Referencia = referencia;
        }
    }

    public void MarcarComoFallido()
    {
        Estado = EstadoPago.Fallido;
    }

    public void RegistrarCaptureId(string captureIdPayPal)
    {
        if (string.IsNullOrWhiteSpace(captureIdPayPal))
            throw new ArgumentException("El identificador de captura no puede estar vacío.", nameof(captureIdPayPal));

        CaptureIdPayPal = captureIdPayPal;
    }

    public void MarcarComoReembolsado()
    {
        if (Estado == EstadoPago.Reembolsado)
            throw new BusinessRuleException("El pago ya se encuentra reembolsado.");

        Estado = EstadoPago.Reembolsado;
    }

    public void AprobarTransferencia(string? notas = null)
    {
        if (Metodo != MetodoPago.Transferencia)
            throw new BusinessRuleException("Este pago no es una transferencia bancaria.");

        if (EstadoTransferencia != AutoMarket.Core.Entities.Enums.EstadoTransferencia.Pendiente)
            throw new BusinessRuleException("La transferencia no está pendiente de aprobación.");

        EstadoTransferencia = AutoMarket.Core.Entities.Enums.EstadoTransferencia.Aprobada;
        NotasAdmin = notas;
        FechaConfirmacionUtc = DateTime.UtcNow;
    }

    public void RechazarTransferencia(string? notas = null)
    {
        if (Metodo != MetodoPago.Transferencia)
            throw new BusinessRuleException("Este pago no es una transferencia bancaria.");

        if (EstadoTransferencia != AutoMarket.Core.Entities.Enums.EstadoTransferencia.Pendiente)
            throw new BusinessRuleException("La transferencia no está pendiente de aprobación.");

        EstadoTransferencia = AutoMarket.Core.Entities.Enums.EstadoTransferencia.Rechazada;
        NotasAdmin = notas;
        FechaConfirmacionUtc = DateTime.UtcNow;
    }
}