/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { anuncioService } from "../services/anuncio.service";
import type { AnuncioListado } from "../types/anuncio.types";
import AnuncioCard from "../components/AnuncioCard";
import { getUserIdFromToken } from "../utils/jwt.util";

export default function MisAnuncios() {
  const [anuncios, setAnuncios] = useState<AnuncioListado[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const cargarAnuncios = async () => {
    try {
      setCargando(true);
      setError(null);

      const usuarioLogueadoId = getUserIdFromToken();

      if (!usuarioLogueadoId) {
        setError("No se pudo obtener el usuario autenticado.");
        return;
      }

      const data = await anuncioService.obtenerMisAnuncios(usuarioLogueadoId);
      setAnuncios(data.items ?? []);
    } catch (err) {
      console.error(err);
      setError("Error al cargar los anuncios.");
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    cargarAnuncios();
  }, []);

  const handlePublicar = async (id: number) => {
    try {
      await anuncioService.publicarAnuncio(id);
      await cargarAnuncios();
    } catch (err) {
      console.error("Hubo un problema al publicar:", err);
      alert("Error al publicar el anuncio");
    }
  };

  if (cargando) {
    return <div className="p-8 text-center">Cargando tus anuncios...</div>;
  }

  if (error) {
    return <div className="p-8 text-center text-red-500">{error}</div>;
  }

  return (
    <div className="p-8 max-w-7xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Mi Dashboard de Inventario</h1>
        <Link
          to="/dashboard/publicar"
          className="bg-blue-600 text-white px-4 py-2 rounded shadow hover:bg-blue-700 font-medium transition-colors"
        >
          Nuevo Anuncio
        </Link>
      </div>

      <div className="bg-white shadow-sm border border-gray-200 overflow-hidden rounded-lg">
        <ul className="divide-y divide-gray-100">
          {anuncios.length > 0 ? (
            anuncios.map((anuncio) => (
              <AnuncioCard
                key={anuncio.id}
                anuncio={anuncio}
                onPublicar={handlePublicar}
              />
            ))
          ) : (
            <li className="p-12 text-center">
              <p className="text-gray-500 font-medium">
                No tienes anuncios creados todavía.
              </p>
              <p className="text-sm text-gray-400 mt-1">
                Haz clic en Nuevo Anuncio para empezar.
              </p>
            </li>
          )}
        </ul>
      </div>
    </div>
  );
}
