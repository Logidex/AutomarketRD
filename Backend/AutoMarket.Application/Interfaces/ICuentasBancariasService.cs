using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Application.Interfaces;

public interface ICuentasBancariasService
{
    Task<IReadOnlyList<CuentaBancaria>> ObtenerCuentasActivasAsync();
    Task<IReadOnlyList<CuentaBancaria>> ObtenerTodasAsync();
    Task<CuentaBancaria> CrearCuentaAsync(BancoDestino banco, string nombreTitular, string numeroCuenta, string tipoCuenta, string documento, string conceptoReferencia);
    Task<CuentaBancaria> ActualizarCuentaAsync(int id, string nombreTitular, string numeroCuenta, string tipoCuenta, string documento, string conceptoReferencia);
    Task<CuentaBancaria> ToggleCuentaAsync(int id);
}
