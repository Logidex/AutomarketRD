using AutoMarket.Core.Entities;
using MediatR;

namespace AutoMarket.Application.Features.CuentasBancarias.Queries;

public record ObtenerCuentasActivasQuery() : IRequest<IReadOnlyList<CuentaBancaria>>;
public record ObtenerTodasCuentasQuery() : IRequest<IReadOnlyList<CuentaBancaria>>;
