/* eslint-disable no-useless-catch */
/* eslint-disable react-hooks/set-state-in-effect */
import { useState, useEffect } from 'react';
import { authService, type LoginDto } from '../services/auth.service';

export function useAuth() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const auth = authService.isAuthenticated();
    setIsAuthenticated(auth);
    setLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    const data: LoginDto = { email, password };

    try {
      const response = await authService.login(data);

      // Verifica si hay token en lugar de exito
      if (response.token) {
        setIsAuthenticated(true);
      } else {
        throw new Error(response.mensaje);
      }
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
  };

  return {
    isAuthenticated,
    loading,
    login,
    logout,
  };
}
