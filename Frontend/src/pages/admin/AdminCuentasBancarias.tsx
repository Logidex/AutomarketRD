import { useState } from "react";
import { FaPlus, FaEdit, FaToggleOn, FaToggleOff } from "react-icons/fa";
import Swal from "sweetalert2";
import {
  useAdminCuentasBancarias,
  useCrearCuentaBancaria,
  useActualizarCuentaBancaria,
  useToggleCuentaBancaria,
} from "../../hooks/useCuentasBancarias";
import type { CuentaBancaria } from "../../services/cuentasBancarias.service";
import SpinnerComponent from "../../components/Spinner";

const BANCOS: Record<number, string> = { 1: "Popular", 2: "QIK" };

const estadoInicial = {
  banco: 1,
  nombreTitular: "",
  numeroCuenta: "",
  tipoCuenta: "Ahorro",
  documento: "",
  conceptoReferencia: "Pago Suscripción AutoMarket",
};

export default function AdminCuentasBancarias() {
  const { data: cuentas = [], isLoading } = useAdminCuentasBancarias();
  const crear = useCrearCuentaBancaria();
  const actualizar = useActualizarCuentaBancaria();
  const toggle = useToggleCuentaBancaria();

  const [formulario, setFormulario] = useState(estadoInicial);
  const [editando, setEditando] = useState<number | null>(null);
  const [mostrarForm, setMostrarForm] = useState(false);

  const abrirCrear = () => {
    setFormulario(estadoInicial);
    setEditando(null);
    setMostrarForm(true);
  };

  const abrirEditar = (c: CuentaBancaria) => {
    setFormulario({
      banco: c.banco,
      nombreTitular: c.nombreTitular,
      numeroCuenta: c.numeroCuenta,
      tipoCuenta: c.tipoCuenta,
      documento: c.documento,
      conceptoReferencia: c.conceptoReferencia,
    });
    setEditando(c.id);
    setMostrarForm(true);
  };

  const handleSubmit = async () => {
    if (!formulario.nombreTitular || !formulario.numeroCuenta || !formulario.documento) {
      await Swal.fire("Campos requeridos", "Nombre, número de cuenta y documento son obligatorios.", "warning");
      return;
    }

    try {
      if (editando) {
        await actualizar.mutateAsync({ id: editando, cuenta: formulario });
        await Swal.fire("Actualizada", "Cuenta bancaria actualizada.", "success");
      } else {
        await crear.mutateAsync(formulario);
        await Swal.fire("Creada", "Cuenta bancaria registrada.", "success");
      }
      setMostrarForm(false);
    } catch {
      await Swal.fire("Error", "No se pudo guardar la cuenta.", "error");
    }
  };

  const handleToggle = async (c: CuentaBancaria) => {
    const accion = c.activa ? "desactivar" : "activar";
    const result = await Swal.fire({
      title: `${accion.charAt(0).toUpperCase() + accion.slice(1)} cuenta`,
      text: `¿Deseas ${accion} la cuenta de ${BANCOS[c.banco]}?`,
      icon: "question",
      showCancelButton: true,
      confirmButtonColor: "#7c3aed",
      cancelButtonColor: "#6b7280",
    });
    if (result.isConfirmed) {
      await toggle.mutateAsync(c.id);
    }
  };

  if (isLoading) {
    return <SpinnerComponent />;
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-ink">Cuentas Bancarias</h2>
          <p className="mt-1 text-sm text-ink-3">
            Gestiona las cuentas destino para transferencias de pago.
          </p>
        </div>
        <button
          onClick={abrirCrear}
          className="inline-flex items-center gap-2 rounded-lg border border-green-200 bg-surface px-4 py-2 text-sm font-semibold text-green-700 transition-colors hover:bg-green-50"
        >
          <FaPlus /> Agregar
        </button>
      </div>

      {mostrarForm && (
        <div className="rounded-lg border border-line bg-surface p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-bold text-ink">
            {editando ? "Editar Cuenta" : "Nueva Cuenta"}
          </h3>
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-3">Banco</label>
              <select
                value={formulario.banco}
                onChange={(e) => setFormulario({ ...formulario, banco: Number(e.target.value) })}
                className="w-full rounded-lg border border-line bg-surface-2 px-3 py-2 text-ink focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
              >
                <option value={1}>Popular</option>
                <option value={2}>QIK</option>
              </select>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-3">Tipo de Cuenta</label>
              <select
                value={formulario.tipoCuenta}
                onChange={(e) => setFormulario({ ...formulario, tipoCuenta: e.target.value })}
                className="w-full rounded-lg border border-line bg-surface-2 px-3 py-2 text-ink focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
              >
                <option value="Ahorro">Ahorro</option>
                <option value="Corriente">Corriente</option>
              </select>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-3">Nombre del Titular</label>
              <input
                type="text"
                value={formulario.nombreTitular}
                onChange={(e) => setFormulario({ ...formulario, nombreTitular: e.target.value })}
                className="w-full rounded-lg border border-line bg-surface-2 px-3 py-2 text-ink focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
                placeholder="Nombre completo"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-3">Número de Cuenta</label>
              <input
                type="text"
                value={formulario.numeroCuenta}
                onChange={(e) => setFormulario({ ...formulario, numeroCuenta: e.target.value })}
                className="w-full rounded-lg border border-line bg-surface-2 px-3 py-2 text-ink focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
                placeholder="000-000000-00"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-3">Documento (Cédula/RNC)</label>
              <input
                type="text"
                value={formulario.documento}
                onChange={(e) => setFormulario({ ...formulario, documento: e.target.value })}
                className="w-full rounded-lg border border-line bg-surface-2 px-3 py-2 text-ink focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
                placeholder="001-0000000-0"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-3">Concepto / Referencia</label>
              <input
                type="text"
                value={formulario.conceptoReferencia}
                onChange={(e) => setFormulario({ ...formulario, conceptoReferencia: e.target.value })}
                className="w-full rounded-lg border border-line bg-surface-2 px-3 py-2 text-ink focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
                placeholder="Pago Suscripción AutoMarket"
              />
            </div>
          </div>
          <div className="mt-4 flex gap-3">
            <button
              onClick={handleSubmit}
              disabled={crear.isPending || actualizar.isPending}
              className="inline-flex items-center gap-1 rounded-lg border border-green-200 bg-surface px-4 py-2 text-sm font-semibold text-green-700 transition-colors hover:bg-green-50 disabled:opacity-50"
            >
              {editando ? "Actualizar" : "Crear"}
            </button>
            <button
              onClick={() => setMostrarForm(false)}
              className="inline-flex items-center gap-1 rounded-lg border border-line bg-surface px-4 py-2 text-sm font-semibold text-ink-2 transition-colors hover:bg-surface-2"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}

      <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
            <tr>
              <th className="px-4 py-3">Banco</th>
              <th className="px-4 py-3">Tipo</th>
              <th className="px-4 py-3">Titular</th>
              <th className="px-4 py-3">Cuenta</th>
              <th className="px-4 py-3">Documento</th>
              <th className="px-4 py-3">Referencia</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-line">
            {cuentas.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-4 py-8 text-center text-ink-3">
                  No hay cuentas bancarias registradas.
                </td>
              </tr>
            ) : (
              cuentas.map((c) => (
                <tr key={c.id} className="hover:bg-surface-2">
                  <td className="px-4 py-3">
                    <span className="inline-block rounded-full bg-violet-100 px-2.5 py-0.5 text-xs font-semibold text-violet-700">
                      {BANCOS[c.banco]}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-ink-2">{c.tipoCuenta}</td>
                  <td className="px-4 py-3">
                    <p className="font-semibold text-ink">{c.nombreTitular}</p>
                  </td>
                  <td className="px-4 py-3 font-mono text-ink-2">{c.numeroCuenta}</td>
                  <td className="px-4 py-3 text-ink-2">{c.documento}</td>
                  <td className="px-4 py-3 text-ink-2">{c.conceptoReferencia}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                        c.activa
                          ? "bg-green-100 text-green-700"
                          : "bg-red-100 text-red-700"
                      }`}
                    >
                      {c.activa ? "Activa" : "Inactiva"}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        type="button"
                        onClick={() => abrirEditar(c)}
                        className="inline-flex items-center gap-1 rounded-lg border border-blue-200 bg-surface px-3 py-1.5 text-xs font-semibold text-blue-700 transition-colors hover:bg-blue-50"
                        title="Editar"
                      >
                        <FaEdit /> Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => handleToggle(c)}
                        className={`inline-flex items-center gap-1 rounded-lg border px-3 py-1.5 text-xs font-semibold transition-colors ${
                          c.activa
                            ? "border-yellow-200 bg-surface text-yellow-700 hover:bg-yellow-50"
                            : "border-green-200 bg-surface text-green-700 hover:bg-green-50"
                        }`}
                        title={c.activa ? "Desactivar" : "Activar"}
                      >
                        {c.activa ? <FaToggleOn /> : <FaToggleOff />}
                        {c.activa ? "Desactivar" : "Activar"}
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
