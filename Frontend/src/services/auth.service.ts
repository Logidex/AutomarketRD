import api from './api';
import type { LoginDto, RegistroDto, AuthResponse } from '../types/auth.types';

export const authService = {
  async login(data: LoginDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/login', data);

    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
    }

    return response.data;
  },

  async register(data: RegistroDto): Promise<{ exito: boolean; mensaje: string }> {
    const response = await api.post<{ exito: boolean; mensaje: string }>('/api/auth/registrar', data);

    return response.data;
  },

  logout() {
    localStorage.removeItem('token');
  },

  isAuthenticated(): boolean {
    const token = localStorage.getItem('token');
    return !!token;
  },
};


