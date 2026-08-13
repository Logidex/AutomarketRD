import api from "./api";

export interface AnuncioFavorito {
  id: number;
  marca: string;
  modelo: string;
  anio: number;
  precio: number;
  moneda: string;
  fotoPrincipal: string | null;
}

export const favoritoService = {
  async obtenerMisFavoritos(): Promise<AnuncioFavorito[]> {
    const response = await api.get<AnuncioFavorito[]>("/api/favoritos");
    return response.data;
  },

  async agregar(anuncioId: number): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      `/api/favoritos/${anuncioId}`,
    );
    return response.data;
  },

  async quitar(anuncioId: number): Promise<{ mensaje: string }> {
    const response = await api.delete<{ mensaje: string }>(
      `/api/favoritos/${anuncioId}`,
    );
    return response.data;
  },
};