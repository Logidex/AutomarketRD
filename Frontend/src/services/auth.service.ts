import api from './api';

import type {
  LoginDto,
  RegistroDto,
  AuthResponse,
  UsuarioAuth
} from '../types/auth.types';

export const authService = {
  async login(data: LoginDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>(
      '/api/auth/login',
      data
    );

    this.guardarSesion(response.data);

    return response.data;
  },

  // Guarda token + usuario en localStorage (usado en login y ascenso de rol)
  guardarSesion(authData: AuthResponse) {
    if (authData.token) {
      // Almacenar token con marca de tiempo para tracking de expiración
      const expiringAt = Date.now() + (24 * 60 * 60 * 1000); // Default 24h en ms
      const tokenData = {
        token: authData.token,
        expiringAt,
      };
      localStorage.setItem('token', JSON.stringify(tokenData));
    }

    if (authData.usuario) {
      localStorage.setItem(
        'user',
        JSON.stringify(authData.usuario)
      );
    }
  },

  async register(
    data: RegistroDto
  ): Promise<{ exito: boolean; mensaje: string }> {
    const response = await api.post<{
      exito: boolean;
      mensaje: string;
    }>('/api/auth/registrar', data);

    return response.data;
  },

  async solicitarRecuperacion(email: string): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/auth/recuperar-password',
      { email }
    );
    return response.data;
  },

  async restablecerPassword(datos: {
    email: string;
    codigo: string;
    nuevaPassword: string;
  }): Promise<{ mensaje: string }> {
    const response = await api.post<{ mensaje: string }>(
      '/api/auth/restablecer-password',
      datos
    );
    return response.data;
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  // Actualiza solo las partes editadas del usuario guardado (nombre, correo, etc.)
  actualizarUsuario(patch: Partial<UsuarioAuth>) {
    const usuario = this.getCurrentUser();
    if (!usuario) return;

    localStorage.setItem(
      'user',
      JSON.stringify({ ...usuario, ...patch })
    );
  },

  isAuthenticated(): boolean {
    const tokenData = this.getTokenData();
    if (!tokenData) return false;
    
    // Verificar si el token aún no expira
    return Date.now() < tokenData.expiringAt;
  },

  getTokenData(): { token: string; expiringAt: number } | null {
    const tokenStr = localStorage.getItem('token');
    if (!tokenStr) return null;

    try {
      const tokenData = JSON.parse(tokenStr) as { token: string; expiringAt: number };
      return tokenData;
    } catch {
      localStorage.removeItem('token');
      return null;
    }
  },

  getCurrentUser(): UsuarioAuth | null {
    const user = localStorage.getItem('user');

    if (!user) {
      return null;
    }

    try {
      return JSON.parse(user) as UsuarioAuth;
    } catch {
      localStorage.removeItem('user');
      return null;
    }
  },

  getRole(): string | null {
    const user = this.getCurrentUser();

    return user?.rol ?? null;
  }
};
