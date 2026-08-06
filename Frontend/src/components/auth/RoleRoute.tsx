import {
  Navigate,
  Outlet,
  useLocation
} from 'react-router-dom';

import { authService } from '../../services/auth.service';

interface RoleRouteProps {
  allowedRoles: string[];
}

export default function RoleRoute({
  allowedRoles
}: RoleRouteProps) {
  const location = useLocation();
  const usuario = authService.getCurrentUser();

  if (!usuario) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  const rolActual = usuario.rol?.trim().toLowerCase();

  const tienePermiso = allowedRoles.some(
    (rol) => rol.toLowerCase() === rolActual
  );

  if (!tienePermiso) {
    if (rolActual === 'vendedor') {
      return <Navigate to="/vendedor" replace />;
    }

    if (rolActual === 'comprador') {
      return <Navigate to="/" replace />;
    }

    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}