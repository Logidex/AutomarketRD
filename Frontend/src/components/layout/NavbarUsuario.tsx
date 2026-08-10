import { Link, useNavigate } from "react-router-dom";
import { FaSignOutAlt } from "react-icons/fa";
import { authService } from "../../services/auth.service";
import { ROLES } from "../../constants/roles";

export default function NavbarUsuario() {
  const navigate = useNavigate();

  const autenticado = authService.isAuthenticated();
  const usuario = authService.getCurrentUser();

  const nombreUsuario = usuario
    ? `${usuario.nombre} ${usuario.apellido ?? ""}`.trim()
    : "Usuario";

  const inicial = usuario?.nombre
    ? usuario.nombre.charAt(0).toUpperCase()
    : "U";

  const handleLogout = () => {
    authService.logout();
    navigate("/login", { replace: true });
  };

  if (!autenticado) {
    return (
      <Link
        to="/login"
        className="rounded-lg bg-blue-500 px-5 py-2 font-semibold text-white transition-colors hover:bg-blue-600"
      >
        Iniciar Sesión
      </Link>
    );
  }

  // Dealer y Vendedor tienen sus paneles propios → "Mi Panel"
  if (usuario?.rol === ROLES.DEALER) {
    return (
      <Link
        to="/dashboard"
        className="rounded-lg bg-blue-500 px-5 py-2 font-semibold text-white transition-colors hover:bg-blue-600"
      >
        Mi Panel
      </Link>
    );
  }

  if (usuario?.rol === ROLES.VENDEDOR) {
    return (
      <Link
        to="/vendedor"
        className="rounded-lg bg-blue-500 px-5 py-2 font-semibold text-white transition-colors hover:bg-blue-600"
      >
        Mi Panel
      </Link>
    );
  }

  // Comprador: avatar + nombre (link a /perfil) + logout
  return (
    <div className="flex items-center gap-2">
      <Link
        to="/perfil"
        title="Mi Perfil"
        className="flex items-center gap-2 rounded-full border border-white/15 bg-white/5 py-1 pl-1 pr-3 transition-colors hover:border-blue-500/50 hover:bg-white/10"
      >
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-500 text-sm font-bold text-white">
          {inicial}
        </span>
        <span className="hidden max-w-28 truncate text-sm font-medium sm:block">
          {nombreUsuario}
        </span>
      </Link>
      <button
        type="button"
        onClick={handleLogout}
        title="Cerrar sesión"
        className="rounded-full p-2 text-[#9aa1b1] transition-colors hover:bg-white/10 hover:text-white"
      >
        <FaSignOutAlt />
      </button>
    </div>
  );
}