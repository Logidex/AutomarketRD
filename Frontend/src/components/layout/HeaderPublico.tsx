// Ubicación sugerida: src/components/layout/HeaderPublico.tsx
import { useState } from "react";
import { Link } from "react-router-dom";
import MenuPublico from "./MenuPublico";

interface HeaderPublicoProps {
  /** Texto opcional junto al logo (ej. título de página interna) */
  titulo?: string;
}

export default function HeaderPublico({ titulo }: HeaderPublicoProps) {
  const [logoFallido, setLogoFallido] = useState(false);

  return (
    // Fondo sólido: el 80% + blur hacía ilegible el text-ink-2 del menú
    // sobre el hero de Home.
    <header className="sticky top-0 z-40 relative flex items-center justify-between border-b border-line bg-page px-4 py-3 sm:px-8">
      <Link to="/" className="flex items-center gap-3">
        {logoFallido ? (
          <span className="text-lg font-bold text-ink">
            AutoMarket<span className="text-brand">RD</span>
          </span>
        ) : (
          <img
            src="/automarket-rdlogo-opt.png"
            alt="AutoMarket RD"
            className="h-14 w-auto object-contain sm:h-20"
            onError={() => setLogoFallido(true)}
          />
        )}
        {titulo && <span className="text-lg font-bold text-ink">{titulo}</span>}
      </Link>

      <MenuPublico />
    </header>
  );
}