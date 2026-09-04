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

  async registrarTransferencia(
    nombrePlan: string,
    ciclo: string,
    imagen: File
  ): Promise<{ exito: boolean; pagoId: number; mensaje: string }> {
    const formData = new FormData();
    formData.append('nombrePlan', nombrePlan);
    formData.append('ciclo', ciclo);
    formData.append('imagen', imagen);

    const response = await api.post<{ exito: boolean; pagoId: number; mensaje: string }>(
      '/api/pagos/transferencia',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },
};