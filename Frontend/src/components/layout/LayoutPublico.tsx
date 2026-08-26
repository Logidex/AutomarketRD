// Ubicación: src/components/layout/LayoutPublico.tsx
import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import HeaderPublico from "./HeaderPublico";
import SectionBackground from "../SectionBackground";

interface LayoutPublicoProps {
  titulo: string;
  children: ReactNode;
}

export default function LayoutPublico({ titulo, children }: LayoutPublicoProps) {
  return (
    <div className="relative min-h-screen overflow-hidden bg-page text-ink">
      <HeaderPublico titulo={titulo} />

      <SectionBackground variant="recent" className="mx-auto max-w-4xl px-8 py-16">
      <main className="mx-auto max-w-4xl px-8 py-16">{children}</main>
      </SectionBackground>

      <footer className="border-t border-line py-8">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-6 text-sm text-ink-2 sm:flex-row sm:px-8">
          <span>© 2026 AutoMarket RD. Todos los derechos reservados.</span>
          <nav className="flex flex-wrap items-center justify-center gap-4">
            <Link to="/terminos" className="transition-colors hover:text-ink">
              Términos
            </Link>
            <Link to="/privacidad" className="transition-colors hover:text-ink">
              Privacidad
            </Link>
            <Link to="/reembolso" className="transition-colors hover:text-ink">
              Reembolsos
            </Link>
            <Link to="/contacto" className="transition-colors hover:text-ink">
              Contacto
            </Link>
            <Link to="/precios" className="transition-colors hover:text-ink">
              Planes y precios
            </Link>
          </nav>
        </div>
      </footer>
    </div>
  );
}
