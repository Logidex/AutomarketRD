import api from "./api";

export interface PerfilDealerPublico {
  id: number;
  nombreAgencia: string;
  logoUrl?: string | null;
  horarios?: string | null;
  ubicacion: string;
  telefonoAgencia: string;
  descripcion: string;
  whatsApp?: string | null;
  esVendedorParticular: boolean;
}

export const dealerService = {
  async obtenerPerfilPublico(
    dealerId: number
  ): Promise<PerfilDealerPublico> {
    const response = await api.get<PerfilDealerPublico>(
      `/api/dealers/${dealerId}`
    );
    return response.data;
  },
};