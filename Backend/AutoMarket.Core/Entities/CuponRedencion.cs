namespace AutoMarket.Core.Entities;

/// <summary>
/// Registro de un canje de cupón por un dealer. El índice único
/// (CuponId, PerfilDealerId) garantiza un solo canje por dealer a nivel de BD.
/// </summary>
public class CuponRedencion
{
    public int Id { get; private set; }
    public int CuponId { get; private set; }
    public virtual Cupon Cupon { get; private set; } = null!;

    /// <summary>Dealer que canjeó (PK de PerfilDealer = UsuarioId).</summary>
    public int PerfilDealerId { get; private set; }
    public virtual PerfilDealer PerfilDealer { get; private set; } = null!;

    public DateTime FechaUtc { get; private set; }

    private CuponRedencion() { }

    public CuponRedencion(int cuponId, int perfilDealerId)
    {
        // Nota: cuponId puede ser 0 si el cupón aún no fue insertado (la BD
        // asigna el Id); la FK de RedencionesCupon protege la integridad real.
        if (perfilDealerId <= 0)
            throw new ArgumentException("El dealer es inválido.", nameof(perfilDealerId));

        CuponId = cuponId;
        PerfilDealerId = perfilDealerId;
        FechaUtc = DateTime.UtcNow;
    }
}
