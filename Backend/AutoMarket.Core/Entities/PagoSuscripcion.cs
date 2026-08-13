using AutoMarket.Core.Entities.Enums;

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

    public DateTime FechaUtc { get; private set; }

    private PagoSuscripcion() { }

    public PagoSuscripcion(
        int perfilDealerId,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        decimal monto,
        string moneda,
        string? orderIdPayPal = null,
        string? eventoIdPayPal = null,
        string? captureIdPayPal = null,
        string? referencia = null)
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
        Estado = EstadoPago.Completado;
        Monto = monto;
        Moneda = moneda.Trim().ToUpperInvariant();
        OrderIdPayPal = orderIdPayPal;
        EventoIdPayPal = eventoIdPayPal;
        CaptureIdPayPal = captureIdPayPal;
        Referencia = referencia;
        FechaUtc = DateTime.UtcNow;
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
            throw new InvalidOperationException("El pago ya se encuentra reembolsado.");

        Estado = EstadoPago.Reembolsado;
    }
}