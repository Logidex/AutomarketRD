using AutoMarket.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoMarket.Application.Interfaces
{
    public interface IAnuncioService
    {
        // 1. Crear
        Task<int> CrearAnuncioAsync(AnuncioCreateDto dto);

        // 2. Obtener por ID
        Task<AnuncioDto?> ObtenerAnuncioPorIdAsync(int id);

        // 3. Obtener todos (vitrina)
        Task<IReadOnlyCollection<AnuncioListadoDto>> ObtenerTodosLosAnuncios();

        // 4. Actualizar
        Task<AnuncioUpdateDto?> ActualizarAsync(int id, int usuarioId, AnuncioUpdateDto updateAnuncio);

        // 5. Publicar
        Task<bool> PublicarAnuncioAsync(int id, int usuarioId);

        // 6. Subir Imágenes
        Task SubirImagenesAsync(AnuncioImagenUploadDto dto);

        // 7. Buscar (Filtros y Paginación)
        Task<PagedResult<AnuncioListadoDto>> BuscarAnunciosAsync(AnuncioSearchDto dto);

        // 8. Cambiar estado de Anuncio
        Task<bool> CambiarEstadoAsync(int id, int usuarioId, string estado);

        // 9. Eliminar Foto
        Task EliminarImagenAsync(int anuncioId, int usuarioId, string urlImagen);
        
        // 10. Registrar vistas
        Task RegistrarVistaAsync(int anuncioId);
    }
}