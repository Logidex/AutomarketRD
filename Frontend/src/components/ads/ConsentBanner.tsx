import { Link } from "react-router-dom";
import { useConsent } from "../../context/ConsentContext";

/** Banner de consentimiento de cookies/publicidad. Se muestra hasta que el
 *  usuario acepta o rechaza. Cumple la política de consentimiento de Google:
 *  sin aceptación no se cargan los anuncios ni el script de AdSense. */
export default function ConsentBanner() {
  const { estado, aceptar, rechazar } = useConsent();

  if (estado !== "pendiente") return null;

  return (
    <div className="fixed inset-x-0 bottom-0 z-50 border-t border-line bg-surface/95 p-4 shadow-2xl backdrop-blur-xl sm:p-5">
      <div className="mx-auto flex max-w-6xl flex-col items-start gap-4 sm:flex-row sm:items-center sm:justify-between">
        <p className="max-w-3xl text-sm leading-6 text-ink-2">
          Usamos cookies esenciales para que el sitio funcione y, si aceptas,
          cookies de publicidad para mostrar anuncios de Google (AdSense) que
          nos ayudan a mantener la plataforma. Puedes leer más en nuestra{" "}
          <Link
            to="/privacidad"
            className="font-semibold text-brand underline transition-colors hover:text-brand-hover"
          >
            Política de Privacidad
          </Link>
          .
        </p>

        <div className="flex shrink-0 items-center gap-3">
          <button
            type="button"
            onClick={rechazar}
            className="rounded-xl border border-line bg-page px-5 py-2.5 text-sm font-semibold text-ink-2 transition-colors hover:bg-hover hover:text-ink"
          >
            Rechazar
          </button>
          <button
            type="button"
            onClick={aceptar}
            className="rounded-xl bg-brand px-5 py-2.5 text-sm font-bold text-white transition-colors hover:bg-brand-hover"
          >
            Aceptar
          </button>
        </div>
      </div>
    </div>
  );
}