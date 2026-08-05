import api from './api';
import type { AnuncioListado, PagedResult, AnuncioCreateDto } from '../types/anuncio.types';

export const anuncioService = {
  async obtenerMisAnuncios(usuarioId: number): Promise<PagedResult<AnuncioListado>> {
    // Añadimos un timestamp para evitar que el navegador guarde la respuesta en caché
    const timestamp = new Date().getTime();
    
    // También subí la cantidad a 50 para asegurarnos de que el nuevo anuncio no se quede en la página 2
    const response = await api.get<PagedResult<AnuncioListado>>(
      `/api/anuncios/buscar?UsuarioId=${usuarioId}&PaginaActual=1&CantidadAnuncios=50&_t=${timestamp}`
    );
    return response.data;
  },

  async publicarAnuncio(id: number): Promise<{ mensaje: string }> {
    const response = await api.patch<{ mensaje: string }>(`/api/anuncios/${id}/publicar`);
    return response.data;
  },

  async crearAnuncio(dto: AnuncioCreateDto): Promise<{ mensaje: string, id: number }> {
    const response = await api.post<{ mensaje: string, id: number }>('/api/anuncios', dto);
    return response.data;
  },

  async subirImagenes(id: number, imagenes: File[]): Promise<void> {
    const formData = new FormData();
    imagenes.forEach(img => {
        formData.append('imagenes', img);
    });

    await api.post(`/api/anuncios/${id}/imagenes`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
};