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
  usuarioId: number;
}

export interface PlanAdmin {
  id: number;
  nivel: string;
  nombre: string;
  descripcion: string | null;
  limiteAnuncios: number;
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
  precioMensual: number;
  descuentoTrimestralPorcentaje: number;
  descuentoAnualPorcentaje: number;
  activo: boolean;
}

const respuesta = async <T>(promesa: Promise<{ data: T }>): Promise<T> =>
  (await promesa).data;

export const adminService = {
  // ===== Dashboard / Resumen =====
  async obtenerResumen(): Promise<AdminResumen> {
    return respuesta(api.get<AdminResumen>("/api/admin/dashboard/resumen"));
  },

  // ===== Usuarios =====
  async listarUsuarios(): Promise<UsuarioAdmin[]> {
    return respuesta(api.get<UsuarioAdmin[]>("/api/admin/usuarios"));
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
    return respuesta(api.get<AnuncioAdmin[]>("/api/admin/anuncios"));
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
};