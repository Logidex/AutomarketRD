import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { FaChevronDown } from "react-icons/fa";
import NavbarUsuario from "./NavbarUsuario";

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
  const contenedorRef = useRef<HTMLDivElement>(null);
  const contenedorLegalRef = useRef<HTMLDivElement>(null);

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

  return (
    <nav className="flex items-center gap-6 text-sm font-medium">
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
          className="flex items-center gap-1.5 text-[#9aa1b1] transition-colors hover:text-white"
        >
          Compra tu carro
          <FaChevronDown className="text-[10px]" />
        </button>

        {menuAbierto && (
          <div
            role="menu"
            className="absolute right-0 top-full z-50 w-72 pt-2"
          >
            <div className="overflow-hidden rounded-xl border border-white/10 bg-[#13161d] py-2 shadow-2xl">
              {OPCIONES_COMPRA.map((opcion) => (
                <Link
                  key={opcion.to}
                  to={opcion.to}
                  role="menuitem"
                  onClick={() => setMenuAbierto(false)}
                  className="block px-5 py-3 transition-colors hover:bg-white/5"
                >
                  <span className="block font-semibold text-white">
                    {opcion.etiqueta}
                  </span>
                  <span className="block text-xs text-[#9aa1b1]">
                    {opcion.descripcion}
                  </span>
                </Link>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* VENDE TU CARRO */}
      <Link
        to="/registro"
        className="text-[#9aa1b1] transition-colors hover:text-white"
      >
        Vende tu carro
      </Link>

      {/* DIRECTORIO */}
      <Link
        to="/vehiculos"
        className="text-[#9aa1b1] transition-colors hover:text-white"
      >
        Directorio
      </Link>

      {/* COMPARADOR */}
      <Link
        to="/comparador"
        className="text-[#9aa1b1] transition-colors hover:text-white"
      >
        Comparar
      </Link>

      <Link
        to="/precios"
        className="text-[#9aa1b1] transition-colors hover:text-white"
      >
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
          className="flex items-center gap-1.5 text-[#9aa1b1] transition-colors hover:text-white"
        >
          Legal
          <FaChevronDown className="text-[10px]" />
        </button>

        {menuLegalAbierto && (
          <div
            role="menu"
            className="absolute right-0 top-full z-50 w-72 pt-2"
          >
            <div className="overflow-hidden rounded-xl border border-white/10 bg-[#13161d] py-2 shadow-2xl">
              {OPCIONES_LEGAL.map((opcion) => (
                <Link
                  key={opcion.to}
                  to={opcion.to}
                  role="menuitem"
                  onClick={() => setMenuLegalAbierto(false)}
                  className="block px-5 py-3 transition-colors hover:bg-white/5"
                >
                  <span className="block font-semibold text-white">
                    {opcion.etiqueta}
                  </span>
                  <span className="block text-xs text-[#9aa1b1]">
                    {opcion.descripcion}
                  </span>
                </Link>
              ))}
            </div>
          </div>
        )}
      </div>

      <NavbarUsuario />
    </nav>
  );
}