import api from './api';

export interface UsuarioCuenta {
  usuarioId: number;
  nombre: string;
  apellido: string;
  email: string;
  emailConfirmado: boolean;
  telefonoPersonal: string | null;
  rol: string;
}

export const usuarioService = {
  async obtenerCuenta(): Promise<UsuarioCuenta> {
    const response = await api.get<UsuarioCuenta>('/api/usuario/me');
    return response.data;
  },

  async actualizarDatos(datos: {
    nombre: string;
    apellido: string;
    telefonoPersonal?: string | null;
  }): Promise<UsuarioCuenta> {
    const response = await api.put<UsuarioCuenta>(
      '/api/usuario/me',
      datos
    );
    return response.data;
  },

  async cambiarPassword(
    passwordActual: string,
    nuevaPassword: string
  ): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/usuario/cambiar-password',
      { passwordActual, nuevaPassword }
    );
    return response.data;
  },

  async solicitarCambioEmail(
    passwordActual: string,
    nuevoEmail: string
  ): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/usuario/cambiar-email',
      { passwordActual, nuevoEmail }
    );
    return response.data;
  },

  async confirmarCambioEmail(
    codigo: string
  ): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/usuario/confirmar-email',
      { codigo }
    );
    return response.data;
  },
};