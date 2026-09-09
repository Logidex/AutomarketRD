using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using MediatR;

namespace AutoMarket.Application.Features.CuentasBancarias.Commands;

public record CrearCuentaBancariaCommand(
    BancoDestino Banco, string NombreTitular, string NumeroCuenta,
    string TipoCuenta, string Documento, string ConceptoReferencia) : IRequest<CuentaBancaria>;

public record ActualizarCuentaBancariaCommand(
    int Id, string NombreTitular, string NumeroCuenta,
    string TipoCuenta, string Documento, string ConceptoReferencia) : IRequest<CuentaBancaria>;

public record ToggleCuentaBancariaCommand(int Id) : IRequest<CuentaBancaria>;
