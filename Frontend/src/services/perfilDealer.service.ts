import api from "./api";

export interface PerfilDealer {
  id: number;
  nombreAgencia: string;
  logoUrl: string | null;
  horarios: string | null;
  ubicacion: string;
  telefonoAgencia: string;
  descripcion: string;
  whatsApp: string | null;
}

export interface PerfilDealerUpdate {
  nombreAgencia: string;
  ubicacion: string;
  telefonoAgencia: string;
  horarios: string;
  descripcion: string;
  whatsApp: string;
  logo?: File;
}

export const perfilDealerService = {
  async obtenerPerfil(usuarioId: number): Promise<PerfilDealer> {
    const response = await api.get<PerfilDealer>(`/api/dealers/${usuarioId}`);
    return response.data;
  },

  async actualizarPerfil(dto: PerfilDealerUpdate): Promise<PerfilDealer> {
    const formData = new FormData();

    formData.append("nombreAgencia", dto.nombreAgencia);
    formData.append("ubicacion", dto.ubicacion);
    formData.append("telefonoAgencia", dto.telefonoAgencia);
    formData.append("horarios", dto.horarios);
    formData.append("descripcion", dto.descripcion);
    formData.append("whatsApp", dto.whatsApp);

    if (dto.logo) {
      formData.append("logo", dto.logo);
    }

    const response = await api.put<PerfilDealer>("/api/dealers/me", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return response.data;
  },
};