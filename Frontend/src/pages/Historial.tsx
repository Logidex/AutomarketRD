import { Link, useNavigate } from "react-router-dom";
import { FaCalendarAlt, FaHistory, FaClock } from "react-icons/fa";
import { formatearFecha } from "../utils/fecha";
import { useHistorialReciente } from "../hooks/useHistorial";
import { urlImagen } from "../utils/imagen";

const IMAGEN_VACIA =
  "https://via.placeholder.com/600x400?text=Sin+Foto";

export default function Historial() {
  const navigate = useNavigate();

  const {
    data: recientes = [],
    isLoading: cargando,
    isError,
    refetch,
  } = useHistorialReciente(12);

  const mensajeError = isError ? "No se pudo cargar tu historial." : "";

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link
          to="/perfil"
          className="text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white"
        >
          ← Mi cuenta
        </Link>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <div className="mb-8">
          <h1 className="flex items-center gap-3 text-2xl font-bold">
            <FaHistory className="text-blue-400" />
            Vehículos recientes
            {recientes.length > 0 && (
              <span className="text-sm font-normal text-[#9aa1b1]">
                ({recientes.length})
              </span>
            )}
          </h1>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            Los últimos vehículos que has visto, para retomar tu búsqueda.
          </p>
        </div>

        {cargando ? (
          <div className="flex items-center justify-center py-24">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
          </div>
        ) : mensajeError ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-400">{mensajeError}</p>
            <button
              type="button"
              onClick={() => refetch()}
              className="mt-4 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
            >
              Reintentar
            </button>
          </div>
        ) : recientes.length === 0 ? (
          <div className="rounded-2xl border border-white/10 bg-[#13161d] p-16 text-center">
            <FaHistory className="mx-auto text-5xl text-gray-600" />
            <h2 className="mt-4 text-lg font-semibold">
              Aún no has visto vehículos
            </h2>
            <p className="mt-2 text-sm text-[#9aa1b1]">
              Cuando abras un vehículo desde la vitrina, aparecerá aquí.
            </p>
            <button
              type="button"
              onClick={() => navigate("/")}
              className="mt-6 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
            >
              Explorar vehículos
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {recientes.map((anuncio) => (
              <button
                key={`${anuncio.id}-${anuncio.vistoEnUtc}`}
                type="button"
                onClick={() => navigate(`/anuncio/${anuncio.id}`)}
                className="group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] text-left transition-all hover:border-blue-500/40 hover:shadow-lg"
              >
                <div className="relative aspect-[16/10] overflow-hidden">
                  <img
                    src={urlImagen(anuncio.fotoPrincipal) || IMAGEN_VACIA}
                    alt={`${anuncio.marca} ${anuncio.modelo}`}
                    className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                    onError={(e) => {
                      const img = e.currentTarget;
                      if (img.src !== IMAGEN_VACIA) img.src = IMAGEN_VACIA;
                    }}
                  />
                  <span className="absolute left-3 top-3 inline-flex items-center gap-1.5 rounded-full bg-black/60 px-2.5 py-1 text-xs font-medium text-white backdrop-blur">
                    <FaClock className="text-gray-300" />
                    {formatearFecha(anuncio.vistoEnUtc, true)}
                  </span>
                </div>

                <div className="flex flex-1 flex-col p-5">
                  <div className="flex items-start justify-between gap-3">
                    <h3 className="truncate text-lg font-bold">
                      {anuncio.marca} {anuncio.modelo}
                    </h3>
                    <span className="shrink-0 rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
                      {anuncio.anio}
                    </span>
                  </div>

                  <p className="mt-2 text-xl font-bold text-blue-500">
                    RD$ {anuncio.precio.toLocaleString("es-DO")}
                  </p>

                  <div className="mt-4 flex items-center gap-3 border-t border-white/10 pt-4 text-xs text-[#9aa1b1]">
                    <span className="inline-flex items-center gap-1.5">
                      <FaCalendarAlt className="text-gray-500" />
                      {anuncio.anio}
                    </span>
                    <span className="inline-flex items-center gap-1.5 text-blue-400 group-hover:text-blue-300">
                      Ver de nuevo →
                    </span>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}