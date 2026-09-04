using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class CuentasBancariasService : ICuentasBancariasService
{
    private readonly ICuentasBancariasRepository _repository;

    public CuentasBancariasService(ICuentasBancariasRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CuentaBancaria>> ObtenerCuentasActivasAsync()
    {
        return await _repository.ObtenerActivasAsync();
    }

    public async Task<IReadOnlyList<CuentaBancaria>> ObtenerTodasAsync()
    {
        return await _repository.ObtenerTodasAsync();
    }

    public async Task<CuentaBancaria> CrearCuentaAsync(
        BancoDestino banco,
        string nombreTitular,
        string numeroCuenta,
        string tipoCuenta,
        string documento,
        string conceptoReferencia)
    {
        var cuenta = new CuentaBancaria(banco, nombreTitular, numeroCuenta, tipoCuenta, documento, conceptoReferencia);

        await _repository.AgregarAsync(cuenta);

        return cuenta;
    }

    public async Task<CuentaBancaria> ActualizarCuentaAsync(
        int id,
        string nombreTitular,
        string numeroCuenta,
        string tipoCuenta,
        string documento,
        string conceptoReferencia)
    {
        var cuenta = await _repository.ObtenerPorIdAsync(id);

        if (cuenta == null)
            throw new KeyNotFoundException("Cuenta bancaria no encontrada.");

        cuenta.Actualizar(nombreTitular, numeroCuenta, tipoCuenta, documento, conceptoReferencia);

        await _repository.ActualizarAsync(cuenta);

        return cuenta;
    }

    public async Task<CuentaBancaria> ToggleCuentaAsync(int id)
    {
        var cuenta = await _repository.ObtenerPorIdAsync(id);

        if (cuenta == null)
            throw new KeyNotFoundException("Cuenta bancaria no encontrada.");

        cuenta.ToggleActiva();

        await _repository.ActualizarAsync(cuenta);

        return cuenta;
    }
}
