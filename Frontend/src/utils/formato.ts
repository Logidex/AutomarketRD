export type Ciclo = "Mensual" | "Trimestral" | "Anual";

export const formatearRD$ = (valor: number | null | undefined): string => {
  const cantidad = Number(valor ?? 0);
  if (cantidad === 0) return "Gratis";
  return `RD$ ${cantidad.toLocaleString("es-DO")}`;
};

export const precioCicloDe = (
  plan: {
    precioMensual: number;
    precioTrimestral: number;
    precioAnual: number;
  },
  ciclo: Ciclo,
): number => {
  if (ciclo === "Trimestral") return plan.precioTrimestral;
  if (ciclo === "Anual") return plan.precioAnual;
  return plan.precioMensual;
};