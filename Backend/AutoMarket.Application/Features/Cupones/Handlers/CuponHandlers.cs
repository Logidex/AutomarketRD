using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Application.Features.Cupones.Commands;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Cupones.Handlers;

public class CuponCommandHandler
    : IRequestHandler<AplicarCuponCommand, CuponAplicadoDto>
{
    private readonly ICuponService _service;
    public CuponCommandHandler(ICuponService service) => _service = service;

    public async Task<CuponAplicadoDto> Handle(AplicarCuponCommand request, CancellationToken ct)
        => await _service.AplicarCuponAsync(request.PerfilDealerId, request.Codigo);
}
