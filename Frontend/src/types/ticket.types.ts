export type TicketEstado =
  | "Abierto"
  | "EnProceso"
  | "Resuelto"
  | "Cerrado"
  | "Detenido";
export type TicketPrioridad = "Baja" | "Normal" | "Alta" | "Urgente";
export type TicketCategoria =
  | "General"
  | "Facturacion"
  | "Anuncios"
  | "SoporteTecnico"
  | "Cuenta";

export interface TicketListado {
  id: number;
  usuarioId: number;
  usuarioNombre: string;
  usuarioEmail: string;
  asunto: string;
  categoria: TicketCategoria;
  prioridad: TicketPrioridad;
  estado: TicketEstado;
  fechaCreacionUtc: string;
  fechaActualizacionUtc: string;
  ultimoMensaje: string;
  cantidadMensajes: number;
  ultimoMensajeEsAdmin: boolean;
}

export interface TicketMensaje {
  id: number;
  autorId: number;
  autorNombre: string;
  esAdmin: boolean;
  mensaje: string;
  fechaCreacionUtc: string;
}

export interface TicketDetalle {
  id: number;
  usuarioId: number;
  usuarioNombre: string;
  usuarioEmail: string;
  asunto: string;
  categoria: TicketCategoria;
  prioridad: TicketPrioridad;
  estado: TicketEstado;
  fechaCreacionUtc: string;
  fechaActualizacionUtc: string;
  mensajes: TicketMensaje[];
}
