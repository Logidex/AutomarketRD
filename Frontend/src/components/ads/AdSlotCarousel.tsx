import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { FaChevronLeft, FaChevronRight } from 'react-icons/fa';
import { adslotsService } from '../../services/adslots.service';
import type { AdSlotAnuncioPublico } from '../../types/adslot.types';
import AdSlotCard from './AdSlotCard';

interface Props {
  anuncios: AdSlotAnuncioPublico[];
  intervaloMs?: number;
  ancho?: number;
  alto?: number;
  orientacion?: 'vertical' | 'horizontal';
  className?: string;
}

/**
 * Construye una cola de rotación ponderada por prioridad.
 * Los anuncios con mayor prioridad se muestran más veces en el ciclo.
 */
function construirColaRotacion(anuncios: AdSlotAnuncioPublico[]): number[] {
  if (anuncios.length === 0) return [];

  const prioridades = anuncios.map((a) => a.prioridad || 1);
  const minPrioridad = Math.min(...prioridades);

  const cola: number[] = [];
  anuncios.forEach((anuncio, index) => {
    const repeticiones = Math.max(1, Math.round((anuncio.prioridad || 1) / minPrioridad));
    for (let i = 0; i < repeticiones; i++) {
      cola.push(index);
    }
  });

  for (let i = cola.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [cola[i], cola[j]] = [cola[j], cola[i]];
  }

  return cola;
}

/**
 * Carrusel que rota anuncios publicitarios con rotación ponderada por prioridad.
 * Los anuncios con mayor prioridad se muestran más veces en el ciclo.
 */
export default function AdSlotCarousel({
  anuncios,
  intervaloMs = 5000,
  ancho = 300,
  alto = 250,
  orientacion = 'vertical',
  className = '',
}: Props) {
  const [indiceActual, setIndiceActual] = useState(0);
  const [enPausa, setEnPausa] = useState(false);
  const intervaloRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const cola = useMemo(() => construirColaRotacion(anuncios), [anuncios]);

  const siguiente = useCallback(() => {
    if (cola.length === 0) return;
    setIndiceActual((prev) => (prev + 1) % cola.length);
  }, [cola.length]);

  const anterior = useCallback(() => {
    if (cola.length === 0) return;
    setIndiceActual((prev) => (prev - 1 + cola.length) % cola.length);
  }, [cola.length]);

  useEffect(() => {
    if (enPausa || cola.length <= 1) return;

    intervaloRef.current = setInterval(siguiente, intervaloMs);
    return () => {
      if (intervaloRef.current) clearInterval(intervaloRef.current);
    };
  }, [enPausa, siguiente, intervaloMs, cola.length]);

  // Registrar impresión (se ejecuta antes del early return para cumplir rules-of-hooks)
  const anuncioActual = cola.length > 0 ? anuncios[cola[indiceActual]] : null;
  useEffect(() => {
    if (anuncioActual) {
      adslotsService.registrarImpresion(anuncioActual.id).catch(() => {});
    }
  }, [anuncioActual]);

  if (anuncios.length === 0 || cola.length === 0 || !anuncioActual) return null;

  return (
    <div
      className={`relative overflow-hidden rounded-2xl border border-line bg-surface/60 ${className}`}
      onMouseEnter={() => setEnPausa(true)}
      onMouseLeave={() => setEnPausa(false)}
      style={{ width: ancho, height: alto }}
    >
      <AnimatePresence mode="wait">
        <motion.div
          key={`${anuncioActual.id}-${indiceActual}`}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          transition={{ duration: 0.3 }}
          className="h-full w-full"
        >
          <AdSlotCard
            anuncio={anuncioActual}
            orientacion={orientacion}
            onClick={() => {
              adslotsService.registrarClick(anuncioActual.id).catch(() => {});
              if (anuncioActual.enlace) {
                window.open(anuncioActual.enlace, '_blank', 'noopener,noreferrer');
              }
            }}
          />
        </motion.div>
      </AnimatePresence>

      {cola.length > 1 && (
        <>
          <button
            type="button"
            onClick={anterior}
            className="absolute left-1 top-1/2 z-10 flex h-7 w-7 -translate-y-1/2 items-center justify-center rounded-full bg-black/40 text-white backdrop-blur-sm transition-colors hover:bg-black/60"
          >
            <FaChevronLeft className="h-3 w-3" />
          </button>
          <button
            type="button"
            onClick={siguiente}
            className="absolute right-1 top-1/2 z-10 flex h-7 w-7 -translate-y-1/2 items-center justify-center rounded-full bg-black/40 text-white backdrop-blur-sm transition-colors hover:bg-black/60"
          >
            <FaChevronRight className="h-3 w-3" />
          </button>
        </>
      )}

      {cola.length > 1 && (
        <div className="absolute bottom-2 left-0 right-0 flex justify-center gap-1">
          {Array.from(new Set(cola)).map((_, i) => (
            <div
              key={i}
              className={`h-1.5 rounded-full transition-all duration-300 ${
                i === cola[indiceActual]
                  ? 'w-4 bg-amber-500'
                  : 'w-1.5 bg-white/40'
              }`}
            />
          ))}
        </div>
      )}

      <div className="absolute left-2 top-2 z-10 flex items-center gap-1 rounded-full bg-amber-500/90 px-2 py-0.5 text-[10px] font-bold text-white backdrop-blur-sm">
        ★ Patrocinado
      </div>
    </div>
  );
}
