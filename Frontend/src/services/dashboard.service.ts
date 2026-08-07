// services/dashboard.service.ts
import api from "./api";

export interface DashboardResumen {
  totalAnuncios: number;
  anunciosActivos: number;
  anunciosBorrador: number;
  anunciosVendidos: number;
  anunciosPausados: number;
  totalLeads: number;
  leadsNoLeidos: number;
  planActual: string;
  diasRestantesSuscripcion: number;
  anunciosMasVistos: Array<{
    id: number;
    nombreAnuncio: string;
    vistas: number;
  }>;
}

export const dashboardService = {
  // services/dashboard.service.ts
  async obtenerResumen(): Promise<DashboardResumen> {
    const response = await api.get<DashboardResumen>(
      "/api/dealers/me/dashboard-resumen",
    );
    return response.data;
  },
};
