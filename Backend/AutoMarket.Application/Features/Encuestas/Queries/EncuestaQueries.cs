using AutoMarket.Application.DTOs.Encuestas;
using MediatR;

namespace AutoMarket.Application.Features.Encuestas.Queries;

public record ObtenerEncuestaActivaQuery(int UsuarioId) : IRequest<EncuestaActivaDto?>;
public record ObtenerResultadosEncuestaQuery(int EncuestaId) : IRequest<EncuestaResultadosDto>;
