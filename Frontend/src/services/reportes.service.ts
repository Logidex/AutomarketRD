import api from "./api";
import type { CrearReportePublicoDto } from "./admin.service";

export const reportesService = {
  // Público y anónimo: cualquier visitante puede reportar un anuncio.
  async reportar(datos: CrearReportePublicoDto): Promise<{ exito: boolean; mensaje: string }> {
    const response = await api.post<{ exito: boolean; mensaje: string }>(
      "/api/reportes",
      datos,
    );
    return response.data;
  },
};
