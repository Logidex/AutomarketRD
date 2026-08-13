import { Link, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import {
  FaHeart,
  FaLocationArrow,
  FaCalendarAlt,
} from "react-icons/fa";
import { useMisFavoritos, useQuitarFavorito } from "../hooks/useFavoritos";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";

const IMAGEN_VACIA =
  "https://via.placeholder.com/600x400?text=Sin+Foto";

export default function Favoritos() {
  const navigate = useNavigate();

  const { data: favoritos = [], isLoading: cargando, isError, refetch } = useMisFavoritos();
  const quitarFavorito = useQuitarFavorito();

  const quitar = async (anuncioId: number) => {
    try {
      await quitarFavorito.mutateAsync(anuncioId);
    } catch (err) {
      void Swal.fire({
        icon: "error",
        title: "Error",
        text: err instanceof Error ? err.message : "No se pudo quitar el favorito.",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const mensajeError = isError ? "No se pudieron cargar tus favoritos." : "";

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
            <FaHeart className="text-red-500" />
            Mis favoritos
            {favoritos.length > 0 && (
              <span className="text-sm font-normal text-[#9aa1b1]">
                ({favoritos.length})
              </span>
            )}
          </h1>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            Vehículos que guardaste para revisar o comparar más tarde.
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
        ) : favoritos.length === 0 ? (
          <div className="rounded-2xl border border-white/10 bg-[#13161d] p-16 text-center">
            <FaHeart className="mx-auto text-5xl text-gray-600" />
            <h2 className="mt-4 text-lg font-semibold">
              Aún no tienes favoritos
            </h2>
            <p className="mt-2 text-sm text-[#9aa1b1]">
              Pulsa el corazón en un vehículo para guardarlo aquí.
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
            {favoritos.map((favorito) => (
              <div
                key={favorito.id}
                className="group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] transition-all hover:border-red-500/30 hover:shadow-lg"
              >
                <button
                  type="button"
                  onClick={() => navigate(`/anuncio/${favorito.id}`)}
                  className="relative aspect-[16/10] overflow-hidden text-left"
                >
                  <img
                    src={urlImagen(favorito.fotoPrincipal) || IMAGEN_VACIA}
                    alt={`${favorito.marca} ${favorito.modelo}`}
                    className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                    onError={(e) => {
                      const img = e.currentTarget;
                      if (img.src !== IMAGEN_VACIA) img.src = IMAGEN_VACIA;
                    }}
                  />
                </button>

                <div className="flex flex-1 flex-col p-5">
                  <div className="flex items-start justify-between gap-3">
                    <h3 className="truncate text-lg font-bold">
                      {favorito.marca} {favorito.modelo}
                    </h3>
                    <span className="shrink-0 rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
                      {favorito.anio}
                    </span>
                  </div>

                  <p className="mt-2 text-xl font-bold text-blue-500">
                    {formatearPrecio(favorito.precio, favorito.moneda)}
                  </p>

                  <div className="mt-4 flex items-center gap-3 border-t border-white/10 pt-4">
                    <span className="inline-flex items-center gap-1.5 text-xs text-[#9aa1b1]">
                      <FaCalendarAlt className="text-gray-500" />
                      {favorito.anio}
                    </span>
                    <span className="inline-flex items-center gap-1.5 text-xs text-[#9aa1b1]">
                      <FaLocationArrow className="text-gray-500" />
                      Ver detalle
                    </span>
                  </div>

                  <div className="mt-4 flex gap-2">
                    <button
                      type="button"
                      onClick={() => navigate(`/anuncio/${favorito.id}`)}
                      className="flex-1 rounded-lg bg-blue-500 px-4 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
                    >
                      Ver anuncio
                    </button>
                    <button
                      type="button"
                      onClick={() => quitar(favorito.id)}
                      title="Quitar de favoritos"
                      className="rounded-lg border border-white/10 px-4 py-2 text-sm text-[#9aa1b1] transition-colors hover:border-red-500/40 hover:text-red-400"
                    >
                      <FaHeart />
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}