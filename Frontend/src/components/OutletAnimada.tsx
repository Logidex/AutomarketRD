import { useLocation, useOutlet } from "react-router-dom";

// Envuelve el Outlet de un layout para que el contenido cambie con una
// animación sutil SIN remontar el layout (navbar/sidebar no parpadean ni
// vuelven a pedir datos). La key por pathname remonta solo el contenido.
export default function OutletAnimada() {
  const location = useLocation();
  const outlet = useOutlet();

  return (
    <div key={location.pathname} className="animar-pagina">
      {outlet}
    </div>
  );
}
