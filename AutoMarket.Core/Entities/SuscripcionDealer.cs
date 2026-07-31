using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

public class SuscripcionDealer
{
    public int Id { get; private set; }
    public int PerfilDealerId { get; private set; }
    public virtual PerfilDealer PerfilDealer { get; private set; } = null!;

    public PlanNivel Nivel { get; private set; }
    public CicloFacturacion Ciclo { get; private set; }
    public EstadoSuscripcion Estado { get; private set; }

    public int LimiteAnuncios => (int)Nivel;

    public DateTime FechaInicioUtc { get; private set; }
    public DateTime FechaVencimientoUtc { get; private set; }

    private SuscripcionDealer() { }

    public SuscripcionDealer(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo)
    {
        if (perfilDealerId <= 0)
            throw new ArgumentException("El perfilDealerId es inválido.", nameof(perfilDealerId));

        PerfilDealerId = perfilDealerId;
        Nivel = nivel;
        Ciclo = ciclo;
        Estado = EstadoSuscripcion.Activa;
        FechaInicioUtc = DateTime.UtcNow;
        FechaVencimientoUtc = CalcularFechaVencimiento(ciclo);
    }

    public bool PermiteNuevosAnuncios(int cantidadAnunciosActuales)
    {
        if (Estado != EstadoSuscripcion.Activa) return false;
        if (DateTime.UtcNow > FechaVencimientoUtc) return false;

        return cantidadAnunciosActuales < LimiteAnuncios;
    }

    public void CambiarPlan(PlanNivel nuevoNivel, CicloFacturacion nuevoCiclo)
    {
        if (Estado == EstadoSuscripcion.Cancelada)
        {
            throw new InvalidOperationException(
                "Imposible mutar: la suscripción actual se encuentra cancelada.");
        }

        Nivel = nuevoNivel;
        Ciclo = nuevoCiclo;
        Estado = EstadoSuscripcion.Activa;
        FechaInicioUtc = DateTime.UtcNow;
        FechaVencimientoUtc = CalcularFechaVencimiento(nuevoCiclo);
    }

    public void RenovarManualmente(DateTime nuevaFechaVencimiento)
    {
        if (nuevaFechaVencimiento <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "La nueva fecha de vencimiento debe ser en el futuro.",
                nameof(nuevaFechaVencimiento));
        }

        FechaVencimientoUtc = nuevaFechaVencimiento;
        Estado = EstadoSuscripcion.Activa;
    }

    private static DateTime CalcularFechaVencimiento(CicloFacturacion ciclo)
    {
        var ahora = DateTime.UtcNow;

        return ciclo switch
        {
            CicloFacturacion.Mensual => ahora.AddMonths(1),
            CicloFacturacion.Trimestral => ahora.AddMonths(3),
            CicloFacturacion.Anual => ahora.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(ciclo), "Ciclo de facturación no válido.")
        };
    }
}

