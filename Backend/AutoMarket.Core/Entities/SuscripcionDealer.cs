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

    public int LimiteAnuncios => Plan?.LimiteAnunciosEfectivo ?? PlanConfig.LimiteAnuncios(Nivel);

    // Indica si el plan es Gratis (ilimitado en tiempo, 1 anuncio renovable mensualmente)
    public bool EsGratis => Nivel == PlanNivel.Gratis;

    // Navigation to PlanCatalogo for CuotaDestacados
    public int? PlanCatalogoId { get; private set; }
    public virtual PlanCatalogo? Plan { get; private set; }

    public DateTime FechaInicioUtc { get; private set; }
    public DateTime FechaVencimientoUtc { get; private set; }
    public DateTime? FechaRecordatorioEnviadoUtc { get; private set; }

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

    /// <summary>
    /// Vincula el plan del catálogo que define la cuota de destacados y demás
    /// límites configurables. Es una referencia informativa: la cuota vigente
    /// se lee desde <see cref="Plan"/> (o desde el nivel como respaldo).
    /// </summary>
    public void VincularPlanCatalogo(PlanCatalogo? plan)
    {
        if (plan == null) return;

        PlanCatalogoId = plan.Id;
    }

    public bool PermiteNuevosAnuncios(int cantidadAnunciosActuales, PlanCatalogo? plan = null)
    {
        // Plan Gratis: permite 1 anuncio renovable mensualmente
        // No expira la suscripción, solo controla anuncios activos
        if (EsGratis)
        {
            return cantidadAnunciosActuales < 1;
        }

        // Planes pagos: vencen según FechaVencimientoUtc
        if (DateTime.UtcNow > FechaVencimientoUtc) return false;

        var limite = Plan?.LimiteAnunciosEfectivo ?? PlanConfig.LimiteAnuncios(Nivel);
        return cantidadAnunciosActuales < limite;
    }

    /// <summary>
    /// Verifica si la suscripción permite destacar más anuncios.
    /// Requiere el plan del catálogo para obtener la cuota.
    /// </summary>
    public bool PermiteDestacarMas(int destacadosActuales, PlanCatalogo plan)
    {
        // Plan Gratis no permite destacados
        if (EsGratis) return false;

        if (DateTime.UtcNow > FechaVencimientoUtc) return false;
        if (Estado != EstadoSuscripcion.Activa) return false;
        if (plan == null) return false;

        return destacadosActuales < plan.CuotaDestacados;
    }

    /// <summary>
    /// Indica si la suscripción está activa.
    /// Para plan Gratis: siempre activa (no vence).
    /// Para planes pagos: activa mientras no haya vencido.
    /// </summary>
    public bool Activa => EsGratis || (Estado == EstadoSuscripcion.Activa && DateTime.UtcNow <= FechaVencimientoUtc);

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
        FechaRecordatorioEnviadoUtc = null;
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
        FechaRecordatorioEnviadoUtc = null;
    }

    /// <summary>Marca la suscripción como cancelada.</summary>
    public void Cancelar()
    {
        if (Estado == EstadoSuscripcion.Cancelada)
        {
            throw new InvalidOperationException(
                "La suscripción ya se encuentra cancelada.");
        }

        Estado = EstadoSuscripcion.Cancelada;
    }

    /// <summary>
    /// Reactiva una suscripción cancelada asignando el plan y ciclo elegidos
    /// con una nueva vigencia desde ahora.
    /// </summary>
    public void ActivarConPlan(PlanNivel nuevoNivel, CicloFacturacion nuevoCiclo)
    {
        Nivel = nuevoNivel;
        Ciclo = nuevoCiclo;
        Estado = EstadoSuscripcion.Activa;
        FechaInicioUtc = DateTime.UtcNow;
        FechaVencimientoUtc = CalcularFechaVencimiento(nuevoCiclo);
        FechaRecordatorioEnviadoUtc = null;
    }

    /// <summary>Registra que ya se envió el recordatorio del vencimiento actual.</summary>
    public void MarcarRecordatorioEnviado()
    {
        FechaRecordatorioEnviadoUtc = DateTime.UtcNow;
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

