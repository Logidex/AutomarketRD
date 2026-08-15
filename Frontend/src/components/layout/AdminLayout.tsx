import { Outlet, Link, useNavigate, useLocation } from "react-router-dom";
import {
  FaChartPie,
  FaUsers,
  FaCar,
  FaCoins,
  FaShieldAlt,
  FaSignOutAlt,
  FaMoneyCheckAlt,
  FaHeadset,
} from "react-icons/fa";
import { authService } from "../../services/auth.service";
import { useResumenTicketsAdmin } from "../../hooks/useTickets";
import logo from "../../assets/AutoMarketRD_Logo.svg";

const menuItems = [
  {
    path: "/admin",
    label: "Resumen",
    icon: <FaChartPie />,
  },
  {
    path: "/admin/usuarios",
    label: "Usuarios",
    icon: <FaUsers />,
  },
  {
    path: "/admin/anuncios",
    label: "Anuncios",
    icon: <FaCar />,
  },
  {
    path: "/admin/planes",
    label: "Planes",
    icon: <FaCoins />,
  },
  {
    path: "/admin/pagos",
    label: "Pagos",
    icon: <FaMoneyCheckAlt />,
  },
  {
    path: "/admin/soporte",
    label: "Soporte",
    icon: <FaHeadset />,
  },
];

export default function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();

  const usuario = authService.getCurrentUser();

  const { data: resumenTickets } = useResumenTicketsAdmin();

  const cantidadAbiertos = resumenTickets?.cantidadAbiertos ?? 0;

  const nombreUsuario = usuario
    ? `${usuario.nombre} ${usuario.apellido ?? ""}`.trim()
    : "Admin";

  const inicialUsuario = usuario?.nombre
    ? usuario.nombre.charAt(0).toUpperCase()
    : "A";

  const handleLogout = () => {
    authService.logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-[#f4f6f9] font-sans">
      {/* SIDEBAR */}
      <aside className="z-10 flex h-full w-[260px] flex-col bg-[#1b1226] text-white shadow-lg">
        <div className="flex h-[120px] flex-col items-center justify-center gap-1 px-4 py-3">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-[110px] w-[160px] object-contain"
          />
          <span className="inline-flex items-center gap-1.5 rounded-full bg-violet-600/30 px-3 py-0.5 text-xs font-semibold text-violet-200">
            <FaShieldAlt className="text-violet-300" />
            Panel de Administración
          </span>
        </div>

        <nav className="flex flex-1 flex-col gap-2 p-6">
          {menuItems.map((item) => {
            const esDashboardPrincipal = item.path === "/admin";

            const isActive = esDashboardPrincipal
              ? location.pathname === "/admin"
              : location.pathname === item.path ||
                location.pathname.startsWith(`${item.path}/`);

            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center rounded-lg px-4 py-3 font-medium transition-colors ${
                  isActive
                    ? "bg-violet-600 text-white"
                    : "text-[#a99fb8] hover:bg-white/5 hover:text-white"
                }`}
              >
                <span className="mr-3 text-lg">{item.icon}</span>
                <span>{item.label}</span>
                {item.path === "/admin/soporte" && cantidadAbiertos > 0 && (
                  <span className="ml-auto inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1.5 text-[10px] font-bold text-white">
                    {cantidadAbiertos}
                  </span>
                )}
              </Link>
            );
          })}
        </nav>

        <div className="border-t border-white/5 p-4">
          <button
            type="button"
            onClick={handleLogout}
            className="flex w-full items-center justify-center rounded-lg px-4 py-3 font-medium text-red-400 transition-colors hover:bg-red-500/10"
          >
            <FaSignOutAlt className="mr-3" />
            Cerrar Sesión
          </button>
        </div>
      </aside>

      {/* ÁREA PRINCIPAL */}
      <main className="flex h-full flex-1 flex-col overflow-hidden">
        <header className="flex h-[70px] shrink-0 items-center justify-between border-b border-gray-200 bg-white px-8">
          <h3 className="text-xl font-semibold text-gray-800">
            Administración AutoMarket RD
          </h3>

          <div className="flex items-center gap-3">
            <div className="hidden text-right sm:block">
              <p className="text-sm font-semibold text-gray-800">
                {nombreUsuario}
              </p>
              <p className="text-xs text-gray-500">Administrador</p>
            </div>

            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-violet-600 font-bold text-white">
              {inicialUsuario}
            </div>
          </div>
        </header>

        <div className="flex-1 overflow-y-auto p-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}