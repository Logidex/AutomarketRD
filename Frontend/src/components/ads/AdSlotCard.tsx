import { urlImagen } from '../../utils/imagen';
import type { AdSlotAnuncioPublico } from '../../types/adslot.types';

interface Props {
  anuncio: AdSlotAnuncioPublico;
  orientacion?: 'vertical' | 'horizontal';
  onClick?: () => void;
}

/**
 * Card individual de un anuncio publicitario.
 * Muestra imagen con object-fit cover y overlay con nombre del dealer.
 */
export default function AdSlotCard({ anuncio, orientacion = 'vertical', onClick }: Props) {
  const esVertical = orientacion === 'vertical';

  return (
    <button
      type="button"
      onClick={onClick}
      className={`group relative h-full w-full overflow-hidden transition-transform duration-200 ${
        onClick ? 'cursor-pointer hover:scale-[1.02]' : ''
      }`}
    >
      {/* Imagen */}
      <img
        src={urlImagen(anuncio.imagenUrl)}
        alt={anuncio.titulo || 'Anuncio patrocinado'}
        className="h-full w-full object-cover"
        loading="lazy"
      />

      {/* Overlay degradado inferior */}
      <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 via-black/30 to-transparent p-3">
        {/* Nombre del dealer */}
        <p className="truncate text-xs font-semibold text-white">
          {anuncio.nombreDealer}
        </p>

        {/* Título del anuncio (si existe) */}
        {anuncio.titulo && (
          <p className="mt-0.5 truncate text-[10px] text-white/80">
            {anuncio.titulo}
          </p>
        )}
      </div>
    </button>
  );
}
