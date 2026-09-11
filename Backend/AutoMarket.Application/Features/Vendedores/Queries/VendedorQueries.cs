using AutoMarket.Application.DTOs;
using MediatR;

namespace AutoMarket.Application.Features.Vendedores.Queries;

public record ObtenerPerfilPublicoVendedorQuery(int VendedorId)
    : IRequest<VendedorPerfilPublicoDto?>;

public record ObtenerAnunciosVendedorQuery(int VendedorId)
    : IRequest<IReadOnlyCollection<AnuncioListadoDto>>;

public record ObtenerMiSuscripcionQuery(int UsuarioId)
    : IRequest<VendedorSuscripcionDto?>;
