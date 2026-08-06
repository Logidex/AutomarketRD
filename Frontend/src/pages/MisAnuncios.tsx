import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import AnuncioCard from "../components/AnuncioCard";
import { anuncioService } from "../services/anuncio.service";
import type { AnuncioListado } from "../types/anuncio.types";
import { getUserIdFromToken } from "../utils/jwt.util";
import { Link } from "react-router-dom";
import { FaCar, FaPlusCircle } from "react-icons/fa";

export default function MisAnuncios() {
  const [anuncios, setAnuncios] = useState<AnuncioListado[]>([]);
  const [cargando, setCargando] = useState(true);
  const usuarioId = getUserIdFromToken();

  useEffect(() => {
    if (usuarioId === null) return;

    const fetchAnuncios = async () => {
      try {
        setCargando(true);
        const response = await anuncioService.obtenerMisAnuncios(usuarioId);
        setAnuncios(response.items ?? []);
      } catch (error) {
        console.error(error);
        Swal.fire({
          title: "Error",
          text: "No se pudieron cargar los anuncios.",
          icon: "error",
          confirmButtonColor: "#ef4444",
        });
      } finally {
        setCargando(false);
      }
    };

    fetchAnuncios();
  }, [usuarioId]);

  if (usuarioId === null) {
    return (
      <div className="mx-auto max-w-6xl p-6">
        <p className="text-gray-500">No se pudo identificar al usuario.</p>
      </div>
    );
  }

  if (cargando) {
    return (
      <div className="mx-auto max-w-6xl p-6">
        <p className="text-gray-500">Cargando anuncios...</p>
      </div>
    );
  }

  const handlePublicar = async (id: number) => {
    try {
      await anuncioService.publicarAnuncio(id);

      setAnuncios((prev) =>
        prev.map((anuncio) =>
          anuncio.id === id ? { ...anuncio, estado: "Publicado" } : anuncio,
        ),
      );

      Swal.fire({
        title: "Publicado",
        text: "El anuncio fue publicado correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: "No se pudo publicar el anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const handleCambiarEstado = async (id: number, nuevoEstado: string) => {
    try {
      const result = await Swal.fire({
        title: "¿Cambiar estado?",
        text: `Vas a cambiar el anuncio a "${nuevoEstado}".`,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Sí, cambiar",
        cancelButtonText: "Cancelar",
        confirmButtonColor: "#2563eb",
        cancelButtonColor: "#6b7280",
      });

      if (!result.isConfirmed) return;

      await anuncioService.cambiarEstado(id, nuevoEstado);

      setAnuncios((prev) =>
        prev.map((anuncio) =>
          anuncio.id === id ? { ...anuncio, estado: nuevoEstado } : anuncio,
        ),
      );

      Swal.fire({
        title: "Actualizado",
        text: "El estado del anuncio se actualizó correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
    } catch (error) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: "No se pudo cambiar el estado del anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  const handleEditar = (id: number) => {
    console.log("Editar anuncio:", id);
  };

  return (
    <div className="mx-auto max-w-6xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Mis Anuncios</h1>

        {anuncios.length > 0 && (
          <Link
            to="/dashboard/publicar"
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            <FaPlusCircle />
            Publicar vehículo
          </Link>
        )}
      </div>

      {anuncios.length === 0 ? (
        <div className="flex min-h-[420px] flex-col items-center justify-center rounded-2xl border border-gray-200 bg-white px-6 text-center shadow-sm">
          <div className="mb-5 flex h-20 w-20 items-center justify-center rounded-full bg-blue-50">
            <FaCar className="text-4xl text-blue-600" />
          </div>

          <h2 className="mb-2 text-2xl font-bold text-gray-800">
            Todavía no tienes anuncios
          </h2>

          <p className="mb-6 max-w-md text-gray-500">
            Aún no has publicado ningún vehículo. Comienza agregando tu primer
            anuncio para mostrarlo en AutoMarket RD.
          </p>

          <Link
            to="/dashboard/publicar"
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            <FaPlusCircle />
            Publicar vehículo
          </Link>
        </div>
      ) : (
        <ul className="space-y-4">
          {anuncios.map((anuncio) => (
            <AnuncioCard
              key={anuncio.id}
              anuncio={anuncio}
              onPublicar={handlePublicar}
              onCambiarEstado={handleCambiarEstado}
              onEditar={handleEditar}
            />
          ))}
        </ul>
      )}
    </div>
  );
}
