import { Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";

import { FaBars, FaCar, FaHome, FaPlusCircle, FaChartPie, FaSignOutAlt, FaEnvelope, FaStore, FaCreditCard, FaPaperPlane, FaHeadset, FaTimes } from "react-icons/fa";

import { authService } from "../../services/auth.service";
import { suscripcionService, type SuscripcionDealer } from "../../services/suscripcion.service";
import { nombrePlan as nombrePlanUtil } from "../../constants/planes";
import { confirmarCierreSesion } from "../../utils/confirmarCierreSesion";
import { iniciarTourDealer } from "../../utils/tourDealer";
import { usePerfilDealer } from "../../hooks/usePerfilDealer";
import { urlImagen } from "../../utils/imagen";
import { ROLES } from "../../constants/roles";
import logo from "../../assets/AutoMarketRD_Logo.svg";
import CampanaNotificaciones from "./CampanaNotificaciones";
import BotonTema from "../BotonTema";
import BannerConfirmarCorreo from "../BannerConfirmarCorreo";
import BannerPerfilIncompleto from "../BannerPerfilIncompleto";
import OutletAnimada from "../OutletAnimada";

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
    path: "/dashboard/contactados",
    label: "Contactados",
    icon: <FaPaperPlane />,
  },
  {
    path: "/dashboard/soporte",
    label: "Soporte",
    icon: <FaHeadset />,
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

  // Perfil del dealer: nombre de la agencia y logo para el header
  const { data: perfilDealer } = usePerfilDealer(usuario?.usuarioId ?? null);
  const logoDealer = perfilDealer?.logoUrl ?? null;
  const nombreMostrado =
    perfilDealer?.nombreAgencia?.trim() || nombreUsuario;

  const [suscripcion, setSuscripcion] = useState<SuscripcionDealer | null>(null);
  const [menuAbierto, setMenuAbierto] = useState(false);

  useEffect(() => {
    suscripcionService
      .obtenerSuscripcion()
      .then(setSuscripcion)
      .catch(() => setSuscripcion(null));
  }, [location.pathname]);

  // Tour de bienvenida para dealers: una sola vez, en la entrada al panel
  // y solo en escritorio (en móvil la sidebar está oculta).
  useEffect(() => {
    if (!usuario || usuario.rol !== ROLES.DEALER) return;
    if (location.pathname !== "/dashboard") return;

    const t = setTimeout(() => iniciarTourDealer(usuario.usuarioId), 600);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [usuario?.usuarioId, usuario?.rol, location.pathname]);

  // El drawer se cierra al hacer clic en cualquier enlace del menú
  const cerrarMenu = () => setMenuAbierto(false);

  const handleLogout = async () => {
    if (!(await confirmarCierreSesion())) return;

    authService.logout();

    navigate("/login", {
      replace: true,
    });
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
        className={`z-40 flex h-full w-[260px] max-lg:fixed max-lg:inset-y-0 max-lg:left-0 flex-col bg-[#11141a] text-white shadow-lg transition-transform duration-200 max-lg:transition-transform ${
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

        {/* LOGO */}
        <div className="flex h-[120px] items-center justify-center px-4 py-3">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-[145px] w-[210px] object-contain"
          />
        </div>

        {/* MENÚ PRINCIPAL */}
        <nav className="scroll-fino flex min-h-0 flex-1 flex-col gap-2 overflow-y-auto p-6">
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
                onClick={cerrarMenu}
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

        {/* ACCIONES DEL SIDEBAR (compactas para pantallas bajas) */}
        <div className="space-y-1 border-t border-white/5 p-3">
          <Link
            to="/"
            onClick={cerrarMenu}
            className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-[#8a94a6] transition-colors hover:bg-white/5 hover:text-white"
          >
            <FaHome />
            Volver al inicio
          </Link>
          <button
            type="button"
            onClick={handleLogout}
            className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-red-500 transition-colors hover:bg-red-500/10"
          >
            <FaSignOutAlt />
            Cerrar Sesión
          </button>
        </div>
      </aside>

      {/* ÁREA PRINCIPAL */}
      <main className="flex h-full flex-1 flex-col overflow-hidden">
        {/* HEADER */}
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
              Panel de Control
            </h3>
          </div>

          {/* INFORMACIÓN DEL USUARIO */}
          <div className="flex items-center gap-3">
            {/* BOTÓN DE TEMA */}
            <BotonTema />

            {/* NOTIFICACIONES DE LEADS */}
            <CampanaNotificaciones rutaLeads="/dashboard/leads" tema="claro" />

            {/* INDICADOR DE PLAN */}
            <PlanBadge suscripcion={suscripcion} />

            <div className="hidden min-w-0 text-right sm:block">
              <p className="truncate text-sm font-semibold text-ink">
                {nombreMostrado}
              </p>

              {usuario?.rol && (
                <p className="text-xs text-ink-3">{usuario.rol}</p>
              )}
            </div>

            {logoDealer ? (
              <img
                src={urlImagen(logoDealer)}
                alt={nombreMostrado}
                className="h-10 w-10 shrink-0 rounded-full border border-line object-cover"
              />
            ) : (
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-blue-600 font-bold text-white">
                {inicialUsuario}
              </div>
            )}
          </div>
        </header>

        {/* CONTENIDO DINÁMICO */}
        <div className="scroll-fino flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">
          <BannerConfirmarCorreo />
          {usuario?.rol === ROLES.DEALER && perfilDealer && (
            <BannerPerfilIncompleto usuarioId={usuario.usuarioId} perfil={perfilDealer} />
          )}
          <OutletAnimada />
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
        className="hidden rounded-full border border-line px-3 py-1.5 text-xs font-semibold text-ink-2 transition-colors hover:border-blue-400 hover:text-blue-600 sm:block"
      >
        Suscríbete
      </Link>
    );
  }

  const nombrePlan = nombrePlanUtil(suscripcion.nivel);
  const esGratis = suscripcion.nivel === "Gratis";
  const esCancelada = suscripcion.estado === "Cancelada";
  const vencida = !esGratis && !esCancelada && suscripcion.diasRestantes <= 0;

  const colorClase = esCancelada || vencida ? "border-red-300 text-red-600" : "border-green-300 text-green-700";

  return (
    <Link
      to="/dashboard/suscripcion"
      className={`hidden rounded-full border px-3 py-1.5 text-xs font-semibold transition-colors sm:block ${colorClase} hover:bg-hover`}
      title={`Plan ${nombrePlan}`}
    >
      Plan {nombrePlan}
      {!esGratis && !esCancelada && !vencida && (
        <span className="ml-1 opacity-70">· {suscripcion.diasRestantes}d</span>
      )}
      {esCancelada && <span className="ml-1 opacity-70">cancelada</span>}
      {vencida && <span className="ml-1 opacity-70">vencida</span>}
    </Link>
  );
}
