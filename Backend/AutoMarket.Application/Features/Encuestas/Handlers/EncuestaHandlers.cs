using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.Features.Encuestas.Commands;
using AutoMarket.Application.Features.Encuestas.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;

namespace AutoMarket.Application.Features.Encuestas.Handlers;

public class EncuestaCommandHandler
    : IRequestHandler<ResponderEncuestaCommand>
{
    private readonly IEncuestaService _service;
    public EncuestaCommandHandler(IEncuestaService service) => _service = service;

    public async Task Handle(ResponderEncuestaCommand request, CancellationToken ct)
        => await _service.ResponderAsync(request.UsuarioId, request.Dto);
}

public class EncuestaQueryHandler
    : IRequestHandler<ObtenerEncuestaActivaQuery, EncuestaActivaDto?>,
      IRequestHandler<ObtenerResultadosEncuestaQuery, EncuestaResultadosDto>
{
    private readonly IEncuestaService _service;
    public EncuestaQueryHandler(IEncuestaService service) => _service = service;

    public async Task<EncuestaActivaDto?> Handle(ObtenerEncuestaActivaQuery request, CancellationToken ct)
        => await _service.ObtenerActivaAsync(request.UsuarioId);

    public async Task<EncuestaResultadosDto> Handle(ObtenerResultadosEncuestaQuery request, CancellationToken ct)
        => await _service.ObtenerResultadosAsync(request.EncuestaId);
}
