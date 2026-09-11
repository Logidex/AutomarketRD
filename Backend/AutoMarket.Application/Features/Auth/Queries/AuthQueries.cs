using AutoMarket.Application.DTOs.Usuario;
using MediatR;

namespace AutoMarket.Application.Features.Auth.Queries;

public record ObtenerCuentaQuery(int UsuarioId)
    : IRequest<UsuarioCuentaDto>;
