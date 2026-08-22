import { useEffect, useState } from "react";

// Fallback de navegación: una barra fina arriba que solo aparece si la
// nueva página tarda más de 250 ms en cargar (la mayoría carga al instante,
// así que casi nunca se ve). Reemplaza al antiguo spinner de pantalla
// completa, que hacía los cambios de página sentirse bruscos.
export default function BarraProgresoNavegacion() {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const timer = window.setTimeout(() => setVisible(true), 250);
    return () => window.clearTimeout(timer);
  }, []);

  if (!visible) return null;

  return (
    <div className="barra-progreso-navegacion" role="progressbar" aria-label="Cargando página" />
  );
}
