import api from "./api";
import type {
  TicketDetalle,
  TicketEstado,
  TicketListado,
} from "../types/ticket.types";

export interface TicketCreateDto {
  asunto: string;
  categoria:
    | "General"
    | "Facturacion"
    | "Anuncios"
    | "SoporteTecnico"
    | "Cuenta";
  prioridad: "Baja" | "Normal" | "Alta" | "Urgente";
  mensaje: string;
}

export interface TicketResumenAdmin {
  cantidadAbiertos: number;
}

export const ticketService = {
  // ===== Panel del Dealer/Vendedor =====
  async crearTicket(dto: TicketCreateDto): Promise<{ mensaje: string; ticketId: number }> {
    const response = await api.post<{ mensaje: string; ticketId: number }>(
      "/api/tickets",
      dto,
    );
    return response.data;
  },

  async obtenerMisTickets(): Promise<TicketListado[]> {
    const response = await api.get<TicketListado[]>("/api/tickets/mis-tickets");
    return response.data;
  },

  async obtenerTicket(id: number): Promise<TicketDetalle> {
    const response = await api.get<TicketDetalle>(`/api/tickets/${id}`);
    return response.data;
  },

  async responderTicket(
    id: number,
    mensaje: string,
  ): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      `/api/tickets/${id}/mensajes`,
      { mensaje },
    );
    return response.data;
  },

  async cerrarTicket(id: number): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      `/api/tickets/${id}/cerrar`,
    );
    return response.data;
  },

  // ===== Panel del Administrador =====
  async obtenerTicketsAdmin(): Promise<TicketListado[]> {
    const response = await api.get<TicketListado[]>("/api/admin/tickets");
    return response.data;
  },

  async obtenerTicketAdmin(id: number): Promise<TicketDetalle> {
    const response = await api.get<TicketDetalle>(`/api/admin/tickets/${id}`);
    return response.data;
  },

  async responderTicketAdmin(
    id: number,
    mensaje: string,
  ): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      `/api/admin/tickets/${id}/mensajes`,
      { mensaje },
    );
    return response.data;
  },

  async cambiarEstadoAdmin(
    id: number,
    nuevoEstado: TicketEstado,
  ): Promise<{ mensaje: string }> {
    const response = await api.patch<{ mensaje: string }>(
      `/api/admin/tickets/${id}/estado`,
      { nuevoEstado },
    );
    return response.data;
  },

  async obtenerResumenAdmin(): Promise<TicketResumenAdmin> {
    const response = await api.get<TicketResumenAdmin>(
      "/api/admin/tickets/resumen",
    );
    return response.data;
  },
};
