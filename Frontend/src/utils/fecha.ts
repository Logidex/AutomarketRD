export const formatearFecha = (iso: string | null | undefined, conHora = false): string => {
  if (!iso) return "—";

  const fecha = new Date(iso);

  const opciones: Intl.DateTimeFormatOptions = conHora
    ? {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      }
    : {
        day: "2-digit",
        month: "short",
        year: "numeric",
      };

  return fecha.toLocaleDateString("es-DO", opciones);
};