import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import AnuncioCard from "../components/AnuncioCard";
import { anuncioService } from "../services/anuncio.service";
import type { AnuncioListado } from "../types/anuncio.types";
import { getUserIdFromToken } from "../utils/jwt.util";

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
      <h1 className="mb-6 text-2xl font-bold text-gray-900">Mis Anuncios</h1>

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
    </div>
  );
}
