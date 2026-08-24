using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ICuponRepository
{
    /// <summary>Obtiene el cupón por código (case-insensitive, ya normalizado en la entidad).</summary>
    Task<Cupon?> ObtenerPorCodigoAsync(string codigo);

    /// <summary>Inserta un cupón nuevo (usado por el seeder).</summary>
    Task<Cupon> AgregarAsync(Cupon cupon);

    /// <summary>Indica si el dealer ya canjeó ese cupón.</summary>
    Task<bool> ExisteRedencionAsync(int cuponId, int perfilDealerId);

    /// <summary>
    /// Persiste el canje completo en una sola transacción: el incremento de
    /// usos del cupón, la redención nueva y cualquier cambio rastreado en la
    /// suscripción del dealer (mismo DbContext con scope compartido).
    /// </summary>
    Task GuardarCanjeAsync(Cupon cupon, CuponRedencion redencion);
}
