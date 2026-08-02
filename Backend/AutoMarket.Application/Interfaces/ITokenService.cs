using AutoMarket.Core.Entities;

namespace AutoMarket.Application.Interfaces;
public interface ITokenService
{
    string GenerarToken(Usuario usuario);
}