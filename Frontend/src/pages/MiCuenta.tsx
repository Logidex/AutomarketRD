import { Link, useNavigate } from "react-router-dom";
import { FaHeart, FaSignOutAlt, FaUserCircle } from "react-icons/fa";
import { authService } from "../services/auth.service";

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
    <div className="min-h-screen bg-[#0c101b] text-white">
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link to="/" className="text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white">
          ← Volver al inicio
        </Link>
      </header>

      <main className="mx-auto max-w-lg px-6 py-12 sm:px-8">
        <div className="flex flex-col items-center text-center">
          <div className="flex h-24 w-24 items-center justify-center rounded-full bg-blue-500 text-4xl font-bold text-white">
            {inicial}
          </div>
          <h1 className="mt-4 text-2xl font-bold">{nombreCompleto}</h1>
          <p className="mt-1 text-sm text-[#9aa1b1]">{usuario.email}</p>
          <span className="mt-3 rounded-full bg-blue-500/10 px-3 py-1 text-xs font-semibold text-blue-400">
            Comprador
          </span>
        </div>

        <div className="mt-8 flex flex-col gap-3 rounded-2xl border border-white/10 bg-[#13161d] p-6 text-sm text-[#9aa1b1]">
          <p>
            Tu actividad del comprador: <strong className="text-gray-200">favoritos</strong>,
            <strong className="text-gray-200"> búsquedas recientes</strong> y la opción
            de <strong className="text-gray-200">editar tus datos</strong>.
          </p>
          <p className="text-xs text-[#6b7280]">
            Proximamente: historial de búsquedas y edición de tu usuario.
          </p>

          <div className="mt-2 grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Link
              to="/perfil/favoritos"
              className="flex items-center justify-center gap-2 rounded-lg bg-white/5 px-5 py-3 font-semibold text-white transition-colors hover:bg-blue-500"
            >
              <FaHeart className="text-red-400" />
              Mis Favoritos
            </Link>
            <button
              type="button"
              title="Próximamente"
              className="flex cursor-not-allowed items-center justify-center gap-2 rounded-lg border border-white/10 px-5 py-3 font-semibold text-[#6b7280]"
            >
              Mi Historial
            </button>
          </div>
        </div>

        <button
          type="button"
          onClick={handleLogout}
          className="mt-8 flex w-full items-center justify-center gap-2 rounded-lg border border-red-500/30 bg-red-500/10 px-5 py-3 font-semibold text-red-400 transition-colors hover:bg-red-500/20"
        >
          <FaSignOutAlt />
          Cerrar sesión
        </button>

        <p className="mt-6 text-center text-xs text-[#6b7280]">
          <FaUserCircle className="mr-1 inline" />
          AutoMarket RD — Cuenta de comprador
        </p>
      </main>
    </div>
  );
}