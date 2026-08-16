import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { FaCheckCircle, FaExclamationCircle, FaSpinner, FaTachometerAlt } from "react-icons/fa";
import logo from "../assets/AutoMarketRD_Logo.svg";
import { useConfirmarPago } from "../hooks/useSuscripcion";

type Estado = "procesando" | "exito" | "error";

export default function PagoExitoso() {
  const [params] = useSearchParams();

  const orderId = params.get("token");
  const confirmarPago = useConfirmarPago();
  const [estado, setEstado] = useState<Estado>(orderId ? "procesando" : "exito");
  const [mensajeError, setMensajeError] = useState<string>("");

  useEffect(() => {
    if (!orderId) return;

    let activo = true;

    confirmarPago
      .mutateAsync(orderId)
      .then(() => {
        if (activo) setEstado("exito");
      })
      .catch((err) => {
        if (activo) {
          setEstado("error");
          setMensajeError(
            err instanceof Error ? err.message : "No se pudo confirmar el pago. Inténtalo nuevamente."
          );
        }
      });

    return () => {
      activo = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [orderId]);

  return (
    <div className="min-h-screen bg-page flex items-center justify-center p-4 text-ink">
      <div className="w-full max-w-md bg-surface-2 rounded-2xl border border-line p-10 text-center">
        <div className="mb-6 flex justify-center">
          <img src={logo} alt="AutoMarket RD" className="h-14 w-auto object-contain" />
        </div>

        {estado === "procesando" && (
          <>
            <FaSpinner className="mx-auto mb-4 animate-spin text-5xl text-blue-400" />
            <h1 className="text-2xl font-bold mb-2">Confirmando tu pago...</h1>
            <p className="text-ink-2">
              Estamos verificando tu pago con PayPal y activando tu suscripción.
            </p>
          </>
        )}

        {estado === "exito" && (
          <>
            <FaCheckCircle className="mx-auto mb-4 text-5xl text-green-500" />
            <h1 className="text-2xl font-bold mb-2">¡Pago exitoso!</h1>
            <p className="text-ink-2 mb-8">
              Tu suscripción se activó correctamente. Ya puedes seguir publicando
              tus anuncios sin interrupciones.
            </p>
          </>
        )}

        {estado === "error" && (
          <>
            <FaExclamationCircle className="mx-auto mb-4 text-5xl text-red-500" />
            <h1 className="text-2xl font-bold mb-2">No pudimos confirmar el pago</h1>
            <p className="text-ink-2 mb-8">{mensajeError}</p>
            <p className="text-sm text-ink-2 mb-8">
              Puede que el pago aún se esté procesando. Revisa el estado de tu
              suscripción en tu panel de control.
            </p>
          </>
        )}

        <Link
          to="/dashboard"
          className="flex items-center justify-center gap-2 w-full rounded-lg bg-blue-500 py-3 font-semibold hover:bg-blue-600 transition-colors"
        >
          <FaTachometerAlt />
          Ir a mi Panel
        </Link>
      </div>
    </div>
  );
}