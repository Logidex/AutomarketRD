import api from './api';
import type { AuthResponse } from '../types/auth.types';

export interface UsuarioCuenta {
  usuarioId: number;
  nombre: string;
  apellido: string;
  email: string;
  emailConfirmado: boolean;
  telefonoPersonal: string | null;
  rol: string;
}

export interface AscenderRolDto {
  nuevoRol: "Vendedor" | "Dealer";
  nombreAgencia?: string;
  agenciaRNC?: string;
  ubicacionAgencia?: string;
  telefonoAgencia?: string;
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

  async confirmarCambioPassword(
    codigo: string
  ): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/usuario/confirmar-password',
      { codigo }
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

  // Asciende el rol de la cuenta (Comprador → Vendedor/Dealer, Vendedor → Dealer).
  async ascenderRol(dto: AscenderRolDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>(
      '/api/usuario/ascender-rol',
      dto
    );
    return response.data;
  },
};