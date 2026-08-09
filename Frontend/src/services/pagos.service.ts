import api from './api';

export interface GenerarLinkPagoResponse {
  url: string;
  monto: number;
  moneda: string;
}

export const pagosService = {
  async generarLinkPago(
    nombrePlan: string,
    ciclo: string
  ): Promise<GenerarLinkPagoResponse> {
    const response = await api.post<GenerarLinkPagoResponse>(
      '/api/pagos/generar-link',
      { nombrePlan, ciclo }
    );
    return response.data;
  },

  async confirmarPago(orderId: string): Promise<void> {
    await api.post('/api/pagos/confirmar-pago', { orderId });
  },
};