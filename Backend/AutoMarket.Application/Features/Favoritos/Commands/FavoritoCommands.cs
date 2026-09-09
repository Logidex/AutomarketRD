using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Application.DTOs.Encuestas;
using AutoMarket.Application.DTOs.Favorito;
using AutoMarket.Application.DTOs.Historial;
using MediatR;

namespace AutoMarket.Application.Features.Favoritos.Commands;

public record AgregarFavoritoCommand(int UsuarioId, int AnuncioId) : IRequest;
public record QuitarFavoritoCommand(int UsuarioId, int AnuncioId) : IRequest;
