import api from './api';

export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponse {
  exito: boolean;
  mensaje: string;
  token?: string;
}

export interface RegistroDto {
  nombre: string;
  apellido: string;
  email: string;
  password: string;
  rol: string;
  telefonoPersonal?: string;
  nombreAgencia?: string;
  agenciaRNC?: string;
  ubicacionAgencia: string;
  telefonoAgencia: string;
}

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


