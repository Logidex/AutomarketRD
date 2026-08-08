import api from './api';

export interface SuscripcionDealer {
  perfilDealerId: number;
  nivel: string;
  ciclo: string;
  estado: string;
  limiteAnuncios: number;
  fechaInicioUtc: string;
  fechaVencimientoUtc: string;
  diasRestantes: number;
  activa: boolean;
}

export const suscripcionService = {
  async obtenerSuscripcion(): Promise<SuscripcionDealer> {
    const response = await api.get<SuscripcionDealer>('/api/dealers/me/suscripcion');
    return response.data;
  },

  async cancelarSuscripcion(): Promise<void> {
    await api.post('/api/dealers/me/suscripcion/cancelar');
  },
};