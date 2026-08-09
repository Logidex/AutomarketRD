import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Swal from "sweetalert2";
import Spinner from "../../components/Spinner";
import { anuncioService } from "../../services/anuncio.service";
import type { AnuncioListado } from "../../types/anuncio.types";
import { getUserIdFromToken } from "../../utils/jwt.util";

export default function MiVehiculoVendedor() {
  const usuarioId = getUserIdFromToken();
  const [anuncio, setAnuncio] = useState<AnuncioListado | null>(null);
  const [cargando, setCargando] = useState(true);
  const [accion, setAccion] = useState<string | null>(null);

  useEffect(() => {
  const cargar = async () => {
    if (usuarioId === null) return;
    try {
      const resultado = await anuncioService.obtenerMisAnuncios(usuarioId);
      setAnuncio(resultado.items?.[0] ?? null);
    } catch {
      Swal.fire({
        title: "Error",
        text: "No se pudo cargar tu vehículo.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    } finally {
      setCargando(false);
    }
  };

  cargar();
}, [usuarioId]);

// Recarga el anuncio actual en pantalla, sin tocar el estado de "cargando".
const recargarAnuncio = async () => {
  if (usuarioId === null) return;
  try {
    const resultado = await anuncioService.obtenerMisAnuncios(usuarioId);
    setAnuncio(resultado.items?.[0] ?? null);
  } catch {
    Swal.fire({
      title: "Error",
      text: "No se pudo cargar tu vehículo.",
      icon: "error",
      confirmButtonColor: "#ef4444",
    });
  }
};

  if (usuarioId === null) {
    return (
      <p className="text-sm text-gray-500">
        No se pudo identificar tu cuenta.
      </p>
    );
  }

  if (cargando) {
    return <Spinner />;
  }

  const handlePublicar = async () => {
    if (!anuncio) return;
    setAccion("publicar");
    try {
      await anuncioService.publicarAnuncio(anuncio.id);
      await recargarAnuncio();
    } catch (error) {
      Swal.fire({
        title: "Error",
        text:
          error instanceof Error
            ? error.message
            : "No se pudo publicar el anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    } finally {
      setAccion(null);
    }
  };

  const handleEliminar = async () => {
    if (!anuncio) return;

    const resultado = await Swal.fire({
      title: "¿Eliminar tu vehículo?",
      text: "El anuncio y sus fotos se eliminarán. No se puede deshacer.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Sí, eliminar",
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#ef4444",
      cancelButtonColor: "#6b7280",
    });

    if (!resultado.isConfirmed) return;

    setAccion("eliminar");
    try {
      await anuncioService.eliminarAnuncio(anuncio.id);
      setAnuncio(null);
    } catch (error) {
      Swal.fire({
        title: "Error",
        text:
          error instanceof Error
            ? error.message
            : "No se pudo eliminar el anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    } finally {
      setAccion(null);
    }
  };

  if (!anuncio) {
    return (
      <div className="mx-auto max-w-2xl">
        <h1 className="mb-1 text-lg font-semibold text-gray-900">
          Mi Vehículo
        </h1>
        <p className="mb-6 text-sm text-gray-500">
          Publica tu vehículo para que los interesados puedan contactarte.
        </p>

        <div className="rounded-lg border border-dashed border-gray-300 bg-white p-8 text-center">
          <p className="mb-1 text-sm text-gray-600">
            Todavía no tienes ningún vehículo publicado.
          </p>
          <p className="mb-6 text-sm text-gray-400">
            Recuerda: puedes publicar solo un anuncio con tu cuenta.
          </p>

          <Link
            to="/vendedor/publicar"
            className="inline-flex items-center gap-2 rounded-md bg-gray-800 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-gray-700"
          >
            Publicar mi vehículo
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold text-gray-900">Mi Vehículo</h1>
          <p className="text-sm text-gray-500">
            Así te ven los compradores en la vitrina.
          </p>
        </div>
        <Link
          to={`/vendedor/editar-anuncio/${anuncio.id}`}
          className="rounded-md border border-gray-300 px-3 py-1.5 text-xs font-semibold text-gray-600 transition-colors hover:bg-gray-50"
        >
          Editar
        </Link>
      </div>

      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
        {/* FOTO */}
        {anuncio.fotos.length > 0 ? (
          <img
            src={anuncio.fotos[0]}
            alt={`${anuncio.marca} ${anuncio.modelo}`}
            className="h-52 w-full object-cover sm:h-64"
          />
        ) : (
          <div className="flex h-40 w-full items-center justify-center bg-gray-100 text-sm text-gray-400">
            Sin fotos todavía
          </div>
        )}

        <div className="space-y-4 p-5">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h2 className="text-lg font-semibold text-gray-900">
                {anuncio.marca} {anuncio.modelo} {anuncio.anio}
              </h2>
              <p className="text-sm text-gray-500">{anuncio.ubicacion}</p>
            </div>
            <BadgeEstado estado={anuncio.estado} />
          </div>

          <p className="text-xl font-bold text-gray-900">
            RD$ {anuncio.precio.toLocaleString("es-DO")}
          </p>

          <div className="flex items-center gap-4 text-sm text-gray-600">
            <span>{anuncio.kilometraje.toLocaleString("es-DO")} km</span>
            <span aria-hidden="true">·</span>
            <span>{anuncio.transmision}</span>
            <span aria-hidden="true">·</span>
            <span>{anuncio.combustible}</span>
          </div>

          {/* CONTADOR DE VISTAS */}
          <div className="flex items-center gap-2 rounded-md bg-gray-50 px-3 py-2.5 text-sm text-gray-600">
            <svg
              className="h-5 w-5 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={1.5}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z"
              />
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
              />
            </svg>
            <span>
              <strong className="font-semibold text-gray-800">
                {anuncio.vistas}
              </strong>{" "}
              persona{anuncio.vistas === 1 ? "" : "s"} han visto tu anuncio
            </span>
          </div>

          {/* ACCIONES */}
          <div className="flex flex-wrap items-center gap-3 border-t border-gray-100 pt-4">
            <Link
              to={`/vendedor/editar-anuncio/${anuncio.id}`}
              className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
            >
              Editar anuncio
            </Link>

            {anuncio.estado !== "Publicado" && (
              <button
                type="button"
                onClick={handlePublicar}
                disabled={accion !== null}
                className="rounded-md bg-gray-800 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-gray-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {accion === "publicar" ? "Publicando..." : "Publicar"}
              </button>
            )}

            <button
              type="button"
              onClick={handleEliminar}
              disabled={accion !== null}
              className="ml-auto rounded-md px-3 py-2 text-sm font-medium text-red-600 transition-colors hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Eliminar
            </button>
          </div>
        </div>
      </div>

      <p className="text-xs text-gray-400">
        Para publicar más de un vehículo, actualice a una cuenta de Dealer.
      </p>
    </div>
  );
}

function BadgeEstado({ estado }: { estado: string }) {
  const clases: Record<string, string> = {
    Publicado: "bg-emerald-50 text-emerald-700",
    Borrador: "bg-gray-100 text-gray-600",
    Vendido: "bg-blue-50 text-blue-700",
    Pausado: "bg-amber-50 text-amber-700",
  };

  return (
    <span
      className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${
        clases[estado] ?? "bg-gray-100 text-gray-600"
      }`}
    >
      {estado}
    </span>
  );
}