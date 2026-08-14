using AutoMarket.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoMarket.Application.Interfaces
{
    public interface IAnuncioService
    {
        // 1. Crear
        Task<int> CrearAnuncioAsync(AnuncioCreateDto dto);

        // 2. Obtener por ID (opcionalmente autenticado: solo el dueño ve anuncios no publicados)
        Task<AnuncioDto?> ObtenerAnuncioPorIdAsync(int id, int? usuarioId = null);

        // 3. Obtener todos (vitrina)
        Task<IReadOnlyCollection<AnuncioListadoDto>> ObtenerTodosLosAnuncios();

        // 4. Actualizar
        Task<AnuncioUpdateDto?> ActualizarAsync(int id, int usuarioId, AnuncioUpdateDto updateAnuncio);

        // 5. Publicar
        Task<bool> PublicarAnuncioAsync(int id, int usuarioId);

        // 6. Subir Imágenes
        Task<List<string>> SubirImagenesAsync(AnuncioImagenUploadDto dto);

        // 6b. Establecer foto principal (mueve la foto al inicio de la lista)
        Task<bool> EstablecerFotoPrincipalAsync(int id, int usuarioId, string urlImagen);

        // 7. Buscar (Filtros y Paginación)
        Task<PagedResult<AnuncioListadoDto>> BuscarAnunciosAsync(AnuncioSearchDto dto);

        // 8. Cambiar estado de Anuncio
        Task<bool> CambiarEstadoAsync(int id, int usuarioId, string estado);

        // 9. Eliminar Foto
        Task EliminarImagenAsync(int anuncioId, int usuarioId, string urlImagen);
        
        // 10. Registrar vistas
        Task RegistrarVistaAsync(int anuncioId);

        // 11. Eliminar Anuncio completo
        Task<bool> EliminarAnuncioAsync(int id, int usuarioId);

        // 12. Destacados
        Task<bool> MarcarComoDestacadoAsync(int id, int usuarioId);
        Task<bool> QuitarDestacadoAsync(int id, int usuarioId);
        Task<PagedResult<AnuncioListadoDto>> ObtenerDestacadosAsync(int pagina, int tamanoPagina);
    }
}