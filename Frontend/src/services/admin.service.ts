import api from "./api";

export interface AdminResumen {
  totalUsuarios: number;
  totalAnuncios: number;
  totalLeads: number;
  anunciosActivos: number;
  anunciosBorrador: number;
  anunciosVendidos: number;
  anunciosPausados: number;
  leadsNoLeidos: number;
  planActual: string;
  diasRestantesSuscripcion: number;
  limiteAnuncios: number;
  anunciosMasVistos: Array<{
    id: number;
    nombreAnuncio: string;
    vistas: number;
  }>;
}

export interface UsuarioAdmin {
  usuarioId: number;
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
  isActivo: boolean;
  fechaRegistro: string;
}

export interface CambiarRolAdminDto {
  nuevoRol: string;
  nombreAgencia?: string;
  agenciaRNC?: string;
  ubicacionAgencia?: string;
  telefonoAgencia?: string;
}

export interface AnuncioAdmin {
  id: number;
  marca: string;
  modelo: string;
  precio: number;
  moneda: string;
  usuarioId: number;
}

export interface PagoAdmin {
  id: number;
  perfilDealerId: number;
  dealerNombreAgencia: string;
  dealerEmail: string;
  nivel: string;
  ciclo: string;
  estado: string;
  monto: number;
  moneda: string;
  ordenIdPayPal?: string | null;
  captureIdPayPal?: string | null;
  metodo?: string;
  estadoTransferencia?: string | null;
  urlCapturaTransferencia?: string | null;
  notasAdmin?: string | null;
  fechaConfirmacionUtc?: string | null;
  fechaUtc: string;
}

export interface PlanAdmin {
  id: number;
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
  activo: boolean;
}

export interface PlanAdminForm {
  nivel: string;
  nombre: string;
  descripcion?: string | null;
  limiteAnuncios: number;
  cuotaDestacados: number;
  maxFotos: number;
  diasVigencia: number;
  precioMensual: number;
  descuentoTrimestralPorcentaje: number;
  descuentoAnualPorcentaje: number;
  activo: boolean;
}

const respuesta = async <T>(promesa: Promise<{ data: T }>): Promise<T> =>
  (await promesa).data;

interface PaginatedResponse<T> {
  items: T[];
  totalRegistros: number;
  paginaActual: number;
  cantidadPorPagina: number;
  totalPaginas: number;
}

// ===== Reportes de anuncios =====
export type MotivoReporte =
  | "ContenidoInapropiado"
  | "FraudeEstafa"
  | "InformacionFalsa"
  | "Duplicado"
  | "Otro";

export interface ReporteAdmin {
  id: number;
  motivo: MotivoReporte;
  detalle: string | null;
  estado: "Pendiente" | "Descartado" | "Resuelto";
  fechaCreacionUtc: string;
  ipReportante: string;
  anuncioId: number;
  anuncioTitulo: string;
  anuncioEstado: string;
  anuncioFotoPrincipal: string | null;
  anuncioPrecio: number;
  anuncioMoneda: string;
}

export interface CrearReportePublicoDto {
  anuncioId: number;
  motivo: MotivoReporte;
  detalle?: string;
}


export const adminService = {
  // ===== Dashboard / Resumen =====
  async obtenerResumen(): Promise<AdminResumen> {
    return respuesta(api.get<AdminResumen>("/api/admin/dashboard/resumen"));
  },

  // ===== Usuarios =====
  async listarUsuarios(): Promise<UsuarioAdmin[]> {
    return respuesta(api.get<PaginatedResponse<UsuarioAdmin>>("/api/admin/usuarios")).then(r => r.items);
  },

  async suspenderUsuario(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.patch(`/api/admin/usuarios/${id}/suspender`));
  },

  async reactivarUsuario(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.patch(`/api/admin/usuarios/${id}/reactivar`));
  },

  async cambiarRol(id: number, datos: CambiarRolAdminDto): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.put(`/api/admin/usuarios/${id}/rol`, datos));
  },

  // ===== Anuncios =====
  async listarAnuncios(): Promise<AnuncioAdmin[]> {
    return respuesta(api.get<PaginatedResponse<AnuncioAdmin>>("/api/admin/anuncios")).then(r => r.items);
  },

  async eliminarAnuncio(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.delete(`/api/admin/anuncios/${id}`));
  },

  // ===== Suscripciones de dealers =====
  async cambiarPlan(dealerId: number, nivel: string): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.put(`/api/admin/suscripciones/${dealerId}/plan`, {
      nuevoNivel: nivel,
    }));
  },

  async renovarSuscripcion(dealerId: number, nuevaFechaVencimiento: string): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.put(`/api/admin/suscripciones/${dealerId}/renovar`, {
      nuevaFechaVencimiento,
    }));
  },

  // ===== Pagos y reembolsos =====
  async listarPagos(): Promise<PagoAdmin[]> {
    return respuesta(api.get<PagoAdmin[]>("/api/admin/pagos"));
  },

  async reembolsarPago(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.post(`/api/admin/pagos/${id}/reembolsar`));
  },

  // ===== Catálogo de planes =====
  async listarPlanes(): Promise<PlanAdmin[]> {
    return respuesta(api.get<PlanAdmin[]>("/api/admin/planes"));
  },

  async crearPlan(datos: PlanAdminForm): Promise<PlanAdmin> {
    return respuesta(api.post<PlanAdmin>("/api/admin/planes", datos));
  },

  async actualizarPlan(id: number, datos: PlanAdminForm): Promise<PlanAdmin> {
    return respuesta(api.put<PlanAdmin>(`/api/admin/planes/${id}`, datos));
  },

  async eliminarPlan(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.delete(`/api/admin/planes/${id}`));
  },

  // ===== Reportes de anuncios =====
  async listarReportes(estado: "Pendiente" | "Descartado" | "Resuelto" = "Pendiente"): Promise<ReporteAdmin[]> {
    return respuesta(api.get<ReporteAdmin[]>("/api/admin/reportes", { params: { estado } }));
  },

  async contarReportesPendientes(): Promise<{ total: number }> {
    return respuesta(api.get<{ total: number }>("/api/admin/reportes/pendientes/contador"));
  },

  async descartarReporte(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.patch(`/api/admin/reportes/${id}/descartar`));
  },

  async resolverReporte(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.patch(`/api/admin/reportes/${id}/resolver`));
  },

  // ===== Eliminación de usuarios =====
  async eliminarUsuario(id: number): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.delete(`/api/admin/usuarios/${id}`));
  },

  // ===== Transferencias bancarias =====
  async listarTransferenciasPendientes(): Promise<PagoAdmin[]> {
    return respuesta(api.get<PagoAdmin[]>("/api/admin/transferencias"));
  },

  async aprobarTransferencia(id: number, notas?: string): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.post(`/api/admin/transferencias/${id}/aprobar`, { notas }));
  },

  async rechazarTransferencia(id: number, notas?: string): Promise<{ exito: boolean; mensaje: string }> {
    return respuesta(api.post(`/api/admin/transferencias/${id}/rechazar`, { notas }));
  },
};