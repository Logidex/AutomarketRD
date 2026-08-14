import api from './api';

import type {
  LoginDto,
  RegistroDto,
  AuthResponse,
  UsuarioAuth
} from '../types/auth.types';

const USER_KEY = 'user:v1';
const USER_KEY_V0 = 'user';

export const authService = {
  async login(data: LoginDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>(
      '/api/auth/login',
      data
    );

    this.guardarSesion(response.data);

    return response.data;
  },

  // El JWT vive en una cookie HttpOnly (el servidor la establece en el login).
  // Aquí solo se guarda el usuario (metadatos no sensibles) en localStorage.
  guardarSesion(authData: AuthResponse) {
    if (authData.usuario) {
      localStorage.setItem(USER_KEY, JSON.stringify(authData.usuario));
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
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(USER_KEY_V0);

    // Borra la cookie HttpOnly en el servidor (fire-and-forget: el cliente
    // no necesita esperar para navegar a /login).
    void api.post('/api/auth/logout').catch(() => {});
  },

  // Actualiza solo las partes editadas del usuario guardado (nombre, correo, etc.)
  actualizarUsuario(patch: Partial<UsuarioAuth>) {
    const usuario = this.getCurrentUser();
    if (!usuario) return;

    localStorage.setItem(USER_KEY, JSON.stringify({ ...usuario, ...patch }));
  },

  isAuthenticated(): boolean {
    // El token está en la cookie HttpOnly (no accesible desde JS).
    // Se considera autenticado si hay un usuario guardado; la cookie
    // expira por sí sola y un 401 del servidor limpia el estado local.
    return this.getCurrentUser() !== null;
  },

  getCurrentUser(): UsuarioAuth | null {
    let user = localStorage.getItem(USER_KEY);

    if (!user) {
      const userV0 = localStorage.getItem(USER_KEY_V0);
      if (userV0) {
        user = userV0;
        localStorage.setItem(USER_KEY, userV0);
        localStorage.removeItem(USER_KEY_V0);
      }
    }

    if (!user) {
      return null;
    }

    try {
      return JSON.parse(user) as UsuarioAuth;
    } catch {
      localStorage.removeItem(USER_KEY);
      return null;
    }
  },

  getRole(): string | null {
    const user = this.getCurrentUser();

    return user?.rol ?? null;
  }
};
