/** Google AdSense: publicador y flag de activación.
 *  El ID de publicador no es un secreto: AdSense exige que viaje embebido
 *  en el bundle. En desarrollo se mantiene desactivado (VITE_ADSENSE_ENABLED
 *  = false) para no generar tráfico inválido, que Google penaliza. */
export const ADSENSE_CLIENT = import.meta.env.VITE_ADSENSE_CLIENT ?? "";
export const ADSENSE_ENABLED =
  import.meta.env.VITE_ADSENSE_ENABLED === "true";

/** Unidades de anuncio responsive creadas en el panel de AdSense.
 *  Si más adelante se crean unidades adicionales (leaderboard, rectángulo,
 *  etc.), se registran aquí y se referencian desde AdUnit. */
export const ADSENSE_SLOTS = {
  responsive: "1963326036",
} as const;