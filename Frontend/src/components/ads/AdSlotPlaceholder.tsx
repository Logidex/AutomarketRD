import { FaAd } from 'react-icons/fa';

interface Props {
  className?: string;
}

/**
 * Placeholder que se muestra cuando no hay anuncios para un slot.
 * invita a otros dealers a promocionar su negocio.
 */
export default function AdSlotPlaceholder({ className = '' }: Props) {
  return (
    <div
      className={`flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-line bg-surface/30 p-6 text-center ${className}`}
    >
      <FaAd className="mb-3 text-3xl text-ink-3/40" />
      <p className="text-sm font-medium text-ink-3">
        Espacio disponible
      </p>
      <p className="mt-1 text-xs text-ink-3/70">
        Promociona tu negocio aquí
      </p>
      <a
        href="/precios"
        className="mt-3 rounded-lg bg-amber-500/10 px-4 py-1.5 text-xs font-semibold text-amber-600 transition-colors hover:bg-amber-500/20"
      >
        Conoce nuestros planes
      </a>
    </div>
  );
}
