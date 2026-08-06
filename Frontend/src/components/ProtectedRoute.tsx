import {
  Navigate,
  Outlet,
  useLocation
} from 'react-router-dom';

import { authService } from '../services/auth.service';

interface ProtectedRouteProps {
  allowedRoles?: string[];
}

export default function ProtectedRoute({
  allowedRoles
}: ProtectedRouteProps) {
  const location = useLocation();

  const usuario = authService.getCurrentUser();

  // No hay usuario guardado
  if (!usuario) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  // Si la ruta no requiere un rol específico,
  // basta con que el usuario esté autenticado
  if (!allowedRoles || allowedRoles.length === 0) {
    return <Outlet />;
  }

  const rolActual = usuario.rol?.trim().toLowerCase();

  const tienePermiso = allowedRoles.some(
    (rol) => rol.toLowerCase() === rolActual
  );

  if (tienePermiso) {
    return <Outlet />;
  }

  // Vendedor: por ahora va a una pantalla temporal
  if (rolActual === 'vendedor') {
    return <Navigate to="/vendedor" replace />;
  }

  // Comprador: va al inicio público
  if (rolActual === 'comprador') {
    return <Navigate to="/" replace />;
  }

  // Rol desconocido
  return <Navigate to="/login" replace />;
}