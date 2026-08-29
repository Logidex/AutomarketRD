import api from './api';

export interface SuscripcionVendedor {
  nivel: string;
  ciclo: string;
  estado: string;
  limiteAnuncios: number;
  cuotaDestacados: number;
  maxFotos: number;
  fechaInicioUtc: string;
  fechaVencimientoUtc: string;
  diasRestantes: number;
  activa: boolean;
}

export const vendedorService = {
  async obtenerSuscripcion(): Promise<SuscripcionVendedor> {
    const response = await api.get<SuscripcionVendedor>('/api/vendedores/me/suscripcion');
    return response.data;
  },

  async obtenerMisAnuncios(): Promise<unknown[]> {
    const response = await api.get<unknown[]>('/api/vendedores/me/anuncios');
    return response.data;
  },
};
