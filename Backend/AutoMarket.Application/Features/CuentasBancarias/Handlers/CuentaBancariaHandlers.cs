using AutoMarket.Core.Entities;
using AutoMarket.Application.Features.CuentasBancarias.Commands;
using AutoMarket.Application.Features.CuentasBancarias.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.CuentasBancarias.Handlers;

public class CuentaBancariaCommandHandler
    : IRequestHandler<CrearCuentaBancariaCommand, CuentaBancaria>,
      IRequestHandler<ActualizarCuentaBancariaCommand, CuentaBancaria>,
      IRequestHandler<ToggleCuentaBancariaCommand, CuentaBancaria>
{
    private readonly ICuentasBancariasService _service;
    public CuentaBancariaCommandHandler(ICuentasBancariasService service) => _service = service;

    public async Task<CuentaBancaria> Handle(CrearCuentaBancariaCommand request, CancellationToken ct)
        => await _service.CrearCuentaAsync(request.Banco, request.NombreTitular, request.NumeroCuenta, request.TipoCuenta, request.Documento, request.ConceptoReferencia);

    public async Task<CuentaBancaria> Handle(ActualizarCuentaBancariaCommand request, CancellationToken ct)
        => await _service.ActualizarCuentaAsync(request.Id, request.NombreTitular, request.NumeroCuenta, request.TipoCuenta, request.Documento, request.ConceptoReferencia);

    public async Task<CuentaBancaria> Handle(ToggleCuentaBancariaCommand request, CancellationToken ct)
        => await _service.ToggleCuentaAsync(request.Id);
}

public class CuentaBancariaQueryHandler
    : IRequestHandler<ObtenerCuentasActivasQuery, IReadOnlyList<CuentaBancaria>>,
      IRequestHandler<ObtenerTodasCuentasQuery, IReadOnlyList<CuentaBancaria>>
{
    private readonly ICuentasBancariasService _service;
    public CuentaBancariaQueryHandler(ICuentasBancariasService service) => _service = service;

    public async Task<IReadOnlyList<CuentaBancaria>> Handle(ObtenerCuentasActivasQuery request, CancellationToken ct)
        => await _service.ObtenerCuentasActivasAsync();

    public async Task<IReadOnlyList<CuentaBancaria>> Handle(ObtenerTodasCuentasQuery request, CancellationToken ct)
        => await _service.ObtenerTodasAsync();
}
