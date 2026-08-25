import { useEffect } from "react";
import { useLocation } from "react-router-dom";

// React Router (modo libreria) no reinicia el scroll al navegar: sin esto,
// abrir un anuncio desde una tarjeta a media pagina hereda esa posicion
// y el detalle aparece a mitad de contenido en lugar de al inicio.
export default function ScrollToTop() {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);

  return null;
}
