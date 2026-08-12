import { useState } from "react";
import Swal from "sweetalert2";
import { FaPlus, FaTrash, FaSave, FaTimes } from "react-icons/fa";
import type { PlanAdmin, PlanAdminForm } from "../../services/admin.service";
import Spinner from "../../components/Spinner";
import { formatearRD$ } from "../../utils/formato";
import {
  useAdminPlanes,
  useCrearPlan,
  useActualizarPlan,
  useEliminarPlan,
} from "../../hooks/useAdmin";

const NIVELES = ["Gratis", "Basico", "Pro", "Elite"];

const PLAN_VACIO: PlanAdminForm = {
  nivel: "Basico",
  nombre: "",
  descripcion: "",
  limiteAnuncios: 10,
  precioMensual: 0,
  descuentoTrimestralPorcentaje: 0,
  descuentoAnualPorcentaje: 0,
  activo: true,
};

const inputClase =
  "w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition-colors focus:border-violet-500 focus:ring-2 focus:ring-violet-200";
const labelClase = "mb-1 block text-sm font-medium text-gray-700";

export default function AdminPlanes() {
  const { data: planes = [], isLoading: loading } = useAdminPlanes();
  const [guardando, setGuardando] = useState(false);

  const [modalAbierto, setModalAbierto] = useState(false);
  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [form, setForm] = useState<PlanAdminForm>(PLAN_VACIO);

  const crearPlan = useCrearPlan();
  const actualizarPlan = useActualizarPlan();
  const eliminarPlan = useEliminarPlan();

  const abrirNuevo = () => {
    setEditandoId(null);
    setForm(PLAN_VACIO);
    setModalAbierto(true);
  };

  const abrirEditar = (plan: PlanAdmin) => {
    setEditandoId(plan.id);
    setForm({
      nivel: plan.nivel,
      nombre: plan.nombre,
      descripcion: plan.descripcion ?? "",
      limiteAnuncios: plan.limiteAnuncios,
      precioMensual: plan.precioMensual,
      descuentoTrimestralPorcentaje: plan.descuentoTrimestralPorcentaje,
      descuentoAnualPorcentaje: plan.descuentoAnualPorcentaje,
      activo: plan.activo,
    });
    setModalAbierto(true);
  };

  const guardar = async () => {
    if (!form.nombre.trim()) {
      await Swal.fire({
        icon: "warning",
        title: "Falta el nombre",
        text: "Escribe el nombre del plan.",
        confirmButtonColor: "#7c3aed",
      });
      return;
    }

    setGuardando(true);
    try {
      const respuesta =
        editandoId === null
          ? await crearPlan.mutateAsync(form)
          : await actualizarPlan.mutateAsync({ id: editandoId, datos: form });

      await Swal.fire({
        icon: "success",
        title: "Guardado",
        text: `Plan "${respuesta.nombre}" guardado correctamente.`,
        confirmButtonColor: "#7c3aed",
      });
      setModalAbierto(false);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: error.message || "No se pudo guardar el plan.",
        confirmButtonColor: "#7c3aed",
      });
    } finally {
      setGuardando(false);
    }
  };

  const eliminar = async (plan: PlanAdmin) => {
    const resultado = await Swal.fire({
      icon: "warning",
      title: "Eliminar plan",
      text: `¿Desactivar el plan "${plan.nombre}"? Los dealers que lo usen quedarán sin plan.`,
      showCancelButton: true,
      confirmButtonColor: "#dc2626",
      confirmButtonText: "Sí, eliminar",
      cancelButtonText: "Cancelar",
    });

    if (!resultado.isConfirmed) return;

    try {
      const respuesta = await eliminarPlan.mutateAsync(plan.id);

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
        text: error.message || "No se pudo eliminar el plan.",
        confirmButtonColor: "#7c3aed",
      });
    }
  };

  if (loading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900">Catálogo de planes</h2>

        <button
          type="button"
          onClick={abrirNuevo}
          className="inline-flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-violet-700"
        >
          <FaPlus />
          Nuevo plan
        </button>
      </div>

      <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white shadow-sm">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">Plan</th>
              <th className="px-4 py-3">Nivel</th>
              <th className="px-4 py-3">Límite anuncios</th>
              <th className="px-4 py-3">Mensual</th>
              <th className="px-4 py-3">Trimestral</th>
              <th className="px-4 py-3">Anual</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {planes.map((plan) => (
              <tr key={plan.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <p className="font-semibold text-gray-900">{plan.nombre}</p>
                  {plan.descripcion && (
                    <p className="text-xs text-gray-500">{plan.descripcion}</p>
                  )}
                </td>
                <td className="px-4 py-3">
                  <span className="inline-block rounded-full bg-violet-100 px-2.5 py-0.5 text-xs font-semibold text-violet-700">
                    {plan.nivel}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-700">
                  {plan.limiteAnuncios}
                </td>
                <td className="px-4 py-3 font-medium text-gray-900">
                  {formatearRD$(plan.precioMensual)}
                </td>
                <td className="px-4 py-3 text-gray-700">
                  {formatearRD$(plan.precioTrimestral)}
                </td>
                <td className="px-4 py-3 text-gray-700">
                  {formatearRD$(plan.precioAnual)}
                </td>
                <td className="px-4 py-3">
                  {plan.activo ? (
                    <span className="inline-block rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-semibold text-green-700">
                      Activo
                    </span>
                  ) : (
                    <span className="inline-block rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-semibold text-gray-600">
                      Inactivo
                    </span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center justify-end gap-2">
                    <button
                      type="button"
                      onClick={() => abrirEditar(plan)}
                      className="rounded-lg border border-gray-200 bg-white px-3 py-1.5 text-xs font-semibold text-gray-700 transition-colors hover:bg-gray-50"
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      onClick={() => eliminar(plan)}
                      className="inline-flex items-center gap-1 rounded-lg border border-red-200 bg-white px-3 py-1.5 text-xs font-semibold text-red-700 transition-colors hover:bg-red-50"
                    >
                      <FaTrash />
                      Eliminar
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* MODAL */}
      {modalAbierto && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
          onClick={() => !guardando && setModalAbierto(false)}
        >
          <div
            className="w-full max-w-lg rounded-xl bg-white p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-lg font-bold text-gray-900">
                {editandoId === null ? "Nuevo plan" : "Editar plan"}
              </h3>
              <button
                type="button"
                onClick={() => setModalAbierto(false)}
                disabled={guardando}
                className="rounded-lg p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 disabled:opacity-50"
              >
                <FaTimes />
              </button>
            </div>

            <div className="space-y-4">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label className={labelClase}>Nivel</label>
                  <select
                    value={form.nivel}
                    onChange={(e) => setForm({ ...form, nivel: e.target.value })}
                    className={inputClase}
                  >
                    {NIVELES.map((nivel) => (
                      <option key={nivel} value={nivel}>
                        {nivel}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className={labelClase}>Nombre *</label>
                  <input
                    type="text"
                    value={form.nombre}
                    onChange={(e) => setForm({ ...form, nombre: e.target.value })}
                    className={inputClase}
                    placeholder="Ej: Básico"
                  />
                </div>
              </div>

              <div>
                <label className={labelClase}>Descripción</label>
                <textarea
                  value={form.descripcion ?? ""}
                  onChange={(e) =>
                    setForm({ ...form, descripcion: e.target.value })
                  }
                  rows={2}
                  className={inputClase}
                  placeholder="Beneficios y alcance del plan"
                />
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label className={labelClase}>Límite de anuncios</label>
                  <input
                    type="number"
                    min={0}
                    value={form.limiteAnuncios}
                    onChange={(e) =>
                      setForm({ ...form, limiteAnuncios: Number(e.target.value) })
                    }
                    className={inputClase}
                  />
                </div>

                <div>
                  <label className={labelClase}>Precio mensual (RD$)</label>
                  <input
                    type="number"
                    min={0}
                    step="0.01"
                    value={form.precioMensual}
                    onChange={(e) =>
                      setForm({ ...form, precioMensual: Number(e.target.value) })
                    }
                    className={inputClase}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label className={labelClase}>
                    Descuento trimestral (%)
                  </label>
                  <input
                    type="number"
                    min={0}
                    max={100}
                    value={form.descuentoTrimestralPorcentaje}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        descuentoTrimestralPorcentaje: Number(e.target.value),
                      })
                    }
                    className={inputClase}
                  />
                </div>

                <div>
                  <label className={labelClase}>Descuento anual (%)</label>
                  <input
                    type="number"
                    min={0}
                    max={100}
                    value={form.descuentoAnualPorcentaje}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        descuentoAnualPorcentaje: Number(e.target.value),
                      })
                    }
                    className={inputClase}
                  />
                </div>
              </div>

              <label className="flex cursor-pointer items-center gap-2 text-sm font-medium text-gray-700">
                <input
                  type="checkbox"
                  checked={form.activo}
                  onChange={(e) => setForm({ ...form, activo: e.target.checked })}
                  className="h-4 w-4 rounded border-gray-300 text-violet-600 focus:ring-violet-500"
                />
                Plan activo (visible en el catálogo público)
              </label>
            </div>

            <div className="mt-6 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setModalAbierto(false)}
                disabled={guardando}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50 disabled:opacity-50"
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={guardar}
                disabled={guardando}
                className="inline-flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-violet-700 disabled:opacity-50"
              >
                <FaSave />
                {guardando ? "Guardando…" : "Guardar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}