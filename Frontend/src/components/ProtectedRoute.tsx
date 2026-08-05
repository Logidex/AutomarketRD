import { Navigate, Outlet } from 'react-router-dom';
import { authService } from '../services/auth.service';

export default function ProtectedRoute() {
  // Verifica si el usuario tiene un token válido
  const isAuth = authService.isAuthenticated();

  // Si está autenticado, renderiza las rutas hijas (Outlet), si no, lo manda al login
  return isAuth ? <Outlet /> : <Navigate to="/login" replace />;
}