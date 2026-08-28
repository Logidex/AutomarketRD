using System.ComponentModel.DataAnnotations;

using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Core.Entities;

/// <summary>
/// Cupón promocional canjeable por dealers: otorga <see cref="Dias"/> días del
/// <see cref="Nivel"/> indicado, con un tope total de canjes (<see cref="MaximoUsos"/>)
/// y un canje único por dealer (índice único en <see cref="CuponRedencion"/>).
/// </summary>
public class Cupon
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = null!;
    public PlanNivel Nivel { get; private set; }
    public int Dias { get; private set; }
    public int MaximoUsos { get; private set; }

    /// <summary>
    /// Token de concurrencia optimista: si dos canjes simultáneos compiten por
    /// el último uso disponible, el segundo falla al guardar (DbUpdateConcurrencyException)
    /// y nunca se supera el tope.
    /// </summary>
    [ConcurrencyCheck]
    public int UsosActuales { get; private set; }

    public bool Activo { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }

    public virtual List<CuponRedencion> Redenciones { get; private set; } = new();

    private Cupon() { }

    public Cupon(string codigo, PlanNivel nivel, int dias, int maximoUsos)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cupón es obligatorio.", nameof(codigo));

        if (dias <= 0)
            throw new ArgumentException("Los días del cupón deben ser positivos.", nameof(dias));

        if (maximoUsos <= 0)
            throw new ArgumentException("El máximo de usos debe ser positivo.", nameof(maximoUsos));

        Codigo = codigo.Trim().ToUpperInvariant();
        Nivel = nivel;
        Dias = dias;
        MaximoUsos = maximoUsos;
        UsosActuales = 0;
        Activo = true;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    /// <summary>Indica si el cupón aún acepta canjes.</summary>
    public bool TieneCupoDisponible => UsosActuales < MaximoUsos;

    /// <summary>
    /// Consume un uso del cupón. El incremento viaja con concurrencia optimista
    /// (token en BD): si dos canjes compiten por el último uso, el segundo
    /// falla al guardar y nunca se pasa del tope.
    /// </summary>
    public void RegistrarUso()
    {
        if (!Activo)
            throw new BusinessRuleException("El cupón está desactivado.");

        if (!TieneCupoDisponible)
            throw new BusinessRuleException(
                $"El cupón alcanzó su límite de {MaximoUsos} usos.");

        UsosActuales++;
    }

    /// <summary>Desactiva el cupón sin eliminar su historial de redenciones.</summary>
    public void Desactivar()
    {
        Activo = false;
    }
}
