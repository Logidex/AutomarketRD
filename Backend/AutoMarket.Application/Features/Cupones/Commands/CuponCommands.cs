using AutoMarket.Application.DTOs.Cupones;
using MediatR;

namespace AutoMarket.Application.Features.Cupones.Commands;

public record AplicarCuponCommand(int PerfilDealerId, string Codigo) : IRequest<CuponAplicadoDto>;
