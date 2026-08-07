import { useNavigate } from "react-router-dom";
import type { AnuncioListado } from "../types/anuncio.types";

interface AnuncioCardProps {
  anuncio: AnuncioListado;
  onPublicar: (id: number) => void;
  onCambiarEstado: (id: number, estado: string) => void;
  onEditar?: (id: number) => void;
}

export default function AnuncioCard({
  anuncio,
  onPublicar,
  onCambiarEstado,
}: AnuncioCardProps) {
  const fotoPrincipal =
    anuncio.fotos && anuncio.fotos.length > 0
      ? anuncio.fotos[0]
      : "https://via.placeholder.com/300x200?text=Sin+Foto";

  const estadoNormalizado = (anuncio.estado ?? "").trim();
  const estadoLower = estadoNormalizado.toLowerCase();
  const esPublicado = estadoLower === "publicado";

  const nombreAnuncio = anuncio.nombreAnuncio || "Sin nombre";
  const ubicacion = anuncio.ubicacion || "Sin ubicación";
  const tipoVehiculo = anuncio.tipoVehiculo || "Sin tipo";
  const transmision = anuncio.transmision || "Sin transmisión";
  const combustible = anuncio.combustible || "Sin combustible";
  const precio = Number(anuncio.precio ?? 0);
  const kilometraje = Number(anuncio.kilometraje ?? 0);
  const navigate = useNavigate();

  const getEstadoStyle = (estado: string) => {
    const value = estado.trim().toLowerCase();

    switch (value) {
      case "publicado":
        return "bg-green-100 text-green-800 border-green-200";
      case "borrador":
        return "bg-gray-100 text-gray-700 border-gray-200";
      case "pausado":
        return "bg-yellow-100 text-yellow-800 border-yellow-200";
      case "vendido":
        return "bg-blue-100 text-blue-800 border-blue-200";
      default:
        return "bg-purple-100 text-purple-800 border-purple-200";
    }
  };

  const getSelectStyle = (estado: string) => {
    const value = estado.trim().toLowerCase();

    switch (value) {
      case "publicado":
        return "border-green-300 bg-green-50 text-green-800 focus:ring-green-500";
      case "borrador":
        return "border-gray-300 bg-gray-50 text-gray-700 focus:ring-gray-500";
      case "pausado":
        return "border-yellow-300 bg-yellow-50 text-yellow-800 focus:ring-yellow-500";
      case "vendido":
        return "border-blue-300 bg-blue-50 text-blue-800 focus:ring-blue-500";
      default:
        return "border-purple-300 bg-purple-50 text-purple-800 focus:ring-purple-500";
    }
  };

  return (
    <li className="overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md">
      <div className="flex flex-col lg:flex-row">
        <div className="relative w-full lg:w-64 flex-shrink-0">
          <img
            src={fotoPrincipal}
            alt={nombreAnuncio}
            className="h-56 w-full object-cover lg:h-full lg:min-h-[240px]"
          />
          <div className="absolute left-3 top-3">
            <span
              className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${getEstadoStyle(
                estadoNormalizado,
              )}`}
            >
              {estadoNormalizado || "Sin estado"}
            </span>
          </div>
        </div>

        <div className="flex-1 p-4 sm:p-5">
          <div className="flex flex-col gap-4">
            <div className="min-w-0">
              <h2 className="truncate text-lg font-bold text-gray-900 sm:text-xl">
                {nombreAnuncio}
              </h2>

              <div className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-gray-500">
                <span>{ubicacion}</span>
                <span>•</span>
                <span className="font-semibold text-blue-600">
                  RD$ {precio.toLocaleString()}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
              <div className="rounded-xl border border-gray-100 bg-gray-50 p-3">
                <p className="text-[11px] uppercase tracking-wide text-gray-500">
                  Tipo
                </p>
                <p className="mt-1 truncate text-sm font-semibold text-gray-800">
                  {tipoVehiculo}
                </p>
              </div>

              <div className="rounded-xl border border-gray-100 bg-gray-50 p-3">
                <p className="text-[11px] uppercase tracking-wide text-gray-500">
                  Kilometraje
                </p>
                <p className="mt-1 text-sm font-semibold text-gray-800">
                  {kilometraje.toLocaleString()} km
                </p>
              </div>

              <div className="rounded-xl border border-gray-100 bg-gray-50 p-3">
                <p className="text-[11px] uppercase tracking-wide text-gray-500">
                  Transmisión
                </p>
                <p className="mt-1 truncate text-sm font-semibold text-gray-800">
                  {transmision}
                </p>
              </div>

              <div className="rounded-xl border border-gray-100 bg-gray-50 p-3">
                <p className="text-[11px] uppercase tracking-wide text-gray-500">
                  Combustible
                </p>
                <p className="mt-1 truncate text-sm font-semibold text-gray-800">
                  {combustible}
                </p>
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              <span className="inline-flex rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">
                {tipoVehiculo}
              </span>
              <span className="inline-flex rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-700">
                {ubicacion}
              </span>
              <span className="inline-flex rounded-full bg-green-50 px-3 py-1 text-xs font-semibold text-green-700">
                {transmision}
              </span>
            </div>

            <div className="flex flex-col gap-3 border-t border-gray-100 pt-4 sm:flex-row sm:items-end sm:justify-between">
              <div className="w-full sm:w-60">
                <label className="mb-1 block text-xs font-medium text-gray-500">
                  Cambiar estado
                </label>

                <div className="relative">
                  <select
                    value={estadoNormalizado}
                    onChange={(e) => onCambiarEstado(anuncio.id, e.target.value)}
                    className={`w-full appearance-none rounded-lg border px-3 py-2.5 pr-10 text-sm font-medium shadow-sm transition-all focus:outline-none focus:ring-2 focus:border-transparent ${getSelectStyle(
                      estadoNormalizado,
                    )}`}
                  >
                    <option value="Publicado">Publicado</option>
                    <option value="Borrador">Borrador</option>
                    <option value="Pausado">Pausado</option>
                    <option value="Vendido">Vendido</option>
                  </select>

                  <svg
                    className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-current opacity-60"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      d="M19 9l-7 7-7-7"
                    />
                  </svg>
                </div>
              </div>

              <div className="flex flex-wrap gap-2 sm:justify-end">
                {!esPublicado && (
                  <button
                    onClick={() => onPublicar(anuncio.id)}
                    className="rounded-lg bg-green-600 px-4 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-green-700"
                  >
                    Publicar
                  </button>
                )}

                <button
                  onClick={() => navigate(`/dashboard/editar-anuncio/${anuncio.id}`)}
                  className="rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50"
                >
                  Editar
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </li>
  );
}