import type { AnuncioListado } from "../types/anuncio.types";

interface AnuncioCardProps {
  anuncio: AnuncioListado;
  onPublicar: (id: number) => void;
}

export default function AnuncioCard({ anuncio, onPublicar }: AnuncioCardProps) {
  const fotoPrincipal =
    anuncio.fotos && anuncio.fotos.length > 0
      ? anuncio.fotos[0]
      : "https://via.placeholder.com/300x200?text=Sin+Foto";

  return (
    <li className="p-4 sm:p-5 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div className="flex items-start gap-4 min-w-0">
        <img
          src={fotoPrincipal}
          alt={anuncio.nombreAnuncio}
          className="w-40 h-28 sm:w-48 sm:h-32 object-cover rounded-lg shadow-sm border border-gray-100 flex-shrink-0 bg-gray-50"
        />

        <div className="min-w-0">
          <h2 className="text-lg sm:text-xl font-bold text-gray-900 truncate">
            {anuncio.nombreAnuncio}
          </h2>

          <p className="text-gray-500 text-sm font-medium mt-1">
            {anuncio.ubicacion} ·{" "}
            <span className="text-blue-600">
              RD$ {anuncio.precio.toLocaleString()}
            </span>
          </p>

          <div className="mt-2 flex gap-2 flex-wrap">
            <span className="inline-block px-2.5 py-1 text-xs font-bold text-gray-800 bg-gray-100 rounded-md">
              {anuncio.estado}
            </span>

            <span className="inline-block px-2.5 py-1 text-xs font-bold text-green-800 bg-green-100 rounded-md">
              {anuncio.tipoVehiculo}
            </span>
          </div>
        </div>
      </div>

      <div className="flex gap-2 w-full sm:w-auto sm:items-center">
        {anuncio.estado !== "Publicado" && (
          <button
            onClick={() => onPublicar(anuncio.id)}
            className="flex-1 sm:flex-none bg-green-500 text-white px-4 py-2 rounded-md font-medium hover:bg-green-600 transition-colors"
          >
            Publicar
          </button>
        )}

        <button className="flex-1 sm:flex-none bg-gray-100 text-gray-700 border border-gray-300 px-4 py-2 rounded-md font-medium hover:bg-gray-200 transition-colors">
          Editar
        </button>
      </div>
    </li>
  );
}