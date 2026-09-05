import { useSesionExpiracion } from '../hooks/useSesionExpiracion';

function formatearTiempo(segundos: number): string {
  const m = Math.floor(segundos / 60);
  const s = segundos % 60;
  if (m > 0) return `${m}m ${s}s`;
  return `${s}s`;
}

export default function SesionExpiracionToast() {
  const { mostrarToast, segundosRestantes, extenderSesion, cerrarToast } =
    useSesionExpiracion();

  if (!mostrarToast) return null;

  return (
    <div className="fixed bottom-4 right-4 z-[9999] max-w-sm animate-slide-up">
      <div className="rounded-xl border border-amber-300/40 bg-amber-50 p-4 shadow-xl dark:border-amber-500/30 dark:bg-amber-950/80">
        <div className="flex items-start gap-3">
          <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-amber-100 dark:bg-amber-900/50">
            <svg
              className="h-5 w-5 text-amber-600 dark:text-amber-400"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={2}
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 9v3.75m9-.75a9 9 0 1 1-18 0 9 9 0 0 1 18 0zm-9 3.75h.008v.008H12v-.008z"
              />
            </svg>
          </div>
          <div className="flex-1">
            <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">
              Tu sesión expira en {formatearTiempo(segundosRestantes)}
            </p>
            <p className="mt-1 text-xs text-amber-600 dark:text-amber-400">
              Se renovará automáticamente. También puedes extenderla ahora.
            </p>
          </div>
          <button
            onClick={cerrarToast}
            className="shrink-0 text-amber-400 hover:text-amber-600 dark:hover:text-amber-200"
            aria-label="Cerrar"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div className="mt-3 flex gap-2">
          <button
            onClick={extenderSesion}
            className="rounded-lg bg-amber-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-amber-700 dark:bg-amber-500 dark:hover:bg-amber-600"
          >
            Extender sesión
          </button>
          <button
            onClick={() => {
              cerrarToast();
            }}
            className="rounded-lg px-3 py-1.5 text-xs font-medium text-amber-700 transition hover:bg-amber-100 dark:text-amber-300 dark:hover:bg-amber-900/50"
          >
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
}
