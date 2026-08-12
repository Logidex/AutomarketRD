import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import MenuPublico from "./MenuPublico";
import logo from "../../assets/AutoMarketRD_Logo.svg";

interface LayoutPublicoProps {
  titulo: string;
  children: ReactNode;
}

export default function LayoutPublico({ titulo, children }: LayoutPublicoProps) {
  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      <header className="flex items-center justify-between border-b border-white/10 px-8 py-5">
        <div className="flex items-center gap-4">
          <Link to="/">
            <img
              src={logo}
              alt="AutoMarket RD"
              className="h-12 w-auto object-contain"
            />
          </Link>
          <span className="text-xl font-bold">{titulo}</span>
        </div>

        <MenuPublico />
      </header>

      <main className="mx-auto max-w-4xl px-8 py-16">{children}</main>

      <footer className="border-t border-white/10 py-8">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-6 text-sm text-[#9aa1b1] sm:flex-row sm:px-8">
          <span>© 2026 AutoMarket RD. Todos los derechos reservados.</span>
          <nav className="flex flex-wrap items-center justify-center gap-4">
            <Link to="/terminos" className="transition-colors hover:text-white">
              Términos
            </Link>
            <Link to="/privacidad" className="transition-colors hover:text-white">
              Privacidad
            </Link>
            <Link to="/reembolso" className="transition-colors hover:text-white">
              Reembolsos
            </Link>
            <Link to="/contacto" className="transition-colors hover:text-white">
              Contacto
            </Link>
            <Link to="/precios" className="transition-colors hover:text-white">
              Planes y precios
            </Link>
          </nav>
        </div>
      </footer>
    </div>
  );
}
