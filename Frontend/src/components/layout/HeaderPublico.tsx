// Ubicación sugerida: src/components/layout/HeaderPublico.tsx
import { useState } from "react";
import { Link } from "react-router-dom";
import MenuPublico from "./MenuPublico";
import logo from "../../assets/AutoMarketRD_Logo.svg";

interface HeaderPublicoProps {
  /** Texto opcional junto al logo (ej. título de página interna) */
  titulo?: string;
}

export default function HeaderPublico({ titulo }: HeaderPublicoProps) {
  const [logoFallido, setLogoFallido] = useState(false);

  return (
    <header className="sticky top-0 z-30 flex items-center justify-between border-b border-line bg-page/80 px-6 py-2 backdrop-blur sm:px-8">
      <Link to="/" className="flex items-center gap-3">
        {logoFallido ? (
          <span className="text-lg font-bold text-ink">
            AutoMarket<span className="text-brand">RD</span>
          </span>
        ) : (
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-26 w-auto object-contain"
            onError={() => setLogoFallido(true)}
          />
        )}
        {titulo && <span className="text-lg font-bold text-ink">{titulo}</span>}
      </Link>

      <MenuPublico />
    </header>
  );
}