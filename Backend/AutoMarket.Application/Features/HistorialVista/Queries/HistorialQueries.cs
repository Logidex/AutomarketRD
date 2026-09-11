using AutoMarket.Application.DTOs.Historial;
using MediatR;

namespace AutoMarket.Application.Features.HistorialVista.Queries;

public record ObtenerRecientesQuery(int UsuarioId, int Cantidad) : IRequest<IReadOnlyCollection<AnuncioRecienteDto>>;
