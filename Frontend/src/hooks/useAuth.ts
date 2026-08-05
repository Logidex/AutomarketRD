import { useState } from 'react';
import type { LoginDto } from '../types/auth.types';
import { authService } from '../services/auth.service';

export function useAuth() {
  const [isAuthenticated, setIsAuthenticated] = useState(() =>
    authService.isAuthenticated()
  );

  const login = async (email: string, password: string) => {
    const data: LoginDto = { email, password };
    const response = await authService.login(data);

    if (response.token) {
      setIsAuthenticated(true);
      return response;
    }

    throw new Error(response.mensaje);
  };

  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
  };

  return {
    isAuthenticated,
    loading: false,
    login,
    logout,
  };
}