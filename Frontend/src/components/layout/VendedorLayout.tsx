import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { FaCarSide, FaEnvelope, FaHeadset, FaHome, FaLevelUpAlt, FaPaperPlane, FaSignOutAlt, FaUserCog } from "react-icons/fa";
import { authService } from "../../services/auth.service";
import { confirmarCierreSesion } from "../../utils/confirmarCierreSesion";
import logo from "../../assets/AutoMarketRD_Logo.svg";
import CampanaNotificaciones from "./CampanaNotificaciones";
import BotonTema from "../BotonTema";

export default function VendedorLayout() {
  const navigate = useNavigate();
  const usuario = authService.getCurrentUser();

  const nombreUsuario = usuario
    ? `${usuario.nombre} ${usuario.apellido ?? ""}`.trim()
    : "Usuario";

  const inicial = usuario?.nombre ? usuario.nombre.charAt(0).toUpperCase() : "U";

  const handleLogout = async () => {
    if (!(await confirmarCierreSesion())) return;
    authService.logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex h-screen w-screen flex-col overflow-hidden bg-page font-sans">
      {/* BARRA SUPERIOR SOBRIA */}
      <header className="flex h-20 shrink-0 items-center justify-between border-b border-line bg-surface px-6">
        <Link to="/vendedor" className="flex items-center gap-2">
          <img src={logo} alt="AutoMarket RD" className="h-20 w-40 object-contain" />
          <span className="hidden text-xs text-ink-3 sm:inline">
            Área de Vendedor
          </span>
        </Link>

        <div className="flex items-center gap-4">
          <BotonTema />

          <CampanaNotificaciones rutaLeads="/vendedor/interesados" tema="claro" />

          <Link
            to="/"
            title="Volver al inicio"
            className="flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-ink-3 transition-colors hover:bg-hover hover:text-ink-2"
          >
            <FaHome />
            <span className="hidden sm:inline">Volver al inicio</span>
          </Link>
          <span className="hidden text-sm text-ink-2 md:inline">
            {nombreUsuario}
          </span>
          <span className="flex h-8 w-8 items-center justify-center rounded-full bg-surface-2 text-sm font-semibold text-ink-2">
            {inicial}
          </span>
          <button
            type="button"
            onClick={handleLogout}
            title="Cerrar sesión"
            className="flex h-8 w-8 items-center justify-center rounded-md text-ink-3 transition-colors hover:bg-hover hover:text-ink-2"
          >
            <FaSignOutAlt />
          </button>
        </div>
      </header>

      {/* NAVIGACIÓN */}
      <div className="flex shrink-0 items-center gap-1 border-b border-line bg-surface px-6">
        <NavLink
          to="/vendedor"
          end
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-ink text-ink"
                : "border-transparent text-ink-3 hover:text-ink"
            }`
          }
        >
          <FaCarSide className="text-xs" />
          Mi Vehículo
        </NavLink>

        <NavLink
          to="/vendedor/interesados"
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-ink text-ink"
                : "border-transparent text-ink-3 hover:text-ink"
            }`
          }
        >
          <FaEnvelope className="text-xs" />
          Interesados
        </NavLink>

        <NavLink
          to="/vendedor/contactados"
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-ink text-ink"
                : "border-transparent text-ink-3 hover:text-ink"
            }`
          }
        >
          <FaPaperPlane className="text-xs" />
          Contactados
        </NavLink>

        <NavLink
          to="/vendedor/ascender"
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-yellow-600 text-yellow-700"
                : "border-transparent text-ink-3 hover:text-ink"
            }`
          }
        >
          <FaLevelUpAlt className="text-xs" />
          Ascender a Dealer
        </NavLink>

        <NavLink
          to="/vendedor/soporte"
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-ink text-ink"
                : "border-transparent text-ink-3 hover:text-ink"
            }`
          }
        >
          <FaHeadset className="text-xs" />
          Soporte
        </NavLink>

        <NavLink
          to="/vendedor/cuenta"
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-ink text-ink"
                : "border-transparent text-ink-3 hover:text-ink"
            }`
          }
        >
          <FaUserCog className="text-xs" />
          Mi cuenta
        </NavLink>
      </div>

      {/* CONTENIDO */}
      <main className="flex-1 overflow-y-auto p-6">
        <Outlet />
      </main>
    </div>
  );
}