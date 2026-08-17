using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Dealer;
using AutoMarket.Application.DTOs.Usuario;

namespace AutoMarket.Application.Interfaces;

public interface IPerfilDealerService
{
    Task<PerfilDealerPublicoDto?> ObtenerPerfilPublicoAsync(int dealerId);

    Task<PagedResult<AgenciaListadoDto>> ListarAgenciasAsync(
        string? busqueda,
        bool? soloVerificadas,
        string? planNivel,
        int pagina,
        int cantidadPorPagina);

    Task<PerfilDealerPublicoDto?> ActualizarMiPerfilAsync(
        int dealerId,
        PerfilDealerUpdateDto dto);
}