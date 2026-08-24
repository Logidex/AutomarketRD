import api from './api';

export interface CuponAplicado {
  nivel: string;
  dias: number;
  fechaVencimientoUtc: string;
  mensaje: string;
}

export const cuponesService = {
  async aplicarCupon(codigo: string): Promise<CuponAplicado> {
    const response = await api.post<CuponAplicado>('/api/cupones/aplicar', { codigo });
    return response.data;
  },
};
