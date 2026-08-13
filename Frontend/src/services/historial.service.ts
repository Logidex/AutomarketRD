import api from "./api";

export interface AnuncioReciente {
  id: number;
  marca: string;
  modelo: string;
  anio: number;
  precio: number;
  moneda: string;
  fotoPrincipal: string | null;
  vistoEnUtc: string;
}

export const historialService = {
  async registrarVista(anuncioId: number): Promise<void> {
    await api.post(`/api/historial/${anuncioId}`);
  },

  async obtenerRecientes(cantidad = 12): Promise<AnuncioReciente[]> {
    const response = await api.get<AnuncioReciente[]>(
      "/api/historial/recientes",
      { params: { cantidad } },
    );
    return response.data;
  },
};