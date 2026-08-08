import { Outlet, Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";

import { FaCar, FaPlusCircle, FaChartPie, FaSignOutAlt, FaEnvelope, FaStore, FaCreditCard } from "react-icons/fa";

import { authService } from "../../services/auth.service";
import { suscripcionService, type SuscripcionDealer } from "../../services/suscripcion.service";
import logo from "../../assets/AutoMarketRD_Logo.svg";

export default function DashboardLayout() {
  const navigate = useNavigate();
  const location = useLocation();

  const usuario = authService.getCurrentUser();

  const nombreUsuario = usuario
    ? `${usuario.nombre} ${usuario.apellido ?? ""}`.trim()
    : "Usuario";

  const inicialUsuario = usuario?.nombre
    ? usuario.nombre.charAt(0).toUpperCase()
    : "U";

  const [suscripcion, setSuscripcion] = useState<SuscripcionDealer | null>(null);

  useEffect(() => {
    suscripcionService
      .obtenerSuscripcion()
      .then(setSuscripcion)
      .catch(() => setSuscripcion(null));
  }, [location.pathname]);

  const handleLogout = () => {
    authService.logout();

    navigate("/login", {
      replace: true,
    });
  };

  const menuItems = [
    {
      path: "/dashboard",
      label: "Resumen",
      icon: <FaChartPie />,
    },
    {
      path: "/dashboard/mis-anuncios",
      label: "Mi Inventario",
      icon: <FaCar />,
    },
    {
      path: "/dashboard/publicar",
      label: "Publicar Vehículo",
      icon: <FaPlusCircle />,
    },
    {
      path: "/dashboard/leads",
      label: "Leads",
      icon: <FaEnvelope />,
    },
    {
      path: "/dashboard/mi-perfil",
      label: "Mi Perfil",
      icon: <FaStore />,
    },
    {
      path: "/dashboard/suscripcion",
      label: "Suscripción",
      icon: <FaCreditCard />,
    },
  ];

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-[#f4f6f9] font-sans">
      {/* SIDEBAR */}
      <aside className="z-10 flex h-full w-[260px] flex-col bg-[#11141a] text-white shadow-lg">
        {/* LOGO */}
        <div className="flex h-[120px] items-center justify-center px-4 py-3">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-[145px] w-[210px] object-contain"
          />
        </div>

        {/* MENÚ PRINCIPAL */}
        <nav className="flex flex-1 flex-col gap-2 p-6">
          {menuItems.map((item) => {
            const esDashboardPrincipal = item.path === "/dashboard";

            const isActive = esDashboardPrincipal
              ? location.pathname === "/dashboard"
              : location.pathname === item.path ||
                location.pathname.startsWith(`${item.path}/`);

            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center rounded-lg px-4 py-3 font-medium transition-colors ${
                  isActive
                    ? "bg-blue-600 text-white"
                    : "text-[#8a94a6] hover:bg-white/5 hover:text-white"
                }`}
              >
                <span className="mr-3 text-lg">{item.icon}</span>

                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>

        {/* BOTÓN CERRAR SESIÓN */}
        <div className="border-t border-white/5 p-4">
          <button
            type="button"
            onClick={handleLogout}
            className="flex w-full items-center justify-center rounded-lg px-4 py-3 font-medium text-red-500 transition-colors hover:bg-red-500/10"
          >
            <FaSignOutAlt className="mr-3" />
            Cerrar Sesión
          </button>
        </div>
      </aside>

      {/* ÁREA PRINCIPAL */}
      <main className="flex h-full flex-1 flex-col overflow-hidden">
        {/* HEADER */}
        <header className="flex h-[70px] shrink-0 items-center justify-between border-b border-gray-200 bg-white px-8">
          <h3 className="text-xl font-semibold text-gray-800">
            Panel de Control
          </h3>

          {/* INFORMACIÓN DEL USUARIO */}
          <div className="flex items-center gap-3">
            {/* INDICADOR DE PLAN */}
            <PlanBadge suscripcion={suscripcion} />

            <div className="hidden text-right sm:block">
              <p className="text-sm font-semibold text-gray-800">
                {nombreUsuario}
              </p>

              {usuario?.rol && (
                <p className="text-xs text-gray-500">{usuario.rol}</p>
              )}
            </div>

            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-600 font-bold text-white">
              {inicialUsuario}
            </div>
          </div>
        </header>

        {/* CONTENIDO DINÁMICO */}
        <div className="flex-1 overflow-y-auto p-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

function PlanBadge({ suscripcion }: { suscripcion: SuscripcionDealer | null }) {
  if (!suscripcion) {
    return (
      <Link
        to="/dashboard/suscripcion"
        className="hidden rounded-full border border-gray-300 px-3 py-1.5 text-xs font-semibold text-gray-600 transition-colors hover:border-blue-400 hover:text-blue-600 sm:block"
      >
        Suscríbete
      </Link>
    );
  }

  const nombrePlan = nombrePlanKey(suscripcion.nivel);
  const esCancelada = suscripcion.estado === "Cancelada";
  const vencida = !esCancelada && suscripcion.diasRestantes <= 0;

  const colorClase = esCancelada || vencida ? "border-red-300 text-red-600" : "border-green-300 text-green-700";

  return (
    <Link
      to="/dashboard/suscripcion"
      className={`hidden rounded-full border px-3 py-1.5 text-xs font-semibold transition-colors sm:block ${colorClase} hover:bg-gray-50`}
      title={`Plan ${nombrePlan}`}
    >
      Plan {nombrePlan}
      {!esCancelada && !vencida && (
        <span className="ml-1 opacity-70">· {suscripcion.diasRestantes}d</span>
      )}
      {esCancelada && <span className="ml-1 opacity-70">cancelada</span>}
      {vencida && <span className="ml-1 opacity-70">vencida</span>}
    </Link>
  );
}

function nombrePlanKey(nivel: string): string {
  const mapa: Record<string, string> = {
    Gratis: "Gratis",
    Basico: "Básico",
    Pro: "Pro",
    Elite: "Elite",
  };
  return mapa[nivel] ?? nivel;
}
