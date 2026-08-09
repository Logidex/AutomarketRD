import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { FaCarSide, FaEnvelope, FaSignOutAlt } from "react-icons/fa";
import { authService } from "../../services/auth.service";
import logo from "../../assets/AutoMarketRD_Logo.svg";

export default function VendedorLayout() {
  const navigate = useNavigate();
  const usuario = authService.getCurrentUser();

  const nombreUsuario = usuario
    ? `${usuario.nombre} ${usuario.apellido ?? ""}`.trim()
    : "Usuario";

  const inicial = usuario?.nombre ? usuario.nombre.charAt(0).toUpperCase() : "U";

  const handleLogout = () => {
    authService.logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex h-screen w-screen flex-col overflow-hidden bg-[#f6f7f9] font-sans">
      {/* BARRA SUPERIOR SOBRIA */}
      <header className="flex h-20 shrink-0 items-center justify-between border-b border-gray-200 bg-white px-6">
        <Link to="/vendedor" className="flex items-center gap-2">
          <img src={logo} alt="AutoMarket RD" className="h-20 w-40 object-contain" />
          <span className="hidden text-xs text-gray-500 sm:inline">
            Área de Vendedor
          </span>
        </Link>

        <div className="flex items-center gap-4">
          <span className="hidden text-sm text-gray-600 md:inline">
            {nombreUsuario}
          </span>
          <span className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-200 text-sm font-semibold text-gray-700">
            {inicial}
          </span>
          <button
            type="button"
            onClick={handleLogout}
            title="Cerrar sesión"
            className="flex h-8 w-8 items-center justify-center rounded-md text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-700"
          >
            <FaSignOutAlt />
          </button>
        </div>
      </header>

      {/* NAVIGACIÓN */}
      <div className="flex shrink-0 items-center gap-1 border-b border-gray-200 bg-white px-6">
        <NavLink
          to="/vendedor"
          end
          className={({ isActive }) =>
            `flex items-center gap-2 border-b-2 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive
                ? "border-gray-800 text-gray-900"
                : "border-transparent text-gray-500 hover:text-gray-800"
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
                ? "border-gray-800 text-gray-900"
                : "border-transparent text-gray-500 hover:text-gray-800"
            }`
          }
        >
          <FaEnvelope className="text-xs" />
          Interesados
        </NavLink>
      </div>

      {/* CONTENIDO */}
      <main className="flex-1 overflow-y-auto p-6">
        <Outlet />
      </main>
    </div>
  );
}