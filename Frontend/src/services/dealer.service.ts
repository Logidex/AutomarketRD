import api from "./api";
import type { PagedResult } from "../types/anuncio.types";

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
  esDealerVerificado: boolean;
}

export interface AgenciaListado {
  id: number;
  nombreAgencia: string;
  logoUrl?: string | null;
  ubicacion: string;
  telefonoAgencia: string;
  whatsApp?: string | null;
  descripcion: string;
  esDealerVerificado: boolean;
  planNivel?: string | null;
  cantidadAnuncios: number;
}

export interface AgenciaFiltros {
  busqueda?: string;
  soloVerificadas?: boolean;
  planNivel?: string;
  pagina?: number;
  cantidadPorPagina?: number;
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

  async listarAgencias(
    filtros: AgenciaFiltros = {}
  ): Promise<PagedResult<AgenciaListado>> {
    const params = new URLSearchParams();

    if (filtros.busqueda?.trim()) params.set("Busqueda", filtros.busqueda.trim());
    if (filtros.soloVerificadas) params.set("SoloVerificadas", "true");
    if (filtros.planNivel?.trim()) params.set("PlanNivel", filtros.planNivel.trim());
    params.set("Pagina", String(filtros.pagina ?? 1));
    params.set("CantidadPorPagina", String(filtros.cantidadPorPagina ?? 12));

    const response = await api.get<PagedResult<AgenciaListado>>(
      `/api/dealers?${params.toString()}`
    );
    return response.data;
  },
};