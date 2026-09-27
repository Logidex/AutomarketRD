import api from "./api";

export const contadorService = {
  async incrementar(): Promise<number> {
    const response = await api.post<{ totalVisitas: number }>(
      "/api/visit-counter",
    );
    return response.data.totalVisitas;
  },

  async obtener(): Promise<number> {
    const response = await api.get<{ totalVisitas: number }>(
      "/api/visit-counter",
    );
    return response.data.totalVisitas;
  },
};
