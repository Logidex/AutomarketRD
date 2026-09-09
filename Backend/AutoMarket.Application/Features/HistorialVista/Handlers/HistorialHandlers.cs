using AutoMarket.Application.DTOs.Historial;
using AutoMarket.Application.Features.HistorialVista.Commands;
using AutoMarket.Application.Features.HistorialVista.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.HistorialVista.Handlers;

public class HistorialCommandHandler
    : IRequestHandler<RegistrarVistaCommand>
{
    private readonly IHistorialVistaService _service;
    public HistorialCommandHandler(IHistorialVistaService service) => _service = service;

    public async Task Handle(RegistrarVistaCommand request, CancellationToken ct)
        => await _service.RegistrarVistaAsync(request.UsuarioId, request.AnuncioId);
}

public class HistorialQueryHandler
    : IRequestHandler<ObtenerRecientesQuery, IReadOnlyCollection<AnuncioRecienteDto>>
{
    private readonly IHistorialVistaService _service;
    public HistorialQueryHandler(IHistorialVistaService service) => _service = service;

    public async Task<IReadOnlyCollection<AnuncioRecienteDto>> Handle(ObtenerRecientesQuery request, CancellationToken ct)
        => await _service.ObtenerRecientesAsync(request.UsuarioId, request.Cantidad);
}
