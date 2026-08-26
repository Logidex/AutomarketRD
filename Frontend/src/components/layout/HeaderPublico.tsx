// Ubicación sugerida: src/components/layout/HeaderPublico.tsx
import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import MenuPublico from "./MenuPublico";

interface HeaderPublicoProps {
  /** Texto opcional junto al logo (ej. título de página interna) */
  titulo?: string;
}

export default function HeaderPublico({ titulo }: HeaderPublicoProps) {
  const [logoFallido, setLogoFallido] = useState(false);
  const [scrolled, setScrolled] = useState(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 20);
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <header
      className={`sticky top-0 z-40 flex items-center justify-between border-b px-4 py-3 sm:px-8 transition-all duration-300 ${
        scrolled
          ? "border-line/60 bg-page/80 backdrop-blur-xl shadow-sm shadow-black/5"
          : "border-line bg-page"
      }`}
    >
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