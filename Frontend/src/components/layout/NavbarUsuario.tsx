import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  FaChevronDown,
  FaEnvelope,
  FaHeart,
  FaHistory,
  FaSignOutAlt,
  FaUser,
  FaUserEdit,
} from "react-icons/fa";
import { authService } from "../../services/auth.service";
import { ROLES } from "../../constants/roles";
import CampanaNotificaciones from "./CampanaNotificaciones";

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

  const [menuAbierto, setMenuAbierto] = useState(false);
  const contenedorRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const cerrarFuera = (e: MouseEvent) => {
      if (
        contenedorRef.current &&
        !contenedorRef.current.contains(e.target as Node)
      ) {
        setMenuAbierto(false);
      }
    };

    document.addEventListener("mousedown", cerrarFuera);
    return () => document.removeEventListener("mousedown", cerrarFuera);
  }, []);

  const handleLogout = () => {
    setMenuAbierto(false);
    authService.logout();
    navigate("/login", { replace: true });
  };

  if (!autenticado) {
    return (
      <div className="flex items-center gap-3">
        <Link
          to="/registro"
          className="rounded-lg border border-line px-5 py-2 font-semibold text-ink transition-colors hover:border-blue-500/50 hover:bg-hover"
        >
          Registrarse
        </Link>
        <Link
          to="/login"
          className="rounded-lg bg-blue-500 px-5 py-2 font-semibold text-white transition-colors hover:bg-blue-600"
        >
          Iniciar Sesión
        </Link>
      </div>
    );
  }

  // Dealer y Vendedor tienen sus paneles propios → "Mi Panel"
  const rolEsDealerOVendedor =
    usuario?.rol === ROLES.DEALER || usuario?.rol === ROLES.VENDEDOR;

  const rutaPanel =
    usuario?.rol === ROLES.DEALER
      ? "/dashboard"
      : usuario?.rol === ROLES.VENDEDOR
        ? "/vendedor"
        : "/perfil";

  // Admin: acceso directo al panel de administración
  if (usuario?.rol === ROLES.ADMIN) {
    return (
      <Link
        to="/admin"
        className="rounded-lg bg-violet-600 px-5 py-2 font-semibold text-white transition-colors hover:bg-violet-700"
      >
        Panel Admin
      </Link>
    );
  }

  // Todos los roles autenticados comparten el menú desplegable de cuenta.
  // El comprador conserva los accesos a favoritos, recientes, contactos y edición;
  // dealer/vendedor solo acceden a su propio panel.
  return (
    <div ref={contenedorRef} className="relative flex items-center gap-2">
      {rolEsDealerOVendedor && (
        <CampanaNotificaciones
          rutaLeads={
            usuario?.rol === ROLES.DEALER ? "/dashboard/leads" : "/vendedor/interesados"
          }
        />
      )}

      <button
        type="button"
        onClick={() => setMenuAbierto((abierto) => !abierto)}
        aria-expanded={menuAbierto}
        aria-haspopup="menu"
        title="Mi cuenta"
        className="flex items-center gap-2 rounded-full border border-line bg-hover py-1 pl-1 pr-2.5 transition-colors hover:border-blue-500/50 hover:bg-hover"
      >
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-500 text-sm font-bold text-white">
          {inicial}
        </span>
        <span className="hidden max-w-28 truncate text-sm font-medium sm:block">
          {nombreUsuario}
        </span>
        <FaChevronDown className="text-xs text-ink-2" />
      </button>

      {menuAbierto && (
        <div
          role="menu"
          className="absolute right-0 top-[calc(100%+10px)] z-50 w-64 overflow-hidden rounded-xl border border-line bg-surface shadow-xl"
        >
          <div className="border-b border-line px-4 py-3">
            <p className="truncate text-sm font-semibold text-ink">
              {nombreUsuario}
            </p>
            <p className="truncate text-xs text-ink-2">{usuario?.email}</p>
          </div>

          <Link
            to={rutaPanel}
            role="menuitem"
            onClick={() => setMenuAbierto(false)}
            className="flex items-center gap-3 px-4 py-3 text-sm text-ink-3 transition-colors hover:bg-hover"
          >
            <FaUser className="text-ink-2" />
            {rolEsDealerOVendedor ? "Mi Panel" : "Mi Perfil"}
          </Link>

          {!rolEsDealerOVendedor && (
            <>
              <Link
                to="/perfil/favoritos"
                role="menuitem"
                onClick={() => setMenuAbierto(false)}
                className="flex items-center gap-3 px-4 py-3 text-sm text-ink-3 transition-colors hover:bg-hover"
              >
                <FaHeart className="text-ink-2" />
                Mis Favoritos
              </Link>

              <Link
                to="/perfil/historial"
                role="menuitem"
                onClick={() => setMenuAbierto(false)}
                className="flex items-center gap-3 px-4 py-3 text-sm text-ink-3 transition-colors hover:bg-hover"
              >
                <FaHistory className="text-ink-2" />
                Recientes
              </Link>

              <Link
                to="/perfil/contactados"
                role="menuitem"
                onClick={() => setMenuAbierto(false)}
                className="flex items-center gap-3 px-4 py-3 text-sm text-ink-3 transition-colors hover:bg-hover"
              >
                <FaEnvelope className="text-ink-2" />
                Contactos enviados
              </Link>

              <Link
                to="/perfil/editar"
                role="menuitem"
                onClick={() => setMenuAbierto(false)}
                className="flex items-center gap-3 px-4 py-3 text-sm text-ink-3 transition-colors hover:bg-hover"
              >
                <FaUserEdit className="text-ink-2" />
                Editar Perfil
              </Link>
            </>
          )}

          <button
            type="button"
            role="menuitem"
            onClick={handleLogout}
            className="flex w-full items-center gap-3 border-t border-line px-4 py-3 text-left text-sm text-red-400 transition-colors hover:bg-red-500/10"
          >
            <FaSignOutAlt />
            Cerrar sesión
          </button>
        </div>
      )}
    </div>
  );
}