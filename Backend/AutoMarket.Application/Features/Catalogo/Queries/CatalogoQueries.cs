using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Anuncio;
using MediatR;

namespace AutoMarket.Application.Features.Catalogo.Queries;

public record ObtenerCatalogoPaginadoQuery(int Pagina, int TamanoPagina) : IRequest<PagedResult<AnuncioCatalogoDto>>;
