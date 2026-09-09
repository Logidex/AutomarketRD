using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Vendedores.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Vendedores.Handlers;

public class VendedorQueryHandler
    : IRequestHandler<ObtenerPerfilPublicoVendedorQuery, VendedorPerfilPublicoDto?>,
      IRequestHandler<ObtenerAnunciosVendedorQuery, IReadOnlyCollection<AnuncioListadoDto>>,
      IRequestHandler<ObtenerMiSuscripcionQuery, VendedorSuscripcionDto?>
{
    private readonly IVendedorService _vendedorService;

    public VendedorQueryHandler(IVendedorService vendedorService)
    {
        _vendedorService = vendedorService;
    }

    public async Task<VendedorPerfilPublicoDto?> Handle(
        ObtenerPerfilPublicoVendedorQuery request, CancellationToken ct)
        => await _vendedorService.ObtenerPerfilPublicoAsync(request.VendedorId);

    public async Task<IReadOnlyCollection<AnuncioListadoDto>> Handle(
        ObtenerAnunciosVendedorQuery request, CancellationToken ct)
        => await _vendedorService.ObtenerAnunciosDelVendedorAsync(request.VendedorId);

    public async Task<VendedorSuscripcionDto?> Handle(
        ObtenerMiSuscripcionQuery request, CancellationToken ct)
        => await _vendedorService.ObtenerMiSuscripcionAsync(request.UsuarioId);
}
