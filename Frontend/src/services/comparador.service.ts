import api from "./api";

export interface VehiculoComparador {
  id: number;
  marca: string;
  modelo: string;
  version: string;
  tipoVehiculo: string;
  anio: number;
  precio: number;
  precioAnterior?: number | null;
  enOferta: boolean;
  kilometraje: number;
  condicion: string;
  transmision: string;
  combustible: string;
  motor: string;
  traccion: string;
  colorExterior?: string | null;
  colorInterior?: string | null;
  ubicacion: string;
  fotoPrincipal?: string | null;
}

export const comparadorService = {
  async comparar(ids: number[]): Promise<VehiculoComparador[]> {
    const params = new URLSearchParams();
    ids.forEach((id) => params.append("ids", String(id)));

    const response = await api.get<VehiculoComparador[]>(
      `/api/comparador?${params.toString()}`,
    );

    return response.data;
  },
};
