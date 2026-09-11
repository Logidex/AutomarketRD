import { useSlotsPublicos } from '../../hooks/useAdSlots';
import type { UbicacionAdSlot } from '../../types/adslot.types';
import AdSlotCarousel from './AdSlotCarousel';
import AdSlotPlaceholder from './AdSlotPlaceholder';

interface Props {
  ubicacion: UbicacionAdSlot;
  orientacion?: 'vertical' | 'horizontal';
  className?: string;
  fallback?: 'placeholder' | 'none';
}

/**
 * Componente que obtiene los anuncios de un slot por ubicación
 * y renderiza el carrusel o un placeholder si no hay anuncios.
 */
export default function AdSlotRenderer({
  ubicacion,
  orientacion = 'vertical',
  className = '',
  fallback = 'none',
}: Props) {
  const { data: slots, isLoading } = useSlotsPublicos(ubicacion);

  if (isLoading) {
    return (
      <div
        className={`animate-pulse rounded-2xl bg-surface/50 ${className}`}
        style={{ minHeight: 250 }}
      />
    );
  }

  const slot = slots?.[0];

  if (!slot || slot.anuncios.length === 0) {
    if (fallback === 'placeholder') {
      return <AdSlotPlaceholder className={className} />;
    }
    return null;
  }

  return (
    <AdSlotCarousel
      anuncios={slot.anuncios}
      intervaloMs={slot.intervaloRotacionSeg * 1000}
      ancho={slot.anchoPx}
      alto={slot.altoPx}
      orientacion={orientacion}
      className={className}
    />
  );
}
