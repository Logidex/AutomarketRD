import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  FaChevronDown,
  FaHeart,
  FaHistory,
  FaSignOutAlt,
  FaUser,
  FaUserEdit,
} from "react-icons/fa";
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

  // Comprador: avatar + nombre (menú desplegable) + logout directo
  return (
    <div ref={contenedorRef} className="relative flex items-center gap-2">
      <button
        type="button"
        onClick={() => setMenuAbierto((abierto) => !abierto)}
        aria-expanded={menuAbierto}
        aria-haspopup="menu"
        title="Mi cuenta"
        className="flex items-center gap-2 rounded-full border border-white/15 bg-white/5 py-1 pl-1 pr-2.5 transition-colors hover:border-blue-500/50 hover:bg-white/10"
      >
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-500 text-sm font-bold text-white">
          {inicial}
        </span>
        <span className="hidden max-w-28 truncate text-sm font-medium sm:block">
          {nombreUsuario}
        </span>
        <FaChevronDown className="text-xs text-[#9aa1b1]" />
      </button>

      {menuAbierto && (
        <div
          role="menu"
          className="absolute right-0 top-[calc(100%+10px)] z-50 w-64 overflow-hidden rounded-xl border border-white/10 bg-[#13161d] shadow-xl"
        >
          <div className="border-b border-white/10 px-4 py-3">
            <p className="truncate text-sm font-semibold text-white">
              {nombreUsuario}
            </p>
            <p className="truncate text-xs text-[#9aa1b1]">{usuario?.email}</p>
          </div>

          <Link
            to="/perfil"
            role="menuitem"
            onClick={() => setMenuAbierto(false)}
            className="flex items-center gap-3 px-4 py-3 text-sm text-gray-200 transition-colors hover:bg-white/5"
          >
            <FaUser className="text-[#9aa1b1]" />
            Mi Perfil
          </Link>

          <Link
            to="/perfil/favoritos"
            role="menuitem"
            onClick={() => setMenuAbierto(false)}
            className="flex items-center gap-3 px-4 py-3 text-sm text-gray-200 transition-colors hover:bg-white/5"
          >
            <FaHeart className="text-[#9aa1b1]" />
            Mis Favoritos
          </Link>

          <Link
            to="/perfil/historial"
            role="menuitem"
            onClick={() => setMenuAbierto(false)}
            className="flex items-center gap-3 px-4 py-3 text-sm text-gray-200 transition-colors hover:bg-white/5"
          >
            <FaHistory className="text-[#9aa1b1]" />
            Recientes
          </Link>

          <Link
            to="/perfil/editar"
            role="menuitem"
            onClick={() => setMenuAbierto(false)}
            className="flex items-center gap-3 px-4 py-3 text-sm text-gray-200 transition-colors hover:bg-white/5"
          >
            <FaUserEdit className="text-[#9aa1b1]" />
            Editar Perfil
          </Link>

          <button
            type="button"
            role="menuitem"
            onClick={handleLogout}
            className="flex w-full items-center gap-3 border-t border-white/10 px-4 py-3 text-left text-sm text-red-400 transition-colors hover:bg-red-500/10"
          >
            <FaSignOutAlt />
            Cerrar sesión
          </button>
        </div>
      )}
    </div>
  );
}