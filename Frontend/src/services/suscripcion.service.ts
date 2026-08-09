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

export interface PagoSuscripcion {
  id: number;
  perfilDealerId: number;
  nivel: string;
  ciclo: string;
  estado: string;
  monto: number;
  moneda: string;
  ordenIdPayPal?: string | null;
  referencia?: string | null;
  fechaUtc: string;
}

export const suscripcionService = {
  async obtenerSuscripcion(): Promise<SuscripcionDealer> {
    const response = await api.get<SuscripcionDealer>('/api/dealers/me/suscripcion');
    return response.data;
  },

  async cancelarSuscripcion(): Promise<void> {
    await api.post('/api/dealers/me/suscripcion/cancelar');
  },

  async obtenerHistorialPagos(): Promise<PagoSuscripcion[]> {
    const response = await api.get<PagoSuscripcion[]>('/api/dealers/me/suscripcion/pagos');
    return response.data;
  },
};