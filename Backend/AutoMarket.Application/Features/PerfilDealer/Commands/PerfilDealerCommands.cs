using AutoMarket.Application.DTOs.Usuario;
using MediatR;

namespace AutoMarket.Application.Features.PerfilDealer.Commands;

public record ActualizarMiPerfilCommand(int DealerId, PerfilDealerUpdateDto Dto) : IRequest<PerfilDealerPublicoDto?>;
