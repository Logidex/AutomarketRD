import { Link, useNavigate } from "react-router-dom";
import {
  FaEnvelope,
  FaHeart,
  FaHistory,
  FaSignOutAlt,
  FaUserCircle,
  FaUserEdit,
} from "react-icons/fa";
import { authService } from "../services/auth.service";
import AscenderRol from "../components/AscenderRol";

export default function MiCuenta() {
  const navigate = useNavigate();
  const usuario = authService.getCurrentUser();

  if (!usuario) return null;

  const nombreCompleto =
    `${usuario.nombre} ${usuario.apellido ?? ""}`.trim();
  const inicial = usuario.nombre.charAt(0).toUpperCase();

  const handleLogout = () => {
    authService.logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="min-h-screen bg-page text-ink">
      <header className="flex items-center justify-between border-b border-line px-6 py-5 sm:px-8">
        <Link to="/" className="text-sm font-medium text-ink-2 transition-colors hover:text-ink">
          ← Volver al inicio
        </Link>
      </header>

      <main className="mx-auto max-w-lg px-6 py-12 sm:px-8">
        <div className="flex flex-col items-center text-center">
          <div className="flex h-24 w-24 items-center justify-center rounded-full bg-blue-500 text-4xl font-bold text-white">
            {inicial}
          </div>
          <h1 className="mt-4 text-2xl font-bold">{nombreCompleto}</h1>
          <p className="mt-1 text-sm text-ink-2">{usuario.email}</p>
          <span className="mt-3 rounded-full bg-blue-500/10 px-3 py-1 text-xs font-semibold text-blue-400">
            Comprador
          </span>
        </div>

        <div className="mt-8 flex flex-col gap-3 rounded-2xl border border-line bg-surface p-6 text-sm text-ink-2">
          <p>
            Tu actividad del comprador: <strong className="text-ink-2">favoritos</strong>,
            <strong className="text-ink-2"> vehículos recientes</strong>, los{" "}
            <strong className="text-ink-2">vehículos que contactaste</strong> y la actualización
            de tus <strong className="text-ink-2">datos de cuenta</strong>.
          </p>

          <div className="mt-2 grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Link
              to="/perfil/favoritos"
              className="flex items-center justify-center gap-2 rounded-lg bg-hover px-5 py-3 font-semibold text-ink transition-colors hover:bg-blue-500"
            >
              <FaHeart className="text-red-400" />
              Mis Favoritos
            </Link>
            <Link
              to="/perfil/historial"
              className="flex items-center justify-center gap-2 rounded-lg bg-hover px-5 py-3 font-semibold text-ink transition-colors hover:bg-blue-500"
            >
              <FaHistory className="text-blue-400" />
              Vehículos Recientes
            </Link>
            <Link
              to="/perfil/contactados"
              className="flex items-center justify-center gap-2 rounded-lg bg-hover px-5 py-3 font-semibold text-ink transition-colors hover:bg-blue-500"
            >
              <FaEnvelope className="text-green-400" />
              Contactos enviados
            </Link>
            <Link
              to="/perfil/editar"
              className="flex items-center justify-center gap-2 rounded-lg bg-hover px-5 py-3 font-semibold text-ink transition-colors hover:bg-blue-500 sm:col-span-2"
            >
              <FaUserEdit className="text-yellow-400" />
              Editar mis datos
            </Link>
          </div>
        </div>

        <div className="mt-8 flex flex-col gap-3 rounded-2xl border border-line bg-surface p-6 text-sm text-ink-2">
          <AscenderRol />
        </div>

        <button
          type="button"
          onClick={handleLogout}
          className="mt-8 flex w-full items-center justify-center gap-2 rounded-lg border border-red-500/30 bg-red-500/10 px-5 py-3 font-semibold text-red-400 transition-colors hover:bg-red-500/20"
        >
          <FaSignOutAlt />
          Cerrar sesión
        </button>

        <p className="mt-6 text-center text-xs text-ink-3">
          <FaUserCircle className="mr-1 inline" />
          AutoMarket RD — Cuenta de comprador
        </p>
      </main>
    </div>
  );
}