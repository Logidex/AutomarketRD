export type Ciclo = "Mensual" | "Trimestral" | "Anual";

export const formatearRD$ = (valor: number | null | undefined): string => {
  const cantidad = Number(valor ?? 0);
  if (cantidad === 0) return "Gratis";
  return `RD$ ${cantidad.toLocaleString("es-DO")}`;
};

export const formatearPrecio = (
  valor: number | null | undefined,
  moneda?: string | null,
): string => {
  const cantidad = Number(valor ?? 0);
  const divisa = moneda?.toUpperCase() ?? "DOP";

  if (divisa === "USD") {
    return `US$ ${cantidad.toLocaleString("es-DO")}`;
  }
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

/**
 * Formatea un texto crudo de un input numérico: separa los miles con comas y
 * permite un decimal (`.` o `,`) de hasta 2 dígitos. Ej: "1500000" -> "1,500,000",
 * "1299.5" -> "1,299.5".
 */
export const formatearNumeroInput = (raw: string): string => {
  if (!raw) return "";
  const limpio = raw.replace(/[^0-9.,]/g, "");
  const idxSeparador = Math.max(limpio.lastIndexOf("."), limpio.lastIndexOf(","));
  const resto = idxSeparador >= 0 ? limpio.slice(idxSeparador + 1) : "";
  const esDecimal = idxSeparador >= 0 && resto.length <= 2;
  const entero = esDecimal
    ? limpio.slice(0, idxSeparador).replace(/[.,]/g, "")
    : limpio.replace(/[.,]/g, "");
  const decimal = esDecimal ? "." + resto : "";
  const enteroFormateado = entero.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  return enteroFormateado + decimal;
};

/** Convierte el texto (con o sin separadores) al número que representa. */
export const parsearNumeroInput = (texto: string): number => {
  if (!texto.trim()) return 0;
  const numero = Number(formatearNumeroInput(texto).replace(/,/g, ""));
  return Number.isFinite(numero) ? numero : 0;
};