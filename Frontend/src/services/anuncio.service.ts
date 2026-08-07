import api from "./api";
import type {
  AnuncioListado,
  PagedResult,
  AnuncioCreateRequestDto,
  AnuncioDetalle,
} from "../types/anuncio.types";

export const anuncioService = {
  async obtenerMisAnuncios(
    usuarioId: number,
  ): Promise<PagedResult<AnuncioListado>> {
    const timestamp = new Date().getTime();

    const response = await api.get<PagedResult<AnuncioListado>>(
      `/api/anuncios/buscar?UsuarioId=${usuarioId}&PaginaActual=1&CantidadAnuncios=50&_t=${timestamp}`,
    );

    return response.data;
  },

  async publicarAnuncio(id: number): Promise<{ mensaje: string }> {
    const response = await api.patch<{ mensaje: string }>(
      `/api/anuncios/${id}/publicar`,
    );
    return response.data;
  },

  async cambiarEstado(
    id: number,
    estado: string,
  ): Promise<{ mensaje: string }> {
    const response = await api.patch<{ mensaje: string }>(
      `/api/anuncios/${id}/estado`,
      { estado },
    );

    return response.data;
  },

  async crearAnuncio(
    dto: AnuncioCreateRequestDto,
  ): Promise<{ mensaje: string; id: number }> {
    const response = await api.post<{ mensaje: string; id: number }>(
      "/api/anuncios",
      dto,
    );
    return response.data;
  },

  async subirImagenes(id: number, imagenes: File[]): Promise<void> {
    const formData = new FormData();

    imagenes.forEach((img) => {
      formData.append("imagenes", img);
    });

    await api.post(`/api/anuncios/${id}/imagenes`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
  },

  async obtenerPorId(id: string): Promise<AnuncioDetalle> {
    const response = await api.get<AnuncioDetalle>(`/api/anuncios/${id}`);
    return response.data;
  },

  async actualizarAnuncio(
    id: string,
    dto: AnuncioCreateRequestDto,
  ): Promise<{ mensaje: string }> {
    const response = await api.put<{ mensaje: string }>(
      `/api/anuncios/${id}`,
      dto,
    );
    return response.data;
  },

  async eliminarImagen(
    id: number,
    urlImagen: string,
  ): Promise<{ mensaje: string }> {
    const response = await api.delete<{ mensaje: string }>(
      `/api/anuncios/${id}/imagenes`,
      {
        data: { urlImagen }, // El truco de Axios para el DELETE
      },
    );
    return response.data;
  },

  async registrarVista(id: number): Promise<void> {
    await api.post(`/api/anuncios/${id}/registrar-vista`);
  },
};
