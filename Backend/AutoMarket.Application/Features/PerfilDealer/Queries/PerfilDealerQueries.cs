using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Dealer;
using AutoMarket.Application.DTOs.Usuario;
using MediatR;

namespace AutoMarket.Application.Features.PerfilDealer.Queries;

public record ObtenerPerfilPublicoQuery(int DealerId) : IRequest<PerfilDealerPublicoDto?>;

public record ListarAgenciasQuery(
    string? Busqueda, bool? SoloVerificadas, string? PlanNivel,
    int Pagina, int CantidadPorPagina) : IRequest<PagedResult<AgenciaListadoDto>>;
