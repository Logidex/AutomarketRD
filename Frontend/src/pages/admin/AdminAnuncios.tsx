import { useState } from "react";
import Swal from "sweetalert2";
import { FaTrash } from "react-icons/fa";
import type { AnuncioAdmin } from "../../services/admin.service";
import Spinner from "../../components/Spinner";
import { formatearPrecio } from "../../utils/formato";
import { useAdminAnuncios, useEliminarAnuncioAdmin } from "../../hooks/useAdmin";

export default function AdminAnuncios() {
  const { data: anuncios = [], isLoading: loading } = useAdminAnuncios();
  const [procesando, setProcesando] = useState<number | null>(null);
  const eliminarAnuncio = useEliminarAnuncioAdmin();

  const eliminar = async (anuncio: AnuncioAdmin) => {
    if (procesando !== null) return;

    const resultado = await Swal.fire({
      icon: "warning",
      title: "Eliminar anuncio definitivamente",
      html: `¿Eliminar <strong>${anuncio.marca} ${anuncio.modelo}</strong>?<br/>Esta acción no se puede deshacer.`,
      showCancelButton: true,
      confirmButtonColor: "#dc2626",
      confirmButtonText: "Sí, eliminar",
      cancelButtonText: "Cancelar",
    });

    if (!resultado.isConfirmed) return;

    setProcesando(anuncio.id);
    try {
      const respuesta = await eliminarAnuncio.mutateAsync(anuncio.id);

      await Swal.fire({
        icon: "success",
        title: "Eliminado",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: error.message || "No se pudo eliminar el anuncio.",
        confirmButtonColor: "#7c3aed",
      });
    } finally {
      setProcesando(null);
    }
  };

  if (loading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-ink">
          Moderación de anuncios
        </h2>
      </div>

      {anuncios.length === 0 ? (
        <div className="rounded-lg border border-line bg-surface p-10 text-center text-ink-3 shadow-sm">
          No hay anuncios registrados.
        </div>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
              <tr>
                <th className="px-4 py-3">ID</th>
                <th className="px-4 py-3">Marca</th>
                <th className="px-4 py-3">Modelo</th>
                <th className="px-4 py-3">Precio</th>
                <th className="px-4 py-3">ID del dueño</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-line">
              {anuncios.map((anuncio) => (
                <tr key={anuncio.id} className="hover:bg-surface-2">
                  <td className="px-4 py-3 text-ink-2">{anuncio.id}</td>
                  <td className="px-4 py-3 font-medium text-ink">
                    {anuncio.marca}
                  </td>
                  <td className="px-4 py-3 text-ink-2">{anuncio.modelo}</td>
                  <td className="px-4 py-3 font-semibold text-ink">
                    {formatearPrecio(anuncio.precio, anuncio.moneda)}
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {anuncio.usuarioId}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      type="button"
                      onClick={() => eliminar(anuncio)}
                      disabled={procesando !== null}
                      className="inline-flex items-center gap-1.5 rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 transition-colors hover:bg-red-50 disabled:opacity-50"
                    >
                      <FaTrash />
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}