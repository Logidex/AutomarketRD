using AutoMarket.Application.DTOs;
using System.Threading.Tasks;

namespace AutoMarket.Application.Interfaces
{
    public interface IAnuncioService
    {
        Task<PagedResult<AnuncioListadoDto>> BuscarAnunciosAsync(AnuncioSearchDto dto);
    }
}