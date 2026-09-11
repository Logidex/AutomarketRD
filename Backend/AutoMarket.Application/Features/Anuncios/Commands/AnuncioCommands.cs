using AutoMarket.Application.DTOs;
using MediatR;

namespace AutoMarket.Application.Features.Anuncios.Commands;

public record CrearAnuncioCommand : IRequest<int>
{
    public AnuncioCreateDto Dto { get; init; } = default!;
}

public record ActualizarAnuncioCommand : IRequest<AnuncioUpdateDto?>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
    public AnuncioUpdateDto Dto { get; init; } = default!;
}

public record PublicarAnuncioCommand : IRequest<bool>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
}

public record CambiarEstadoAnuncioCommand : IRequest<bool>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
    public string Estado { get; init; } = default!;
}

public record EliminarAnuncioCommand : IRequest<bool>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
}

public record RenovarAnuncioGratisCommand : IRequest<bool>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
}

public record MarcarDestacadoCommand : IRequest<bool>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
}

public record QuitarDestacadoCommand : IRequest<bool>
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
}

public record RegistrarVistaCommand : IRequest
{
    public int AnuncioId { get; init; }
}
