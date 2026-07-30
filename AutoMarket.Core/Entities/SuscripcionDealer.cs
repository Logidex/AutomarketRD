using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

public class SuscripcionDealer
{
    // ==========================================
    // 1. EL ENLACE (Identidades y Relación Adaptada)
    // ==========================================
    public int Id { get; private set; } // Cambiado a int para alinearse a tu estrategia de IDs
    
    // Clave foránea que apunta exactamente al UsuarioId / PerfilDealer de tipo int
    public int PerfilDealerId { get; private set; } 
    
    public virtual PerfilDealer PerfilDealer { get; private set; } = null!;

    // ==========================================
    // 2. EL CONTRATO (Los Enums y Límites)
    // ==========================================
    public PlanNivel Nivel { get; private set; }
    public CicloFacturacion Ciclo { get; private set; }
    public EstadoSuscripcion Estado { get; private set; }

    public int LimiteAnuncios => (int)Nivel;

    // ==========================================
    // 3. EL RELOJ (Control de Tiempo)
    // ==========================================
    public DateTime FechaInicioUtc { get; private set; }
    public DateTime FechaVencimientoUtc { get; private set; }

    // Constructor para Entity Framework Core
    private SuscripcionDealer() { }

    // Constructor de dominio adaptado con int
    public SuscripcionDealer(int perfilDealerId, PlanNivel nivel, CicloFacturacion ciclo)
    {
        PerfilDealerId = perfilDealerId;
        Nivel = nivel;
        Ciclo = ciclo;
        Estado = EstadoSuscripcion.Activa;
        FechaInicioUtc = DateTime.UtcNow;
        
        FechaVencimientoUtc = ciclo switch
        {
            CicloFacturacion.Mensual => DateTime.UtcNow.AddMonths(1),
            CicloFacturacion.Trimestral => DateTime.UtcNow.AddMonths(3),
            CicloFacturacion.Anual => DateTime.UtcNow.AddYears(1),
            _ => DateTime.UtcNow.AddMonths(1)
        };
    }

    // Regla del Dominio
    public bool PermiteNuevosAnuncios(int cantidadAnunciosActuales)
    {
        if (Estado != EstadoSuscripcion.Activa) return false;
        if (DateTime.UtcNow > FechaVencimientoUtc) return false;
        return cantidadAnunciosActuales < LimiteAnuncios;
    }

    public void ActualizarNivel(PlanNivel nuevoNivel)
    {
        if (Estado == EstadoSuscripcion.Cancelada)
        {
            throw new InvalidOperationException("Imposible mutar: La suscripción actual se encuentra cancelada.");
        }

        Nivel = nuevoNivel;

    }

    // ==========================================
    // 4. RENOVACIÓN MANUAL (BACKOFFICE)
    // ==========================================
    public void RenovarManualmente(DateTime nuevaFechaVencimiento)
    {
        if (nuevaFechaVencimiento <= DateTime.UtcNow)
        {
            throw new ArgumentException("La nueva fecha de vencimiento debe ser en el futuro.");
        }

        FechaVencimientoUtc = nuevaFechaVencimiento;
        Estado = EstadoSuscripcion.Activa; // Revive al moroso automáticamente
    }
}

