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
  cuotaDestacados: 0,
  precioMensual: 0,
  descuentoTrimestralPorcentaje: 0,
  descuentoAnualPorcentaje: 0,
  activo: true,
};

const inputClase =
  "w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none transition-colors focus:border-violet-500 focus:ring-2 focus:ring-violet-200";
const labelClase = "mb-1 block text-sm font-medium text-ink-2";

function useAdminPlanesPage() {
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
      cuotaDestacados: plan.cuotaDestacados ?? 0,
      precioMensual: plan.precioMensual,
      descuentoTrimestralPorcentaje: plan.descuentoTrimestralPorcentaje,
      descuentoAnualPorcentaje: plan.descuentoAnualPorcentaje,
      activo: plan.activo,
    });
    setModalAbierto(true);
  };

  const guardar = async () => {
    if (crearPlan.isPending || actualizarPlan.isPending) return;

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

  const handleFormChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>,
  ) => {
    const { name, type } = e.target;
    let valor: string | number | boolean = e.target.value;
    if (type === "checkbox") {
      valor = (e.target as HTMLInputElement).checked;
    } else if (type === "number") {
      valor = Number.isFinite((e.target as HTMLInputElement).valueAsNumber)
        ? (e.target as HTMLInputElement).valueAsNumber
        : 0;
    }
    setForm((prev) => ({ ...prev, [name]: valor }));
  };

  const cerrarModal = () => {
    if (!guardando) setModalAbierto(false);
  };

  return {
    loading,
    planes,
    guardando,
    modalAbierto,
    editandoId,
    form,
    abrirNuevo,
    abrirEditar,
    guardar,
    eliminar,
    handleFormChange,
    cerrarModal,
  };
}

interface PropsTabla {
  planes: PlanAdmin[];
  onEditar: (plan: PlanAdmin) => void;
  onEliminar: (plan: PlanAdmin) => void;
}

function TablaPlanes({ planes, onEditar, onEliminar }: PropsTabla) {
  return (
    <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
          <tr>
            <th className="px-4 py-3">Plan</th>
            <th className="px-4 py-3">Nivel</th>
            <th className="px-4 py-3">Límite anuncios</th>
            <th className="px-4 py-3">Cuota destacados</th>
            <th className="px-4 py-3">Mensual</th>
            <th className="px-4 py-3">Trimestral</th>
            <th className="px-4 py-3">Anual</th>
            <th className="px-4 py-3">Estado</th>
            <th className="px-4 py-3 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-line">
          {planes.map((plan) => (
            <tr key={plan.id} className="hover:bg-surface-2">
              <td className="px-4 py-3">
                <p className="font-semibold text-ink">{plan.nombre}</p>
                {plan.descripcion && (
                  <p className="text-xs text-ink-3">{plan.descripcion}</p>
                )}
              </td>
              <td className="px-4 py-3">
                <span className="inline-block rounded-full bg-violet-100 px-2.5 py-0.5 text-xs font-semibold text-violet-700">
                  {plan.nivel}
                </span>
              </td>
              <td className="px-4 py-3 text-ink-2">
                {plan.limiteAnuncios}
              </td>
              <td className="px-4 py-3 text-ink-2 font-medium">
                {plan.cuotaDestacados ?? 0}
              </td>
              <td className="px-4 py-3 font-medium text-ink">
                {formatearRD$(plan.precioMensual)}
              </td>
              <td className="px-4 py-3 text-ink-2">
                {formatearRD$(plan.precioTrimestral)}
              </td>
              <td className="px-4 py-3 text-ink-2">
                {formatearRD$(plan.precioAnual)}
              </td>
              <td className="px-4 py-3">
                {plan.activo ? (
                  <span className="inline-block rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-semibold text-green-700">
                    Activo
                  </span>
                ) : (
                  <span className="inline-block rounded-full bg-surface-2 px-2.5 py-0.5 text-xs font-semibold text-ink-2">
                    Inactivo
                  </span>
                )}
              </td>
              <td className="px-4 py-3">
                <div className="flex items-center justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => onEditar(plan)}
                    className="rounded-lg border border-line bg-surface px-3 py-1.5 text-xs font-semibold text-ink-2 transition-colors hover:bg-surface-2"
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    onClick={() => onEliminar(plan)}
                    className="inline-flex items-center gap-1 rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 transition-colors hover:bg-red-50"
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
  );
}

interface PropsModal {
  editandoId: number | null;
  form: PlanAdminForm;
  guardando: boolean;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => void;
  onGuardar: () => void;
  onCerrar: () => void;
}

function ModalPlan({
  editandoId,
  form,
  guardando,
  onChange,
  onGuardar,
  onCerrar,
}: PropsModal) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <button
        type="button"
        aria-label="Cerrar ventana"
        onClick={onCerrar}
        className="absolute inset-0 bg-black/40"
      />
      <div
        className="relative w-full max-w-lg rounded-xl bg-surface p-6 shadow-2xl"
      >
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-lg font-bold text-ink">
            {editandoId === null ? "Nuevo plan" : "Editar plan"}
          </h3>
          <button
            type="button"
            onClick={onCerrar}
            disabled={guardando}
            aria-label="Cerrar ventana"
            className="rounded-lg p-1.5 text-ink-3 hover:bg-surface-2 hover:text-ink-2 disabled:opacity-50"
          >
            <FaTimes />
          </button>
        </div>

        <div className="space-y-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="planNivel" className={labelClase}>Nivel</label>
              <select
                id="planNivel"
                name="nivel"
                value={form.nivel}
                onChange={onChange}
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
              <label htmlFor="planNombre" className={labelClase}>Nombre *</label>
              <input
                id="planNombre"
                name="nombre"
                type="text"
                value={form.nombre}
                onChange={onChange}
                className={inputClase}
                placeholder="Ej: Básico"
              />
            </div>
          </div>

          <div>
            <label htmlFor="planDescripcion" className={labelClase}>Descripción</label>
            <textarea
              id="planDescripcion"
              name="descripcion"
              value={form.descripcion ?? ""}
              onChange={onChange}
              rows={2}
              className={inputClase}
              placeholder="Beneficios y alcance del plan"
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="planLimiteAnuncios" className={labelClase}>Límite de anuncios</label>
              <input
                id="planLimiteAnuncios"
                name="limiteAnuncios"
                type="number"
                min={0}
                value={form.limiteAnuncios}
                onChange={onChange}
                className={inputClase}
              />
            </div>

            <div>
              <label htmlFor="planCuotaDestacados" className={labelClase}>Cuota de destacados</label>
              <input
                id="planCuotaDestacados"
                name="cuotaDestacados"
                type="number"
                min={0}
                value={form.cuotaDestacados}
                onChange={onChange}
                className={inputClase}
              />
            </div>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="planPrecioMensual" className={labelClase}>Precio mensual (RD$)</label>
              <input
                id="planPrecioMensual"
                name="precioMensual"
                type="number"
                min={0}
                step="0.01"
                value={form.precioMensual}
                onChange={onChange}
                className={inputClase}
              />
            </div>

            <div>
              <label htmlFor="planDescuentoTrimestral" className={labelClase}>
                Descuento trimestral (%)
              </label>
              <input
                id="planDescuentoTrimestral"
                name="descuentoTrimestralPorcentaje"
                type="number"
                min={0}
                max={100}
                value={form.descuentoTrimestralPorcentaje}
                onChange={onChange}
                className={inputClase}
              />
            </div>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="planDescuentoAnual" className={labelClase}>Descuento anual (%)</label>
              <input
                id="planDescuentoAnual"
                name="descuentoAnualPorcentaje"
                type="number"
                min={0}
                max={100}
                value={form.descuentoAnualPorcentaje}
                onChange={onChange}
                className={inputClase}
              />
            </div>
          </div>

          <label className="flex cursor-pointer items-center gap-2 text-sm font-medium text-ink-2">
            <input
              name="activo"
              type="checkbox"
              checked={form.activo}
              onChange={onChange}
              className="h-4 w-4 rounded border-line text-violet-600 focus:ring-violet-500"
            />
            Plan activo (visible en el catálogo público)
          </label>
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <button
            type="button"
            onClick={onCerrar}
            disabled={guardando}
            className="rounded-lg border border-line px-4 py-2 text-sm font-semibold text-ink-2 transition-colors hover:bg-surface-2 disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={onGuardar}
            disabled={guardando}
            className="inline-flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-violet-700 disabled:opacity-50"
          >
            <FaSave />
            {guardando ? "Guardando…" : "Guardar"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default function AdminPlanes() {
  const {
    loading,
    planes,
    guardando,
    modalAbierto,
    editandoId,
    form,
    abrirNuevo,
    abrirEditar,
    guardar,
    eliminar,
    handleFormChange,
    cerrarModal,
  } = useAdminPlanesPage();

  if (loading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-ink">Catálogo de planes</h2>

        <button
          type="button"
          onClick={abrirNuevo}
          className="inline-flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-violet-700"
        >
          <FaPlus />
          Nuevo plan
        </button>
      </div>

      <TablaPlanes planes={planes} onEditar={abrirEditar} onEliminar={eliminar} />

      {/* MODAL */}
      {modalAbierto && (
        <ModalPlan
          editandoId={editandoId}
          form={form}
          guardando={guardando}
          onChange={handleFormChange}
          onGuardar={guardar}
          onCerrar={cerrarModal}
        />
      )}
    </div>
  );
}