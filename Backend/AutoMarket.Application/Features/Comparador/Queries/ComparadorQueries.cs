using AutoMarket.Application.DTOs.Anuncio;
using MediatR;

namespace AutoMarket.Application.Features.Comparador.Queries;

public record CompararVehiculosQuery(int[] Ids) : IRequest<IEnumerable<AnuncioComparadorDto>>;
