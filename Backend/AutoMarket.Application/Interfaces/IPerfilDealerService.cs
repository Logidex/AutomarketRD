using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IPerfilDealerService
{
    Task<PerfilDealerPublicoDto?> ObtenerPerfilPublicoAsync(int dealerId);

    Task<PerfilDealerPublicoDto?> ActualizarMiPerfilAsync(
        int dealerId,
        PerfilDealerUpdateDto dto);
}