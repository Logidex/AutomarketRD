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

    const authData = response.data;

    if (authData.token) {
      localStorage.setItem('token', authData.token);
    }

    if (authData.usuario) {
      localStorage.setItem(
        'user',
        JSON.stringify(authData.usuario)
      );
    }

    return authData;
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

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
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
