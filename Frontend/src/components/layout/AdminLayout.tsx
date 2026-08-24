import { useState } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import {
  FaBars,
  FaChartPie,
  FaTimes,
  FaUsers,
  FaCar,
  FaCoins,
  FaShieldAlt,
  FaSignOutAlt,
  FaMoneyCheckAlt,
  FaHeadset,
  FaFlag,
  FaClipboardList,
} from "react-icons/fa";
import { authService } from "../../services/auth.service";
import { useResumenTicketsAdmin } from "../../hooks/useTickets";
import { useContarReportesPendientes } from "../../hooks/useAdmin";
import { confirmarCierreSesion } from "../../utils/confirmarCierreSesion";
import logo from "../../assets/AutoMarketRD_Logo.svg";
import BotonTema from "../BotonTema";
import OutletAnimada from "../OutletAnimada";

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
    path: "/admin/reportes",
    label: "Reportes",
    icon: <FaFlag />,
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
    path: "/admin/encuestas",
    label: "Encuestas",
    icon: <FaClipboardList />,
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
  const [menuAbierto, setMenuAbierto] = useState(false);

  // El drawer se cierra al hacer clic en cualquier enlace del menú
  const cerrarMenu = () => setMenuAbierto(false);

  const usuario = authService.getCurrentUser();

  const { data: resumenTickets } = useResumenTicketsAdmin();
  const { data: contadorReportes } = useContarReportesPendientes();

  const cantidadAbiertos = resumenTickets?.cantidadAbiertos ?? 0;
  const reportesPendientes = contadorReportes?.total ?? 0;

  const nombreUsuario = usuario
    ? `${usuario.nombre} ${usuario.apellido ?? ""}`.trim()
    : "Admin";

  const inicialUsuario = usuario?.nombre
    ? usuario.nombre.charAt(0).toUpperCase()
    : "A";

  const handleLogout = async () => {
    if (!(await confirmarCierreSesion())) return;
    authService.logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-page font-sans">
      {/* BACKDROP MÓVIL */}
      {menuAbierto && (
        <div
          className="fixed inset-0 z-30 bg-black/50 lg:hidden"
          onClick={() => setMenuAbierto(false)}
          aria-hidden="true"
        />
      )}

      {/* SIDEBAR (drawer en móvil, fija en escritorio) */}
      <aside
        className={`z-40 flex h-full w-[260px] max-lg:fixed max-lg:inset-y-0 max-lg:left-0 flex-col bg-[#1b1226] text-white shadow-lg transition-transform duration-200 ${
          menuAbierto ? "max-lg:translate-x-0" : "max-lg:-translate-x-full"
        }`}
      >
        {/* CERRAR (móvil) */}
        <button
          type="button"
          onClick={() => setMenuAbierto(false)}
          aria-label="Cerrar menú"
          className="absolute right-3 top-3 rounded-lg p-2 text-white/70 hover:bg-white/10 lg:hidden"
        >
          <FaTimes />
        </button>

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

        <nav className="scroll-fino flex min-h-0 flex-1 flex-col gap-2 overflow-y-auto p-6">
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
                onClick={cerrarMenu}
                className={`flex items-center rounded-lg px-4 py-3 font-medium transition-colors ${
                  isActive
                    ? "bg-violet-600 text-white"
                    : "text-[#a99fb8] hover:bg-white/5 hover:text-white"
                }`}
              >
                <span className="mr-3 text-lg">{item.icon}</span>
                <span>{item.label}</span>
                {item.path === "/admin/reportes" && reportesPendientes > 0 && (
                  <span className="ml-auto inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1.5 text-[10px] font-bold text-white">
                    {reportesPendientes}
                  </span>
                )}
                {item.path === "/admin/soporte" && cantidadAbiertos > 0 && (
                  <span className="ml-auto inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1.5 text-[10px] font-bold text-white">
                    {cantidadAbiertos}
                  </span>
                )}
              </Link>
            );
          })}
        </nav>

        {/* CERRAR SESIÓN (compacto para pantallas bajas) */}
        <div className="border-t border-white/5 p-3">
          <button
            type="button"
            onClick={handleLogout}
            className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-red-400 transition-colors hover:bg-red-500/10"
          >
            <FaSignOutAlt />
            Cerrar Sesión
          </button>
        </div>
      </aside>

      {/* ÁREA PRINCIPAL */}
      <main className="flex h-full flex-1 flex-col overflow-hidden">
        <header className="flex h-[70px] shrink-0 items-center justify-between border-b border-line bg-surface px-4 sm:px-6 lg:px-8">
          <div className="flex min-w-0 items-center gap-2">
            {/* HAMBURGUESA (móvil) */}
            <button
              type="button"
              onClick={() => setMenuAbierto(true)}
              aria-label="Abrir menú"
              className="rounded-lg p-2 text-ink-2 transition-colors hover:bg-hover lg:hidden"
            >
              <FaBars className="text-lg" />
            </button>
            <h3 className="truncate text-base font-semibold text-ink sm:text-xl">
              Administración AutoMarket RD
            </h3>
          </div>

          <div className="flex items-center gap-3">
            <BotonTema />

            <div className="hidden text-right sm:block">
              <p className="text-sm font-semibold text-ink">
                {nombreUsuario}
              </p>
              <p className="text-xs text-ink-3">Administrador</p>
            </div>

            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-violet-600 font-bold text-white">
              {inicialUsuario}
            </div>
          </div>
        </header>

        <div className="scroll-fino flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">
          <OutletAnimada />
        </div>
      </main>
    </div>
  );
}