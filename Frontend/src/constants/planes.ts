export const PLAN_NOMBRES: Record<string, string> = {
  Gratis: "Gratis",
  Basico: "Básico",
  Pro: "Pro",
  Elite: "Elite",
};

export function nombrePlan(nivel: string | null | undefined): string {
  if (!nivel) return "Sin plan";
  return PLAN_NOMBRES[nivel] ?? nivel;
}