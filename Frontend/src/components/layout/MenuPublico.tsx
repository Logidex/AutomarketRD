import { useEffect, useRef, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { FaBars, FaChevronDown, FaTimes } from "react-icons/fa";
import NavbarUsuario from "./NavbarUsuario";
import BotonTema from "../BotonTema";

interface OpcionCompra {
  etiqueta: string;
  descripcion: string;
  to: string;
}

interface OpcionLegal {
  etiqueta: string;
  descripcion: string;
  to: string;
}

const OPCIONES_COMPRA: OpcionCompra[] = [
  {
    etiqueta: "Carros nuevos",
    descripcion: "Vehículos 0 kilómetros",
    to: "/vehiculos?condicion=Nuevo",
  },
  {
    etiqueta: "Carros usados",
    descripcion: "Vehículos con kilometraje",
    to: "/vehiculos?condicion=Usado",
  },
  {
    etiqueta: "Eléctricos",
    descripcion: "Impulsados por electricidad",
    to: "/vehiculos?combustible=Electrico",
  },
  {
    etiqueta: "Híbridos",
    descripcion: "Gasolina + eléctrico",
    to: "/vehiculos?combustible=Hibrido",
  },
  {
    etiqueta: "En oferta",
    descripcion: "Precios con descuento",
    to: "/vehiculos?enOferta=true",
  },
];

const OPCIONES_LEGAL: OpcionLegal[] = [
  {
    etiqueta: "Términos y Condiciones",
    descripcion: "Reglas de uso de la plataforma",
    to: "/terminos",
  },
  {
    etiqueta: "Política de Privacidad",
    descripcion: "Tratamiento de datos personales",
    to: "/privacidad",
  },
  {
    etiqueta: "Política de Reembolso",
    descripcion: "Condiciones para devoluciones",
    to: "/reembolso",
  },
  {
    etiqueta: "Contacto",
    descripcion: "Soporte y consultas",
    to: "/contacto",
  },
];

export default function MenuPublico() {
  const [menuAbierto, setMenuAbierto] = useState(false);
  const [menuLegalAbierto, setMenuLegalAbierto] = useState(false);
  const [menuMovilAbierto, setMenuMovilAbierto] = useState(false);
  const contenedorRef = useRef<HTMLDivElement>(null);
  const contenedorLegalRef = useRef<HTMLDivElement>(null);
  const location = useLocation();

  useEffect(() => {
    const cerrarFuera = (e: MouseEvent) => {
      if (
        contenedorRef.current &&
        !contenedorRef.current.contains(e.target as Node)
      ) {
        setMenuAbierto(false);
      }
      if (
        contenedorLegalRef.current &&
        !contenedorLegalRef.current.contains(e.target as Node)
      ) {
        setMenuLegalAbierto(false);
      }
    };

    document.addEventListener("mousedown", cerrarFuera);
    return () => document.removeEventListener("mousedown", cerrarFuera);
  }, []);

  const cerrarMenuMovil = () => setMenuMovilAbierto(false);

  // Píldora de navegación: resalta la sección activa y suaviza el hover
  const clasePildora = (to: string, conActivo = true) => {
    const activa =
      conActivo &&
      (location.pathname === to ||
        location.pathname.startsWith(`${to}/`) ||
        location.pathname.startsWith(`${to}?`));

    return `rounded-full px-3 py-2 font-medium transition-colors xl:px-3.5 xl:text-[15px] ${
      activa
        ? "bg-brand-soft text-brand"
        : "text-ink-2 hover:bg-hover hover:text-ink"
    }`;
  };

  const claseDisparador =
    "flex items-center gap-1.5 rounded-full px-3 py-2 text-ink-2 transition-colors hover:bg-hover hover:text-ink xl:px-3.5 xl:text-[15px]";

  return (
    <>
      {/* NAVEGACIÓN ESCRITORIO */}
      <nav className="hidden items-center gap-1 text-sm font-medium lg:flex xl:gap-1.5">
        {/* COMPRA TU CARRO */}
        <div
          ref={contenedorRef}
          className="relative"
          onMouseEnter={() => setMenuAbierto(true)}
          onMouseLeave={() => setMenuAbierto(false)}
        >
          <button
            type="button"
            aria-expanded={menuAbierto}
            aria-haspopup="menu"
            onClick={() => setMenuAbierto((abierto) => !abierto)}
            className={claseDisparador}
          >
            Compra tu carro
            <FaChevronDown className="text-[10px]" />
          </button>

          {menuAbierto && (
            <div
              role="menu"
              className="absolute right-0 top-full z-50 w-72 pt-2"
            >
              <div className="overflow-hidden rounded-xl border border-line bg-surface py-2 shadow-2xl">
                {OPCIONES_COMPRA.map((opcion) => (
                  <Link
                    key={opcion.to}
                    to={opcion.to}
                    role="menuitem"
                    onClick={() => setMenuAbierto(false)}
                    className="block px-5 py-3 transition-colors hover:bg-hover"
                  >
                    <span className="block font-semibold text-ink">
                      {opcion.etiqueta}
                    </span>
                    <span className="block text-xs text-ink-2">
                      {opcion.descripcion}
                    </span>
                  </Link>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* VENDE TU CARRO */}
        <Link to="/registro" className={clasePildora("/registro", false)}>
          Vende tu carro
        </Link>

        {/* DIRECTORIO */}
        <Link to="/vehiculos" className={clasePildora("/vehiculos")}>
          Directorio
        </Link>

        {/* AGENCIAS */}
        <Link to="/agencias" className={clasePildora("/agencias")}>
          Agencias
        </Link>

        {/* COMPARADOR */}
        <Link to="/comparador" className={clasePildora("/comparador")}>
          Comparar
        </Link>

        <Link to="/precios" className={clasePildora("/precios")}>
          Precios
        </Link>

        {/* LEGAL */}
        <div
          ref={contenedorLegalRef}
          className="relative"
          onMouseEnter={() => setMenuLegalAbierto(true)}
          onMouseLeave={() => setMenuLegalAbierto(false)}
        >
          <button
            type="button"
            aria-expanded={menuLegalAbierto}
            aria-haspopup="menu"
            onClick={() => setMenuLegalAbierto((abierto) => !abierto)}
            className={claseDisparador}
          >
            Legal
            <FaChevronDown className="text-[10px]" />
          </button>

          {menuLegalAbierto && (
            <div
              role="menu"
              className="absolute right-0 top-full z-50 w-72 pt-2"
            >
              <div className="overflow-hidden rounded-xl border border-line bg-surface py-2 shadow-2xl">
                {OPCIONES_LEGAL.map((opcion) => (
                  <Link
                    key={opcion.to}
                    to={opcion.to}
                    role="menuitem"
                    onClick={() => setMenuLegalAbierto(false)}
                    className="block px-5 py-3 transition-colors hover:bg-hover"
                  >
                    <span className="block font-semibold text-ink">
                      {opcion.etiqueta}
                    </span>
                    <span className="block text-xs text-ink-2">
                      {opcion.descripcion}
                    </span>
                  </Link>
                ))}
              </div>
            </div>
          )}
        </div>

        <BotonTema />

        <NavbarUsuario />
      </nav>

      {/* NAVEGACIÓN MÓVIL */}
      <div className="flex items-center gap-2 lg:hidden">
        <BotonTema />
        <NavbarUsuario />
        <button
          type="button"
          onClick={() => setMenuMovilAbierto((abierto) => !abierto)}
          aria-label={menuMovilAbierto ? "Cerrar menú" : "Abrir menú"}
          aria-expanded={menuMovilAbierto}
          className="rounded-lg p-2 text-ink-2 transition-colors hover:bg-hover"
        >
          {menuMovilAbierto ? <FaTimes className="text-lg" /> : <FaBars className="text-lg" />}
        </button>
      </div>

      {/* PANEL MÓVIL */}
      {menuMovilAbierto && (
        <div className="absolute inset-x-0 top-full z-50 max-h-[calc(100vh-72px)] overflow-y-auto border-b border-line bg-surface shadow-2xl lg:hidden">
          <nav className="space-y-1 p-4 text-[15px] font-medium">
            <Link
              to="/vehiculos"
              onClick={cerrarMenuMovil}
              className="block rounded-xl px-3.5 py-3 text-ink hover:bg-hover"
            >
              Directorio
            </Link>
            <Link
              to="/agencias"
              onClick={cerrarMenuMovil}
              className="block rounded-xl px-3.5 py-3 text-ink hover:bg-hover"
            >
              Agencias
            </Link>
            <Link
              to="/comparador"
              onClick={cerrarMenuMovil}
              className="block rounded-xl px-3.5 py-3 text-ink hover:bg-hover"
            >
              Comparar
            </Link>
            <Link
              to="/precios"
              onClick={cerrarMenuMovil}
              className="block rounded-xl px-3.5 py-3 text-ink hover:bg-hover"
            >
              Precios
            </Link>
            <Link
              to="/registro"
              onClick={cerrarMenuMovil}
              className="block rounded-xl px-3.5 py-3 text-ink hover:bg-hover"
            >
              Vende tu carro
            </Link>

            <p className="px-3.5 pt-4 pb-1 text-xs font-semibold uppercase tracking-wide text-ink-3">
              Compra tu carro
            </p>
            {OPCIONES_COMPRA.map((opcion) => (
              <Link
                key={opcion.to}
                to={opcion.to}
                onClick={cerrarMenuMovil}
                className="block rounded-xl px-3.5 py-2.5 text-ink-2 hover:bg-hover"
              >
                {opcion.etiqueta}
              </Link>
            ))}

            <p className="px-3.5 pt-4 pb-1 text-xs font-semibold uppercase tracking-wide text-ink-3">
              Legal
            </p>
            {OPCIONES_LEGAL.map((opcion) => (
              <Link
                key={opcion.to}
                to={opcion.to}
                onClick={cerrarMenuMovil}
                className="block rounded-xl px-3.5 py-2.5 text-ink-2 hover:bg-hover"
              >
                {opcion.etiqueta}
              </Link>
            ))}
          </nav>
        </div>
      )}
    </>
  );
}
