import Swal from "sweetalert2";
import { FaUndo } from "react-icons/fa";
import type { PagoAdmin } from "../../services/admin.service";
import { useAdminPagos, useReembolsarPago } from "../../hooks/useAdmin";
import Spinner from "../../components/Spinner";
import { formatearPrecio } from "../../utils/formato";
import { formatearFecha } from "../../utils/fecha";

function estadoPagoLabel(estado: string): string {
  const mapa: Record<string, string> = {
    Completado: "Completado",
    Fallido: "Fallido",
    Reembolsado: "Reembolsado",
  };
  return mapa[estado] ?? estado;
}

function estadoPagoClass(estado: string): string {
  if (estado === "Completado") {
    return "rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-semibold text-green-700";
  }
  if (estado === "Fallido") {
    return "rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-700";
  }
  return "rounded-full bg-surface-2 px-2.5 py-0.5 text-xs font-semibold text-ink-2";
}

export default function AdminPagos() {
  const { data: pagos = [], isLoading: loading } = useAdminPagos();
  const reembolsarPago = useReembolsarPago();

  const reembolsar = async (pago: PagoAdmin) => {
    const resultado = await Swal.fire({
      icon: "warning",
      title: "Reembolsar pago",
      text: `¿Reembolsar ${pago.dealerNombreAgencia} por ${formatearPrecio(
        pago.monto,
        pago.moneda,
      )}? La operación se hará contra PayPal.`,
      showCancelButton: true,
      confirmButtonColor: "#dc2626",
      confirmButtonText: "Sí, reembolsar",
      cancelButtonText: "Cancelar",
    });

    if (!resultado.isConfirmed) return;

    try {
      const respuesta = await reembolsarPago.mutateAsync(pago.id);
      await Swal.fire({
        icon: "success",
        title: "Reembolsado",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "No se pudo reembolsar",
        text: error.message || "Ocurrió un error al intentar el reembolso.",
        confirmButtonColor: "#7c3aed",
      });
    }
  };

  if (loading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6 p-6">
      <div>
        <h2 className="text-2xl font-bold text-ink">Pagos y reembolsos</h2>
        <p className="mt-1 text-sm text-ink-3">
          Historial de pagos de suscripción de los dealers. Puedes reembolsar un
          pago completado contra PayPal.
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
              <th className="px-4 py-3">Orden PayPal</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-line">
            {pagos.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-ink-3">
                  No hay pagos registrados todavía.
                </td>
              </tr>
            ) : (
              pagos.map((pago) => (
                <tr key={pago.id} className="hover:bg-surface-2">
                  <td className="px-4 py-3">
                    <p className="font-semibold text-ink">
                      {pago.dealerNombreAgencia}
                    </p>
                    <p className="text-xs text-ink-3">{pago.dealerEmail}</p>
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    <span className="mr-2 inline-block rounded-full bg-violet-100 px-2.5 py-0.5 text-xs font-semibold text-violet-700">
                      {pago.nivel}
                    </span>
                    <span className="text-xs text-ink-3">{pago.ciclo}</span>
                  </td>
                  <td className="px-4 py-3 font-semibold text-ink">
                    {formatearPrecio(pago.monto, pago.moneda)}
                  </td>
                  <td className="px-4 py-3">
                    <span className={estadoPagoClass(pago.estado)}>
                      {estadoPagoLabel(pago.estado)}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {formatearFecha(pago.fechaUtc, true)}
                  </td>
                  <td className="px-4 py-3 text-xs text-ink-3">
                    {pago.ordenIdPayPal ?? "—"}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end">
                      {pago.estado !== "Reembolsado" && (
                        <button
                          type="button"
                          onClick={() => reembolsar(pago)}
                          className="inline-flex items-center gap-1 rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 transition-colors hover:bg-red-50"
                        >
                          <FaUndo />
                          Reembolsar
                        </button>
                      )}
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