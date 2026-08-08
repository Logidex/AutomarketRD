import api from "./api";
import type { LeadDealer } from "../types/lead.types";

export const leadService = {
  async obtenerMisLeads(): Promise<LeadDealer[]> {
    const response = await api.get<LeadDealer[]>("/api/leads/mis-leads");
    return response.data;
  },

  async marcarLeido(id: number): Promise<{ mensaje: string }> {
    const response = await api.patch<{ mensaje: string }>(
      `/api/leads/${id}/leido`,
    );
    return response.data;
  },
};
