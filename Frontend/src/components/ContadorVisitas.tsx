import { useEffect, useState } from "react";
import { motion } from "motion/react";
import { FaEye } from "react-icons/fa";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { contadorService } from "../services/contador-visitantes.service";

function AnimatedNumber({ value }: { value: number }) {
  const [display, setDisplay] = useState(0);

  useEffect(() => {
    if (value === 0) return;

    const duration = 1200;
    const steps = 40;
    const increment = value / steps;
    let current = 0;
    let step = 0;

    const timer = setInterval(() => {
      step++;
      current = Math.min(Math.round(increment * step), value);
      setDisplay(current);

      if (step >= steps) {
        setDisplay(value);
        clearInterval(timer);
      }
    }, duration / steps);

    return () => clearInterval(timer);
  }, [value]);

  return <span>{display.toLocaleString("es-DO")}</span>;
}

function tiempoRelativo(fecha: Date): string {
  const ahora = new Date();
  const diffMs = ahora.getTime() - fecha.getTime();
  const diffMin = Math.floor(diffMs / 60000);

  if (diffMin < 1) return "ahora mismo";
  if (diffMin < 60) return `hace ${diffMin}m`;

  const diffHoras = Math.floor(diffMin / 60);
  if (diffHoras < 24) return `hace ${diffHoras}h`;

  const diffDias = Math.floor(diffHoras / 24);
  return `hace ${diffDias}d`;
}

export default function ContadorVisitas() {
  const queryClient = useQueryClient();

  const { data: total } = useQuery({
    queryKey: ["visit-counter"],
    queryFn: contadorService.obtener,
    staleTime: 1000 * 60 * 5,
    retry: 1,
  });

  const mutation = useMutation({
    mutationFn: contadorService.incrementar,
    onSuccess: (nuevoTotal) => {
      queryClient.setQueryData(["visit-counter"], nuevoTotal);
    },
  });

  useEffect(() => {
    mutation.mutate();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (total === undefined) return null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.6 }}
      className="mx-auto max-w-6xl px-6 sm:px-8"
    >
      <div className="flex items-center justify-center gap-3 rounded-2xl border border-line bg-surface/80 px-6 py-3.5 shadow-sm backdrop-blur-sm sm:gap-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-full bg-brand-soft">
          <FaEye className="h-4 w-4 text-brand" />
        </span>
        <div className="flex items-baseline gap-2 text-sm">
          <span className="font-bold tabular-nums text-ink">
            <AnimatedNumber value={total} />
          </span>
          <span className="text-ink-2">visitas</span>
        </div>
        <span className="hidden h-4 w-px bg-line sm:block" />
        <span className="hidden text-xs text-ink-3 sm:block">
          Última visita: {tiempoRelativo(new Date())}
        </span>
      </div>
    </motion.div>
  );
}
