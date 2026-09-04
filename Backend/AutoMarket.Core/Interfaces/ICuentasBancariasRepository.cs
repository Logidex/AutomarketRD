using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface ICuentasBancariasRepository
{
    Task<IReadOnlyList<CuentaBancaria>> ObtenerActivasAsync();
    Task<IReadOnlyList<CuentaBancaria>> ObtenerTodasAsync();
    Task<CuentaBancaria?> ObtenerPorIdAsync(int id);
    Task AgregarAsync(CuentaBancaria cuenta);
    Task ActualizarAsync(CuentaBancaria cuenta);
}
