import { useEffect, useRef, useState, type FormEvent, type ReactNode } from "react";
import { Link, useSearchParams } from "react-router-dom";
import {
  FaBalanceScale,
  FaCalendarAlt,
  FaCar,
  FaCheck,
  FaCogs,
  FaExchangeAlt,
  FaMapMarkerAlt,
  FaMinus,
  FaOilCan,
  FaPalette,
  FaPlus,
  FaSearch,
  FaTachometerAlt,
  FaTint,
} from "react-icons/fa";
import type { VehiculoComparador } from "../services/comparador.service";
import type { AnuncioListado } from "../types/anuncio.types";
import { useComparador } from "../context/ComparadorContext";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";
import { useCompararVehiculos, useBuscarComparador } from "../hooks/useComparador";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from "../constants/vehiculo.opciones";

const fotoPrincipal = (v: VehiculoComparador): string =>
  urlImagen(v.fotoPrincipal) || "https://via.placeholder.com/600x400?text=Sin+Foto";

const fotoAnuncio = (a: AnuncioListado): string =>
  urlImagen(a.fotos?.[0]) || "https://via.placeholder.com/600x400?text=Sin+Foto";


const MAX_VEHICULOS = 4;

interface Especificaciones {
  etiqueta: string;
  icono: ReactNode;
  valor: string;
  destacado?: boolean;
}

interface PropsBuscador {
  termino: string;
  pendiente: boolean;
  resultados: AnuncioListado[];
  esSeleccionado: (id: number) => boolean;
  onChangeTermino: (valor: string) => void;
  onBuscar: (e: FormEvent) => void;
  onAgregar: (anuncio: AnuncioListado) => void;
}

function BuscadorVehiculos({
  termino,
  pendiente,
  resultados,
  esSeleccionado,
  onChangeTermino,
  onBuscar,
  onAgregar,
}: PropsBuscador) {
  return (
    <form onSubmit={onBuscar} className="mt-8">
      <div className="flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-1">
          <FaSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-gray-500" />
          <label htmlFor="buscarVehiculo" className="sr-only">
            Buscar un vehículo por marca o modelo
          </label>
          <input
            id="buscarVehiculo"
            type="text"
            value={termino}
            onChange={(e) => onChangeTermino(e.target.value)}
            placeholder="Buscar un vehículo por marca o modelo para agregarlo..."
            className="w-full rounded-xl border border-white/10 bg-[#0c101b] py-3 pl-12 pr-4 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
          />
        </div>
        <button
          type="submit"
          disabled={pendiente || !termino.trim()}
          className="rounded-xl bg-blue-500 px-6 py-3 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:bg-blue-500/40"
        >
          {pendiente ? "Buscando..." : "Buscar"}
        </button>
      </div>

      {resultados.length > 0 && (
        <div className="mt-3 overflow-hidden rounded-xl border border-white/10 bg-[#13161d]">
          {resultados.map((a) => {
            const yaSeleccionado = esSeleccionado(a.id);
            return (
              <div
                key={a.id}
                className={`flex items-center gap-4 border-b border-white/5 p-3 last:border-b-0 ${
                  yaSeleccionado ? "opacity-50" : ""
                }`}
              >
                <img
                  src={fotoAnuncio(a)}
                  alt={`${a.marca} ${a.modelo}`}
                  className="h-14 w-20 rounded-lg object-cover"
                />
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold">
                    {a.marca} {a.modelo}
                    <span className="ml-2 text-xs font-normal text-[#9aa1b1]">
                      {a.anio}
                    </span>
                  </p>
                  <p className="text-xs text-blue-400">
                    {formatearPrecio(a.precio, a.moneda)} ·{" "}
                    {a.kilometraje.toLocaleString("es-DO")} km
                  </p>
                </div>
                <button
                  type="button"
                  disabled={yaSeleccionado}
                  onClick={() => onAgregar(a)}
                  className="inline-flex shrink-0 items-center gap-1.5 rounded-lg border border-white/10 px-3 py-2 text-xs font-semibold transition-colors hover:border-blue-500/40 hover:text-blue-400 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {yaSeleccionado ? (
                    <>
                      <FaCheck className="text-green-400" />
                      Agregado
                    </>
                  ) : (
                    <>
                      <FaPlus />
                      Agregar
                    </>
                  )}
                </button>
              </div>
            );
          })}
        </div>
      )}
    </form>
  );
}

interface PropsContenido {
  vehiculos: VehiculoComparador[];
  seleccionados: number[];
  cargando: boolean;
  error: string;
  necesitaSeleccion: boolean;
  menorPrecio: number | null;
  onRefetch: () => void;
  onLimpiar: () => void;
  onRemover: (id: number) => void;
}

function ContenidoComparador({
  vehiculos,
  seleccionados,
  cargando,
  error,
  necesitaSeleccion,
  menorPrecio,
  onRefetch,
  onLimpiar,
  onRemover,
}: PropsContenido) {
  const filaComparativa = (
    etiqueta: string,
    obtener: (v: VehiculoComparador) => ReactNode,
    destacado?: (v: VehiculoComparador) => boolean,
  ) => (
    <tr className="border-b border-white/5 transition-colors hover:bg-white/[0.03]">
      <th className="w-44 whitespace-nowrap px-6 py-4 text-left align-top text-sm font-semibold text-[#9aa1b1]">
        {etiqueta}
      </th>
      {vehiculos.map((v) => (
        <td
          key={v.id}
          className={`px-6 py-4 text-sm ${
            destacado?.(v) ? "font-semibold text-green-400" : "text-gray-200"
          }`}
        >
          {obtener(v)}
        </td>
      ))}
    </tr>
  );

  if (necesitaSeleccion) {
    return (
      <div className="rounded-2xl border border-white/10 bg-[#13161d] p-12 text-center">
        <FaCar className="mx-auto text-5xl text-gray-600" />
        <h3 className="mt-4 text-lg font-semibold">
          Agrega vehículos para comparar
        </h3>
        <p className="mx-auto mt-2 max-w-md text-sm text-[#9aa1b1]">
          Usa el buscador de arriba o ve al directorio y presiona{" "}
          <span className="font-semibold text-blue-400">
            "+ Agregar a comparar"
          </span>{" "}
          en al menos dos vehículos.
        </p>
        <Link
          to="/vehiculos"
          className="mt-6 inline-flex items-center gap-2 rounded-xl bg-blue-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600"
        >
          Ir al directorio
        </Link>
      </div>
    );
  }

  if (cargando) {
    return (
      <div className="flex items-center justify-center py-24">
        <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
        <p className="text-red-400">{error}</p>
        <button
          type="button"
          onClick={onRefetch}
          className="mt-4 inline-block rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
        >
          Reintentar
        </button>
      </div>
    );
  }

  if (vehiculos.length === 0) {
    return (
      <div className="rounded-2xl border border-white/10 bg-[#13161d] p-12 text-center">
        <FaCar className="mx-auto text-5xl text-gray-600" />
        <h3 className="mt-4 text-lg font-semibold">
          No encontramos esos vehículos
        </h3>
        <p className="mt-2 text-sm text-[#9aa1b1]">
          Es posible que uno de los anuncios ya no esté publicado.
        </p>
        <button
          type="button"
          onClick={onLimpiar}
          className="mt-6 inline-flex items-center gap-2 rounded-xl bg-blue-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600"
        >
          Limpiar selección
        </button>
      </div>
    );
  }

  return (
    <>
      {/* BARRA DE SELECCIÓN */}
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-[#9aa1b1]">
          {seleccionados.length} de {MAX_VEHICULOS} vehículos seleccionados
        </p>
        <button
          type="button"
          onClick={onLimpiar}
          className="text-xs font-medium text-[#9aa1b1] underline-offset-2 transition-colors hover:text-white hover:underline"
        >
          Limpiar todo
        </button>
      </div>

      {/* VISTA ESCRITORIO: TABLA */}
      <div className="hidden overflow-x-auto rounded-2xl border border-white/10 bg-[#13161d] md:block">
        <table className="w-full min-w-[760px] border-collapse">
          <thead>
            <tr className="border-b border-white/10">
              <th className="w-44 px-6 py-5 text-left align-bottom text-sm font-semibold text-[#9aa1b1]">
                Vehículo
              </th>
              {vehiculos.map((v) => (
                <th key={v.id} className="min-w-[220px] px-6 py-5 align-bottom">
                  <Link to={`/anuncio/${v.id}`} className="group block">
                    <div className="relative aspect-[16/10] overflow-hidden rounded-xl">
                      <img
                        src={fotoPrincipal(v)}
                        alt={`${v.marca} ${v.modelo}`}
                        className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                      />
                      {v.enOferta && (
                        <span className="absolute left-2 top-2 rounded-full bg-green-500/90 px-2.5 py-1 text-xs font-bold text-white">
                          Oferta
                        </span>
                      )}
                    </div>
                    <div className="mt-3 text-base font-bold text-white group-hover:text-blue-400">
                      {v.marca} {v.modelo}
                    </div>
                    {v.version && (
                      <div className="text-sm text-[#9aa1b1]">{v.version}</div>
                    )}
                    <div
                      className={`mt-1 text-sm ${
                        v.condicion === "Nuevo" ? "text-blue-400" : "text-gray-300"
                      }`}
                    >
                      {v.condicion === "Nuevo" ? "Nuevo" : "Usado"} · {v.anio}
                    </div>
                  </Link>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filaComparativa(
              "Precio",
              (v) => (
                <div>
                  <div className="text-base font-bold text-blue-500">
                    {formatearPrecio(v.precio, v.moneda)}
                  </div>
                  {v.enOferta && v.precioAnterior != null && (
                    <div className="text-xs text-[#9aa1b1] line-through">
                      {formatearPrecio(v.precioAnterior, v.moneda)}
                    </div>
                  )}
                </div>
              ),
              (v) => v.precio === menorPrecio,
            )}
            {filaComparativa("Kilometraje", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaTachometerAlt className="text-gray-500" />
                {v.kilometraje.toLocaleString("es-DO")} km
              </span>
            ))}
            {filaComparativa("Año", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaCalendarAlt className="text-gray-500" />
                {v.anio}
              </span>
            ))}
            {filaComparativa("Tipo", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaCar className="text-gray-500" />
                {etiquetaDe(v.tipoVehiculo, TIPOS_VEHICULO)}
              </span>
            ))}
            {filaComparativa("Motor", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaCogs className="text-gray-500" />
                {etiquetaLegible(v.motor)}
              </span>
            ))}
            {filaComparativa("Transmisión", (v) =>
              etiquetaDe(v.transmision, TRANSMISIONES),
            )}
            {filaComparativa("Tracción", (v) => etiquetaLegible(v.traccion))}
            {filaComparativa("Combustible", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaOilCan className="text-gray-500" />
                {etiquetaDe(v.combustible, COMBUSTIBLES)}
              </span>
            ))}
            {filaComparativa("Color exterior", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaPalette className="text-gray-500" />
                {etiquetaLegible(v.colorExterior)}
              </span>
            ))}
            {filaComparativa("Color interior", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaTint className="text-gray-500" />
                {etiquetaLegible(v.colorInterior)}
              </span>
            ))}
            {filaComparativa("Ubicación", (v) => (
              <span className="inline-flex items-center gap-1.5">
                <FaMapMarkerAlt className="text-gray-500" />
                {etiquetaLegible(v.ubicacion)}
              </span>
            ))}
          </tbody>
        </table>
      </div>

      {/* VISTA MÓVIL: TARJETAS */}
      <div className="grid grid-cols-1 gap-5 md:hidden">
        {vehiculos.map((v) => (
          <div
            key={v.id}
            className="overflow-hidden rounded-2xl border border-white/10 bg-[#13161d]"
          >
            <div className="relative aspect-[16/10]">
              <img
                src={fotoPrincipal(v)}
                alt={`${v.marca} ${v.modelo}`}
                className="h-full w-full object-cover"
              />
              {v.enOferta && (
                <span className="absolute left-2 top-2 rounded-full bg-green-500/90 px-2.5 py-1 text-xs font-bold text-white">
                  Oferta
                </span>
              )}
              <button
                type="button"
                onClick={() => onRemover(v.id)}
                className="absolute right-2 top-2 inline-flex items-center gap-1.5 rounded-lg bg-[#0c101b]/90 px-3 py-2 text-xs font-semibold text-[#9aa1b1] transition-colors hover:text-red-400"
              >
                <FaMinus />
                Quitar
              </button>
            </div>

            <div className="p-5">
              <Link to={`/anuncio/${v.id}`} className="block">
                <h3 className="text-lg font-bold text-white hover:text-blue-400">
                  {v.marca} {v.modelo}
                  {v.version && (
                    <span className="ml-2 text-sm font-normal text-[#9aa1b1]">
                      {v.version}
                    </span>
                  )}
                </h3>
              </Link>

              <div className="mt-3 divide-y divide-white/5">
                {especificacionesDe(v, menorPrecio).map((esp) => (
                  <div
                    key={esp.etiqueta}
                    className="flex items-center justify-between gap-4 py-2.5 text-sm"
                  >
                    <span className="inline-flex items-center gap-2 text-[#9aa1b1]">
                      {esp.icono}
                      {esp.etiqueta}
                    </span>
                    <span
                      className={`text-right ${
                        esp.destacado
                          ? "font-semibold text-green-400"
                          : "text-gray-200"
                      }`}
                    >
                      {esp.valor}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </>
  );
}

function especificacionesDe(
  v: VehiculoComparador,
  menorPrecio: number | null,
): Especificaciones[] {
  return [
    {
      etiqueta: "Precio",
      icono: <FaBalanceScale className="text-gray-500" />,
      valor: formatearPrecio(v.precio, v.moneda),
      destacado: v.precio === menorPrecio,
    },
    {
      etiqueta: "Kilometraje",
      icono: <FaTachometerAlt className="text-gray-500" />,
      valor: `${v.kilometraje.toLocaleString("es-DO")} km`,
    },
    { etiqueta: "Año", icono: <FaCalendarAlt className="text-gray-500" />, valor: String(v.anio) },
    {
      etiqueta: "Tipo",
      icono: <FaCar className="text-gray-500" />,
      valor: etiquetaDe(v.tipoVehiculo, TIPOS_VEHICULO),
    },
    {
      etiqueta: "Motor",
      icono: <FaCogs className="text-gray-500" />,
      valor: etiquetaLegible(v.motor),
    },
    {
      etiqueta: "Transmisión",
      icono: <FaCogs className="text-gray-500" />,
      valor: etiquetaDe(v.transmision, TRANSMISIONES),
    },
    { etiqueta: "Tracción", icono: <FaCar className="text-gray-500" />, valor: etiquetaLegible(v.traccion) },
    {
      etiqueta: "Combustible",
      icono: <FaOilCan className="text-gray-500" />,
      valor: etiquetaDe(v.combustible, COMBUSTIBLES),
    },
    {
      etiqueta: "Color exterior",
      icono: <FaPalette className="text-gray-500" />,
      valor: etiquetaLegible(v.colorExterior),
    },
    {
      etiqueta: "Color interior",
      icono: <FaTint className="text-gray-500" />,
      valor: etiquetaLegible(v.colorInterior),
    },
    {
      etiqueta: "Ubicación",
      icono: <FaMapMarkerAlt className="text-gray-500" />,
      valor: etiquetaLegible(v.ubicacion),
    },
  ];
}

function etiquetaLegible(valor: string | null | undefined): string {
  if (!valor) return "—";
  const palabras = valor.replace(/_/g, " ").toLowerCase().split(" ");
  return palabras.map((p) => p.charAt(0).toUpperCase() + p.slice(1)).join(" ");
}

export default function Comparador() {
  const [searchParams] = useSearchParams();
  const idsParam: number[] = [];
  for (const valor of searchParams.getAll("ids")) {
    const numero = Number(valor);
    if (Number.isFinite(numero)) idsParam.push(numero);
  }

  const { seleccionados, esSeleccionado, toggle, reemplazar, limpiar } =
    useComparador();

  const ultimaClave = useRef("");

  // Al montar, si la URL trae ids y difieren de la selección guardada, se adoptan.
  useEffect(() => {
    const claveActual = Array.from(new Set(idsParam)).sort().join(",");
    if (claveActual && claveActual !== ultimaClave.current) {
      ultimaClave.current = claveActual;
      reemplazar(idsParam);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const claveSeleccion = Array.from(new Set(seleccionados)).sort().join(",");
  const activos = claveSeleccion
    .split(",")
    .map(Number)
    .filter(Number.isFinite);

  const {
    data: vehiculos = [],
    isLoading: cargando,
    isError,
    refetch,
  } = useCompararVehiculos(activos);
  const error = isError ? "No pudimos cargar los vehículos para comparar. Inténtalo de nuevo." : "";

  const [terminoBusqueda, setTerminoBusqueda] = useState("");
  const [resultados, setResultados] = useState<AnuncioListado[]>([]);
  const buscarMutation = useBuscarComparador();

  const buscar = async (e: FormEvent) => {
    e.preventDefault();
    if (buscarMutation.isPending) return;
    if (!terminoBusqueda.trim()) return;

    try {
      const items = await buscarMutation.mutateAsync(terminoBusqueda.trim());
      setResultados(items);
    } catch {
      setResultados([]);
    }
  };

  const agregarDesdeBusqueda = (anuncio: AnuncioListado) => {
    if (esSeleccionado(anuncio.id)) return;
    toggle(anuncio.id);
    setResultados([]);
    setTerminoBusqueda("");
  };

  const removerVehiculo = (id: number) => toggle(id);

  const menorPrecio =
    vehiculos.length > 0 ? Math.min(...vehiculos.map((v) => v.precio)) : null;

  const necesitaSeleccion = seleccionados.length < 2;

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      {/* HEADER */}
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link to="/" className="flex items-center gap-4">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-20 w-auto object-contain"
          />
        </Link>

        <MenuPublico />
      </header>

      {/* CABECERA */}
      <section className="border-b border-white/10 bg-gradient-to-b from-[#11161f] to-[#0c101b]">
        <div className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
          <div className="flex flex-wrap items-end justify-between gap-4">
            <div>
              <h1 className="text-3xl font-bold">Comparador de vehículos</h1>
              <p className="mt-2 text-sm text-[#9aa1b1]">
                {necesitaSeleccion
                  ? `Selecciona al menos 2 vehículos (máx. ${MAX_VEHICULOS}) para ver la comparación lado a lado.`
                  : `Comparando ${seleccionados.length} vehículo${seleccionados.length !== 1 ? "s" : ""} de ${MAX_VEHICULOS} máx.`}
              </p>
            </div>
            <Link
              to="/vehiculos"
              className="inline-flex items-center gap-2 rounded-xl border border-white/10 px-5 py-2.5 text-sm font-medium text-[#9aa1b1] transition-colors hover:border-white/30 hover:text-white"
            >
              <FaExchangeAlt className="text-gray-500" />
              Ver directorio
            </Link>
          </div>

          {/* BÚSQUEDA EN EL COMPARADOR */}
          <BuscadorVehiculos
            termino={terminoBusqueda}
            pendiente={buscarMutation.isPending}
            resultados={resultados}
            esSeleccionado={esSeleccionado}
            onChangeTermino={setTerminoBusqueda}
            onBuscar={buscar}
            onAgregar={agregarDesdeBusqueda}
          />
        </div>
      </section>

      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <ContenidoComparador
          vehiculos={vehiculos}
          seleccionados={seleccionados}
          cargando={cargando}
          error={error}
          necesitaSeleccion={necesitaSeleccion}
          menorPrecio={menorPrecio}
          onRefetch={refetch}
          onLimpiar={limpiar}
          onRemover={removerVehiculo}
        />
      </main>

      {/* FOOTER */}
      <footer className="border-t border-white/10 py-8">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-6 text-sm text-[#9aa1b1] sm:flex-row sm:px-8">
          <span>© 2026 AutoMarket RD. Todos los derechos reservados.</span>
          <Link to="/precios" className="transition-colors hover:text-white">
            Planes y precios
          </Link>
        </div>
      </footer>
    </div>
  );
}
