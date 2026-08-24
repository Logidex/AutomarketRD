import { useState } from "react";
import { Link } from "react-router-dom";
import { FaStore, FaTimes } from "react-icons/fa";
import type { PerfilDealer } from "../services/perfilDealer.service";

/**
 * Nudge para dealers con perfil incompleto: sin logo o sin descripción.
 * Se oculta con la X (localStorage por usuario) y desaparece solo cuando
 * el perfil se completa. Los perfiles completos generan confianza en la
 * vitrina y aumentan los contactos.
 */
export default function BannerPerfilIncompleto({
  usuarioId,
  perfil,
}: {
  usuarioId: number;
  perfil: PerfilDealer;
}) {
  const [oculto, setOculto] = useState(
    () => localStorage.getItem(`nudge-perfil-oculto-${usuarioId}`) === "1",
  );

  const perfilIncompleto =
    !perfil.logoUrl?.trim() || !perfil.descripcion?.trim();

  if (oculto || !perfilIncompleto) return null;

  const ocultar = () => {
    localStorage.setItem(`nudge-perfil-oculto-${usuarioId}`, "1");
    setOculto(true);
  };

  return (
    <div className="mb-4 flex items-start justify-between gap-3 rounded-xl border border-amber-300/60 bg-amber-50 p-4 text-ink dark:border-amber-500/40 dark:bg-amber-500/10">
      <div className="flex items-start gap-3">
        <FaStore className="mt-1 shrink-0 text-lg text-amber-500" />
        <div>
          <p className="text-sm font-semibold">
            Completa tu perfil de agencia
          </p>
          <p className="mt-0.5 text-sm text-ink-2">
            Agrega logo, horarios y descripción: los compradores confían más en
            agencias con perfil completo.
          </p>
          <Link
            to="/dashboard/mi-perfil"
            className="mt-2 inline-block rounded-lg bg-amber-500 px-4 py-1.5 text-xs font-semibold text-white transition-colors hover:bg-amber-600"
          >
            Ir a Mi Perfil
          </Link>
        </div>
      </div>

      <button
        type="button"
        onClick={ocultar}
        aria-label="No volver a mostrar"
        className="shrink-0 rounded-lg p-1.5 text-ink-3 transition-colors hover:bg-hover hover:text-ink"
      >
        <FaTimes />
      </button>
    </div>
  );
}
