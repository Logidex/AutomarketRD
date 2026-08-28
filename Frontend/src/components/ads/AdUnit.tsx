import { useEffect, useRef } from "react";
import { useConsent } from "../../context/ConsentContext";
import {
  ADSENSE_CLIENT,
  ADSENSE_ENABLED,
  ADSENSE_SLOTS,
} from "../../constants/adsense";
import { cargarScriptAdsense } from "../../hooks/useAdsense";

interface PropsAdUnit {
  /** Slot de AdSense. Default: unidad responsive del catálogo de adsense.ts. */
  slot?: string;
  /** Muestra la etiqueta legal "Publicidad" sobre la unidad. */
  etiqueta?: boolean;
  /** Clases extra para el contenedor que envuelve la unidad. */
  className?: string;
}

/**
 * Unidad de anuncio de Google AdSense. Solo se renderiza y hace push cuando:
 *  - la publicidad está habilitada en el entorno (VITE_ADSENSE_ENABLED=true),
 *  - hay ID de publicador configurado, y
 *  - el usuario aceptó el consentimiento.
 * El push se ejecuta una sola vez por elemento (guard con ref/dataset) para
 * evitar el error de "duplicate push" con renders dobles o re-montajes de una
 * ruta SPA a otra.
 */
export default function AdUnit({
  slot = ADSENSE_SLOTS.responsive,
  etiqueta = true,
  className,
}: PropsAdUnit) {
  const { estado } = useConsent();
  const insRef = useRef<HTMLModElement>(null);
  const consentimientoOk =
    estado === "aceptado" && ADSENSE_ENABLED && ADSENSE_CLIENT;

  useEffect(() => {
    if (!consentimientoOk) return;

    let cancelado = false;

    cargarScriptAdsense(ADSENSE_CLIENT).then((cargado) => {
      if (cancelado || !cargado) return;
      const ins = insRef.current;
      if (!ins || ins.dataset.adState === "cargado") return;

      ins.dataset.adState = "cargado";
      try {
        (window.adsbygoogle = window.adsbygoogle || []).push({});
      } catch {
        // Ad-blocker o SDK no disponible: no romper la página.
      }
    });

    return () => {
      cancelado = true;
    };
  }, [consentimientoOk]);

  if (!consentimientoOk) return null;

  return (
    <div className={className} aria-label={etiqueta ? "Publicidad" : undefined}>
      {etiqueta && (
        <p className="mb-1.5 text-center text-[10px] font-medium uppercase tracking-[0.16em] text-ink-3">
          Publicidad
        </p>
      )}
      {/* AdSense inyecta el anuncio real dentro de un iframe aquí */}
      <ins
        ref={insRef}
        className="adsbygoogle block h-auto min-h-[90px] w-full"
        data-ad-client={ADSENSE_CLIENT}
        data-ad-slot={slot}
        data-ad-format="auto"
        data-full-width-responsive="true"
      />
    </div>
  );
}