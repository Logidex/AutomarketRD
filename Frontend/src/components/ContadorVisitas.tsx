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
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.6 }}
      className="mt-7 flex justify-center"
    >
      <span className="inline-flex items-center gap-2 rounded-full border border-line/70 bg-surface/60 px-4 py-1.5 text-xs text-ink-3 backdrop-blur-sm">
        <FaEye className="h-3.5 w-3.5 text-brand/80" aria-hidden="true" />
        <span className="font-semibold tabular-nums text-ink-2">
          <AnimatedNumber value={total} />
        </span>
        <span>visitas hasta hoy</span>
      </span>
    </motion.div>
  );
}
