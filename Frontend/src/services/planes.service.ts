import api from './api';

export interface PlanCatalogo {
  nivel: string;
  nombre: string;
  descripcion: string | null;
  limiteAnuncios: number;
  cuotaDestacados: number;
  maxFotos: number;
  diasVigencia: number;
  precioMensual: number;
  precioTrimestral: number;
  precioAnual: number;
  descuentoTrimestralPorcentaje: number;
  descuentoAnualPorcentaje: number;
}

export const planesService = {
  async obtenerCatalogo(): Promise<PlanCatalogo[]> {
    const response = await api.get<PlanCatalogo[]>('/api/planes');
    return response.data;
  },
};