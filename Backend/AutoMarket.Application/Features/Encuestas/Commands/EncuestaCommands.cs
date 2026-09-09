using AutoMarket.Application.DTOs.Encuestas;
using MediatR;

namespace AutoMarket.Application.Features.Encuestas.Commands;

public record ResponderEncuestaCommand(int UsuarioId, ResponderEncuestaDto Dto) : IRequest;
