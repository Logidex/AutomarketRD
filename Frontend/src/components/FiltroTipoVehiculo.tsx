import { FaCheck } from "react-icons/fa";

interface OpcionTipoVehiculo {
  valor: string;
  etiqueta: string;
  imagen: string;
}

const OPCIONES: OpcionTipoVehiculo[] = [
  { valor: "Jeepeta", etiqueta: "Todo terreno", imagen: "/imagenes-filtros/suv.jpg" },
  { valor: "Sedan", etiqueta: "Sedán", imagen: "/imagenes-filtros/sedan.jpg" },
  { valor: "Camioneta", etiqueta: "Pick-up", imagen: "/imagenes-filtros/pickup.jpg" },
  { valor: "Hatchback", etiqueta: "Hatchback", imagen: "/imagenes-filtros/hatchback.jpg" },
  { valor: "Deportivo", etiqueta: "Deportivo", imagen: "/imagenes-filtros/deportivo.jpg" },
  { valor: "SuperDeportivo", etiqueta: "Súper Deportivo", imagen: "/imagenes-filtros/superdeportivo.jpg" },
  { valor: "Hypercar", etiqueta: "Hypercar", imagen: "/imagenes-filtros/hypercar.jpg" },
  { valor: "Coupe", etiqueta: "Coupé", imagen: "/imagenes-filtros/coupe.jpg" },
  { valor: "Convertible", etiqueta: "Convertible", imagen: "/imagenes-filtros/convertible.jpg" },
  { valor: "Minivan", etiqueta: "Minivan", imagen: "/imagenes-filtros/minivan.jpeg" },
  { valor: "Camion", etiqueta: "Camión", imagen: "/imagenes-filtros/camion.jpg" },
  { valor: "Motor", etiqueta: "Moto", imagen: "/imagenes-filtros/moto.jpg" },
];

type Size = "default" | "compact" | "large";

interface PropsFiltroTipoVehiculo {
  valor: string;
  onSeleccionar: (valor: string) => void;
  size?: Size;
  className?: string;
}

const SIZE_CLASSES: Record<Size, string> = {
  default: "grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-6",
  compact: "grid-cols-3 gap-2 sm:grid-cols-4 lg:grid-cols-6",
  large: "grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6",
};

const IMAGE_ASPECT: Record<Size, string> = {
  default: "aspect-[4/3]",
  compact: "aspect-square",
  large: "aspect-[4/3]",
};

const LABEL_SIZE: Record<Size, string> = {
  default: "text-xs",
  compact: "text-[10px]",
  large: "text-sm",
};

export default function FiltroTipoVehiculo({
  valor,
  onSeleccionar,
  size = "default",
  className = "",
}: PropsFiltroTipoVehiculo) {
  return (
    <div className={`${SIZE_CLASSES[size]} ${className}`}>
      {OPCIONES.map((opcion) => {
        const seleccionado = valor === opcion.valor;
        return (
          <button
            key={opcion.valor}
            type="button"
            aria-pressed={seleccionado}
            onClick={() => onSeleccionar(seleccionado ? "" : opcion.valor)}
            className="group flex w-full flex-col items-center gap-1.5 outline-none"
          >
            <span
              className={`relative block w-full overflow-hidden rounded-2xl border-2 transition-all duration-200 ${
                seleccionado
                  ? "border-brand shadow-lg shadow-brand/30"
                  : "border-transparent ring-1 ring-line group-hover:ring-brand/40"
              }`}
            >
              <img
                src={opcion.imagen}
                alt={opcion.etiqueta}
                loading="lazy"
                className={`${IMAGE_ASPECT[size]} w-full object-cover transition-transform duration-200 group-hover:scale-105`}
              />
              {seleccionado && (
                <span className="absolute inset-0 flex items-center justify-center bg-brand/30">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-brand text-sm text-white shadow-md">
                    <FaCheck />
                  </span>
                </span>
              )}
            </span>
            <span
              className={`${LABEL_SIZE[size]} font-semibold transition-colors ${
                seleccionado ? "text-brand" : "text-ink-2 group-hover:text-ink"
              }`}
            >
              {opcion.etiqueta}
            </span>
          </button>
        );
      })}
    </div>
  );
}