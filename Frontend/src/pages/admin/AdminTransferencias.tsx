import { useState } from "react";
import Swal from "sweetalert2";
import { FaCheck, FaTimes, FaImage, FaSpinner } from "react-icons/fa";
import type { PagoAdmin } from "../../services/admin.service";
import { useAdminTransferencias, useAprobarTransferencia, useRechazarTransferencia } from "../../hooks/useAdmin";
import SpinnerComponent from "../../components/Spinner";
import { formatearPrecio } from "../../utils/formato";
import { formatearFecha } from "../../utils/fecha";
import { nombrePlan } from "../../constants/planes";
import { urlImagen } from "../../utils/imagen";

function estadoTransferenciaLabel(estado: string | null): string {
  const mapa: Record<string, string> = {
    Pendiente: "Pendiente",
    Aprobada: "Aprobada",
    Rechazada: "Rechazada",
  };
  return mapa[estado ?? ""] ?? estado ?? "—";
}

function estadoTransferenciaClass(estado: string | null): string {
  if (estado === "Pendiente")
    return "rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-semibold text-yellow-700";
  if (estado === "Aprobada")
    return "rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-semibold text-green-700";
  if (estado === "Rechazada")
    return "rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-700";
  return "rounded-full bg-surface-2 px-2.5 py-0.5 text-xs font-semibold text-ink-2";
}

export default function AdminTransferencias() {
  const { data: transferencias = [], isLoading: loading } = useAdminTransferencias();
  const aprobarTransferencia = useAprobarTransferencia();
  const rechazarTransferencia = useRechazarTransferencia();
  const [imagenModal, setImagenModal] = useState<string | null>(null);

  const handleAprobar = async (pago: PagoAdmin) => {
    const resultado = await Swal.fire({
      icon: "question",
      title: "Aprobar transferencia",
      html: `¿Aprobar la transferencia de <strong>${pago.dealerNombreAgencia}</strong> por <strong>${formatearPrecio(pago.monto, pago.moneda)}</strong>?<br/><br/>La suscripción será extendida al plan completo.`,
      input: "text",
      inputPlaceholder: "Notas (opcional)",
      showCancelButton: true,
      confirmButtonColor: "#16a34a",
      confirmButtonText: "Sí, aprobar",
      cancelButtonText: "Cancelar",
    });

    if (!resultado.isConfirmed) return;

    try {
      const respuesta = await aprobarTransferencia.mutateAsync({
        id: pago.id,
        notas: resultado.value || undefined,
      });
      await Swal.fire({
        icon: "success",
        title: "Aprobada",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
    } catch (error: unknown) {
      const mensaje = error instanceof Error ? error.message : "Ocurrió un error.";
      await Swal.fire({
        icon: "error",
        title: "No se pudo aprobar",
        text: mensaje,
        confirmButtonColor: "#7c3aed",
      });
    }
  };

  const handleRechazar = async (pago: PagoAdmin) => {
    const resultado = await Swal.fire({
      icon: "warning",
      title: "Rechazar transferencia",
      html: `¿Rechazar la transferencia de <strong>${pago.dealerNombreAgencia}</strong>?<br/><br/>La suscripción del dealer será revocada.`,
      input: "text",
      inputPlaceholder: "Motivo del rechazo (opcional)",
      showCancelButton: true,
      confirmButtonColor: "#dc2626",
      confirmButtonText: "Sí, rechazar",
      cancelButtonText: "Cancelar",
    });

    if (!resultado.isConfirmed) return;

    try {
      const respuesta = await rechazarTransferencia.mutateAsync({
        id: pago.id,
        notas: resultado.value || undefined,
      });
      await Swal.fire({
        icon: "success",
        title: "Rechazada",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
    } catch (error: unknown) {
      const mensaje = error instanceof Error ? error.message : "Ocurrió un error.";
      await Swal.fire({
        icon: "error",
        title: "No se pudo rechazar",
        text: mensaje,
        confirmButtonColor: "#7c3aed",
      });
    }
  };

  if (loading) {
    return <SpinnerComponent />;
  }

  return (
    <div className="space-y-6 p-6">
      <div>
        <h2 className="text-2xl font-bold text-ink">Transferencias bancarias</h2>
        <p className="mt-1 text-sm text-ink-3">
          Transferencias pendientes de confirmación. Revisa la captura del comprobante y aprueba o rechaza cada pago.
        </p>
      </div>

      <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
            <tr>
              <th className="px-4 py-3">Dealer</th>
              <th className="px-4 py-3">Plan</th>
              <th className="px-4 py-3">Monto</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3">Fecha</th>
              <th className="px-4 py-3">Captura</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-line">
            {transferencias.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-ink-3">
                  No hay transferencias pendientes.
                </td>
              </tr>
            ) : (
              transferencias.map((pago) => (
                <tr key={pago.id} className="hover:bg-surface-2">
                  <td className="px-4 py-3">
                    <p className="font-semibold text-ink">
                      {pago.dealerNombreAgencia}
                    </p>
                    <p className="text-xs text-ink-3">{pago.dealerEmail}</p>
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    <span className="mr-2 inline-block rounded-full bg-violet-100 px-2.5 py-0.5 text-xs font-semibold text-violet-700">
                      {nombrePlan(pago.nivel)}
                    </span>
                    <span className="text-xs text-ink-3">{pago.ciclo}</span>
                  </td>
                  <td className="px-4 py-3 font-semibold text-ink">
                    {formatearPrecio(pago.monto, pago.moneda)}
                  </td>
                  <td className="px-4 py-3">
                    <span className={estadoTransferenciaClass(pago.estadoTransferencia ?? null)}>
                      {estadoTransferenciaLabel(pago.estadoTransferencia ?? null)}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {formatearFecha(pago.fechaUtc, true)}
                  </td>
                  <td className="px-4 py-3">
                    {pago.urlCapturaTransferencia ? (
                      <button
                        type="button"
                        onClick={() => setImagenModal(urlImagen(pago.urlCapturaTransferencia!))}
                        className="inline-flex items-center gap-1 text-sm text-blue-500 hover:text-blue-700 transition-colors"
                      >
                        <FaImage />
                        Ver captura
                      </button>
                    ) : (
                      <span className="text-xs text-ink-3">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      {pago.estadoTransferencia === "Pendiente" && (
                        <>
                          <button
                            type="button"
                            onClick={() => handleAprobar(pago)}
                            disabled={aprobarTransferencia.isPending}
                            className="inline-flex items-center gap-1 rounded-lg border border-green-200 bg-surface px-3 py-1.5 text-xs font-semibold text-green-700 transition-colors hover:bg-green-50 disabled:opacity-50"
                          >
                            {aprobarTransferencia.isPending ? (
                              <FaSpinner className="animate-spin" />
                            ) : (
                              <FaCheck />
                            )}
                            Aprobar
                          </button>
                          <button
                            type="button"
                            onClick={() => handleRechazar(pago)}
                            disabled={rechazarTransferencia.isPending}
                            className="inline-flex items-center gap-1 rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 transition-colors hover:bg-red-50 disabled:opacity-50"
                          >
                            {rechazarTransferencia.isPending ? (
                              <FaSpinner className="animate-spin" />
                            ) : (
                              <FaTimes />
                            )}
                            Rechazar
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal de imagen */}
      {imagenModal && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
          onClick={() => setImagenModal(null)}
        >
          <div
            className="relative max-h-[90vh] max-w-[90vw]"
            onClick={(e) => e.stopPropagation()}
          >
            <button
              type="button"
              onClick={() => setImagenModal(null)}
              className="absolute -top-3 -right-3 rounded-full bg-white p-2 shadow-lg hover:bg-gray-100"
            >
              <FaTimes className="text-gray-600" />
            </button>
            <img
              src={imagenModal}
              alt="Captura de transferencia"
              className="max-h-[85vh] max-w-[85vw] rounded-lg object-contain"
            />
          </div>
        </div>
      )}
    </div>
  );
}
