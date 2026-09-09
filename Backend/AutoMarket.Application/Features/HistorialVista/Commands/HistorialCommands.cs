using MediatR;

namespace AutoMarket.Application.Features.HistorialVista.Commands;

public record RegistrarVistaCommand(int UsuarioId, int AnuncioId) : IRequest;
