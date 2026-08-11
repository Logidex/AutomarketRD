import api from "./api";
import type { LeadContactoUsuario, LeadDealer } from "../types/lead.types";

export interface LeadNoLeidosResumen {
  cantidadNoLeidos: number;
  recientes: LeadDealer[];
}

export interface LeadContactoDto {
  anuncioId: number;
  nombreContacto: string;
  emailContacto?: string;
  telefonoContacto?: string;
  mensaje: string;
  canal: "Formulario" | "WhatsApp" | "Messenger";
}

export const leadService = {
  async crearLead(dto: LeadContactoDto): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>("/api/leads", dto);
    return response.data;
  },

  async obtenerMisLeads(): Promise<LeadDealer[]> {
    const response = await api.get<LeadDealer[]>("/api/leads/mis-leads");
    return response.data;
  },

  async obtenerMisContactos(): Promise<LeadContactoUsuario[]> {
    const response = await api.get<LeadContactoUsuario[]>("/api/leads/mis-contactos");
    return response.data;
  },

  async obtenerResumenNoLeidos(): Promise<LeadNoLeidosResumen> {
    const response = await api.get<LeadNoLeidosResumen>(
      "/api/leads/resumen-no-leidos",
    );
    return response.data;
  },

  async marcarLeido(id: number): Promise<{ mensaje: string }> {
    const response = await api.patch<{ mensaje: string }>(
      `/api/leads/${id}/leido`,
    );
    return response.data;
  },

  async marcarTodosLeidos(): Promise<{ mensaje: string; cantidad: number }> {
    const response = await api.patch<{ mensaje: string; cantidad: number }>(
      "/api/leads/marcar-todos-leido",
    );
    return response.data;
  },
};
