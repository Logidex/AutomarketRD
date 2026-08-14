import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  FaCar,
  FaCalendarAlt,
  FaMapMarkerAlt,
  FaSearch,
  FaTachometerAlt,
  FaStar,
  FaCrown,
  FaGem,
} from "react-icons/fa";
import { useVehiculos } from "../hooks/useVehiculos";
import type { AnuncioListado } from "../types/anuncio.types";
import logo from "../assets/AutoMarketRD_Logo.svg";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";
import MenuPublico from "../components/layout/MenuPublico";
import { anuncioService } from "../services/anuncio.service";
import { useQuery } from "@tanstack/react-query";
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from "../constants/vehiculo.opciones";
import { nombrePlan } from "../constants/planes";

const TAMANO_PAGINA = 12;

type PlanNivel = "Gratis" | "Basico" | "Pro" | "Elite";

const COLOR_PLAN: Record<PlanNivel, string> = {
  Elite: "bg-gradient-to-r from-amber-500 to-yellow-600 text-white",
  Pro: "bg-gradient-to-r from-purple-500 to-pink-500 text-white",
  Basico: "bg-gradient-to-r from-blue-500 to-cyan-500 text-white",
  Gratis: "bg-gradient-to-r from-gray-500 to-gray-600 text-white",
};

const ICONO_PLAN: Record<PlanNivel, React.ReactNode> = {
  Elite: <FaCrown className="w-3 h-3" />,
  Pro: <FaGem className="w-3 h-3" />,
  Basico: <FaStar className="w-3 h-3" />,
  Gratis: <span className="text-xs font-bold">F</span>,
};

function BadgePlan({ nivel, className = "" }: { nivel: PlanNivel; className?: string }) {
  return (
    <span className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-semibold ${COLOR_PLAN[nivel]} ${className}`}>
      {ICONO_PLAN[nivel]}
      {nombrePlan(nivel)}
    </span>
  );
}

interface Filtros {
  marca: string;
  tipoVehiculo: string;
  transmision: string;
  combustible: string;
  precioMinimo: string;
  precioMaximo: string;
}

const FILTROS_INICIALES: Filtros = {
  marca: "",
  tipoVehiculo: "",
  transmision: "",
  combustible: "",
  precioMinimo: "",
  precioMaximo: "",
};

const fotoPrincipal = (anuncio: AnuncioListado): string =>
  urlImagen(anuncio.fotos?.[0]) || "https://via.placeholder.com/600x400?text=Sin+Foto";

function useBusquedaVehiculos() {
  const [pagina, setPagina] = useState(1);
  const [filtros, setFiltros] = useState<Filtros>(FILTROS_INICIALES);
  const [filtrosAplicados, setFiltrosAplicados] = useState<Filtros>(FILTROS_INICIALES);

  const {
    data,
    isLoading,
    isError,
    error,
    isFetching,
    refetch,
  } = useVehiculos({
    filtros: {
      marca: filtrosAplicados.marca || undefined,
      tipoVehiculo: filtrosAplicados.tipoVehiculo || undefined,
      transmision: filtrosAplicados.transmision || undefined,
      combustible: filtrosAplicados.combustible || undefined,
      precioMinimo: filtrosAplicados.precioMinimo ? Number(filtrosAplicados.precioMinimo) : undefined,
      precioMaximo: filtrosAplicados.precioMaximo ? Number(filtrosAplicados.precioMaximo) : undefined,
    },
    pagina,
  });

  const anuncios = data?.items ?? [];
  const totalRegistros = data?.totalRegistros ?? 0;
  const cantidadPorPagina = data?.cantidadPorPagina ?? TAMANO_PAGINA;
  const totalPaginas = cantidadPorPagina > 0
    ? Math.ceil(totalRegistros / cantidadPorPagina)
    : 1;

  const cargando = isLoading || isFetching;

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value } = e.target;
    setFiltros((prev) => ({ ...prev, [name]: value }));
  };

  const aplicarBusqueda = (e: React.FormEvent) => {
    e.preventDefault();
    setPagina(1);
    setFiltrosAplicados({ ...filtros });
  };

  const limpiarFiltros = () => {
    setFiltros(FILTROS_INICIALES);
    setFiltrosAplicados({ ...FILTROS_INICIALES });
    setPagina(1);
  };

  const irAPagina = (p: number) => {
    if (p < 1 || p > totalPaginas || p === pagina) return;
    window.scrollTo({ top: 0, behavior: "smooth" });
    setPagina(p);
  };

  return {
    filtros,
    pagina,
    cargando,
    anuncios,
    totalRegistros,
    totalPaginas,
    isLoading,
    isError,
    error,
    refetch,
    handleChange,
    aplicarBusqueda,
    limpiarFiltros,
    irAPagina,
  };
}

interface PropsHero {
  filtros: Filtros;
  cargando: boolean;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  onSubmit: (e: React.FormEvent) => void;
}

function HeroBusqueda({ filtros, cargando, onChange, onSubmit }: PropsHero) {
  return (
    <section className="border-b border-white/10 bg-gradient-to-b from-[#11161f] to-[#0c101b]">
      <div className="mx-auto max-w-6xl px-6 py-14 text-center sm:px-8">
        <h1 className="text-4xl font-bold sm:text-5xl">
          Compra y vende vehículos
          <span className="block text-blue-500">en República Dominicana</span>
        </h1>
        <p className="mx-auto mt-4 max-w-2xl text-[#9aa1b1]">
          Explora el inventario de agencias y vendedores particulares. Encuentra
          el vehículo que buscas y contacta al vendedor directamente.
        </p>

        <form onSubmit={onSubmit} className="mx-auto mt-10 max-w-5xl">
          <div className="flex flex-col gap-3 rounded-2xl border border-white/10 bg-[#13161d] p-4 sm:flex-row">
            <div className="relative flex-1">
              <FaSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-gray-500" />
              <label
                htmlFor="buscarMarca"
                className="sr-only"
              >
                Buscar por marca o modelo
              </label>
              <input
                id="buscarMarca"
                name="marca"
                type="text"
                value={filtros.marca}
                onChange={onChange}
                placeholder="Buscar por marca o modelo..."
                className="w-full rounded-xl border border-white/10 bg-[#0c101b] py-3 pl-12 pr-4 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>

            <button
              type="submit"
              disabled={cargando}
              className="rounded-xl bg-blue-500 px-8 py-3 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed"
            >
              {cargando ? "Buscando..." : "Buscar"}
            </button>
          </div>

          <div className="mt-4 grid grid-cols-2 gap-3 md:grid-cols-4">
            <label htmlFor="homeFiltroTipoVehiculo" className="sr-only">
              Tipo de vehículo
            </label>
            <select
              id="homeFiltroTipoVehiculo"
              name="tipoVehiculo"
              value={filtros.tipoVehiculo}
              onChange={onChange}
              className="rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
            >
              <option value="">Tipo de vehículo</option>
              {TIPOS_VEHICULO.map((opcion) => (
                <option key={opcion.valor} value={opcion.valor}>
                  {opcion.etiqueta}
                </option>
              ))}
            </select>

            <label htmlFor="homeFiltroTransmision" className="sr-only">
              Transmisión
            </label>
            <select
              id="homeFiltroTransmision"
              name="transmision"
              value={filtros.transmision}
              onChange={onChange}
              className="rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
            >
              <option value="">Transmisión</option>
              {TRANSMISIONES.map((opcion) => (
                <option key={opcion.valor} value={opcion.valor}>
                  {opcion.etiqueta}
                </option>
              ))}
            </select>

            <label htmlFor="homeFiltroCombustible" className="sr-only">
              Combustible
            </label>
            <select
              id="homeFiltroCombustible"
              name="combustible"
              value={filtros.combustible}
              onChange={onChange}
              className="rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
            >
              <option value="">Combustible</option>
              {COMBUSTIBLES.map((opcion) => (
                <option key={opcion.valor} value={opcion.valor}>
                  {opcion.etiqueta}
                </option>
              ))}
            </select>

            <div className="flex items-center gap-2">
              <label
                htmlFor="precioMinimo"
                className="sr-only"
              >
                Precio mínimo
              </label>
              <input
                id="precioMinimo"
                name="precioMinimo"
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                value={filtros.precioMinimo}
                onChange={onChange}
                placeholder="Precio mín."
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
              <span className="text-gray-500">-</span>
              <label
                htmlFor="precioMaximo"
                className="sr-only"
              >
                Precio máximo
              </label>
              <input
                id="precioMaximo"
                name="precioMaximo"
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                value={filtros.precioMaximo}
                onChange={onChange}
                placeholder="Precio máx."
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>
          </div>
        </form>
      </div>
    </section>
  );
}

interface PropsTarjeta {
  anuncio: AnuncioListado;
  onAbrir: () => void;
}

function TarjetaAnuncioHome({ anuncio, onAbrir }: PropsTarjeta) {
  const planNivel = (anuncio.badgeSuscripcion as PlanNivel) ?? "Gratis";

  return (
    <button
      type="button"
      onClick={onAbrir}
      className="group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] text-left transition-[border-color,box-shadow] hover:border-blue-500/40 hover:shadow-lg"
    >
      <div className="relative aspect-[16/10] overflow-hidden">
        <img
          src={fotoPrincipal(anuncio)}
          alt={`${anuncio.marca} ${anuncio.modelo}`}
          className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
        <BadgePlan nivel={planNivel} className="absolute top-3 right-3 z-10" />
        {anuncio.esDestacado && anuncio.fechaDestacadoHasta && new Date(anuncio.fechaDestacadoHasta) > new Date() && (
          <div className="absolute left-3 top-3 z-10">
            <span className="inline-flex items-center gap-1 rounded-full bg-gradient-to-r from-amber-500 to-yellow-600 px-2.5 py-1 text-xs font-bold text-white">
              <FaStar className="w-3 h-3" /> Destacado
            </span>
          </div>
        )}
      </div>

      <div className="flex flex-1 flex-col p-5">
        <div className="flex items-start justify-between gap-3">
          <h3 className="truncate text-lg font-bold">
            {anuncio.marca} {anuncio.modelo}
            <span className="ml-2 text-sm font-normal text-gray-400">
              {anuncio.version}
            </span>
          </h3>
          <span className="shrink-0 rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
            {anuncio.anio}
          </span>
        </div>

        <p className="mt-2 text-xl font-bold text-blue-500">
          {formatearPrecio(anuncio.precio, anuncio.moneda)}
        </p>

        <div className="mt-4 flex flex-wrap gap-x-4 gap-y-2 border-t border-white/10 pt-4 text-xs text-[#9aa1b1]">
          <span className="inline-flex items-center gap-1.5">
            <FaTachometerAlt className="text-gray-500" />
            {anuncio.kilometraje.toLocaleString("es-DO")} km
          </span>
          <span className="inline-flex items-center gap-1.5">
            <FaCalendarAlt className="text-gray-500" />
            {anuncio.anio}
          </span>
          {anuncio.ubicacion && (
            <span className="inline-flex items-center gap-1.5">
              <FaMapMarkerAlt className="text-gray-500" />
              {anuncio.ubicacion}
            </span>
          )}
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          {anuncio.tipoVehiculo && (
            <span className="rounded-full bg-blue-500/10 px-3 py-1 text-xs font-medium text-blue-400">
              {etiquetaDe(anuncio.tipoVehiculo, TIPOS_VEHICULO)}
            </span>
          )}
          {anuncio.combustible && (
            <span className="rounded-full bg-white/5 px-3 py-1 text-xs font-medium text-gray-300">
              {etiquetaDe(anuncio.combustible, COMBUSTIBLES)}
            </span>
          )}
          {anuncio.transmision && (
            <span className="rounded-full bg-white/5 px-3 py-1 text-xs font-medium text-gray-300">
              {etiquetaDe(anuncio.transmision, TRANSMISIONES)}
            </span>
          )}
        </div>
      </div>
    </button>
  );
}

interface PropsPaginacion {
  pagina: number;
  totalPaginas: number;
  cargando: boolean;
  isError: boolean;
  onIrAPagina: (p: number) => void;
}

function PaginacionHome({
  pagina,
  totalPaginas,
  cargando,
  isError,
  onIrAPagina,
}: PropsPaginacion) {
  if (cargando || isError || totalPaginas <= 1) return null;

  return (
    <div className="mt-10 flex flex-wrap items-center justify-center gap-2">
      <button
        type="button"
        onClick={() => onIrAPagina(pagina - 1)}
        disabled={pagina <= 1}
        className="rounded-lg border border-white/10 px-4 py-2 text-sm font-medium transition-colors hover:border-white/30 disabled:cursor-not-allowed disabled:opacity-40"
      >
        Anterior
      </button>

      {(() => {
        const paginas: number[] = [];
        for (let p = 1; p <= totalPaginas; p++) {
          if (
            totalPaginas > 7 &&
            p !== 1 &&
            p !== totalPaginas &&
            Math.abs(p - pagina) > 2
          )
            continue;
          paginas.push(p);
        }
        return paginas.map((p) => (
          <button
            key={p}
            type="button"
            onClick={() => onIrAPagina(p)}
            className={`min-w-10 rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
              p === pagina
                ? "bg-blue-500 text-white"
                : "border border-white/10 hover:border-white/30"
            }`}
          >
            {p}
          </button>
        ));
      })()}

      <button
        type="button"
        onClick={() => onIrAPagina(pagina + 1)}
        disabled={pagina >= totalPaginas}
        className="rounded-lg border border-white/10 px-4 py-2 text-sm font-medium transition-colors hover:border-white/30 disabled:cursor-not-allowed disabled:opacity-40"
      >
        Siguiente
      </button>
    </div>
  );
}

function PiePaginaHome() {
  return (
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
  );
}

export default function Home() {
  const navigate = useNavigate();

  const {
    filtros,
    pagina,
    cargando,
    anuncios,
    totalRegistros,
    totalPaginas,
    isLoading,
    isError,
    error,
    refetch,
    handleChange,
    aplicarBusqueda,
    limpiarFiltros,
    irAPagina,
  } = useBusquedaVehiculos();

  // Anuncios destacados (desde endpoint del backend con cuotas por plan)
  const { data: destacadosData, isLoading: destacadosCargando } = useQuery({
    queryKey: ["anuncios-destacados"],
    queryFn: () => anuncioService.obtenerDestacados(1, 6),
    staleTime: 1000 * 60 * 10,
  });

  const anunciosDestacados = destacadosData?.items ?? [];

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      {/* HEADER */}
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link to="/" className="flex items-center gap-4">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-24 w-auto object-contain drop-shadow-[0_0_20px_rgba(59,130,246,0.4)]"
          />
        </Link>

        <MenuPublico />
      </header>

      {/* HERO + BÚSQUEDA */}
      <HeroBusqueda
        filtros={filtros}
        cargando={cargando}
        onChange={handleChange}
        onSubmit={aplicarBusqueda}
      />

      {/* DESTACADOS POR SUSCRIPCIÓN */}
      {anunciosDestacados.length > 0 && (
        <section className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
          <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <FaCrown className="text-amber-400" />
              <h2 className="text-xl font-bold">Destacados <span className="text-amber-400">Premium</span></h2>
            </div>
            <span className="text-xs text-[#9aa1b1]">Ordenados por plan: Elite → Pro → Básico → Gratis</span>
          </div>

          {destacadosCargando ? (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {[...Array(6)].map((_, i) => (
                <div key={i} className="animate-pulse">
                  <div className="aspect-[16/10] rounded-2xl bg-[#13161d]" />
                  <div className="mt-4 h-4 bg-[#13161d] rounded w-3/4" />
                  <div className="mt-2 h-4 bg-[#13161d] rounded w-1/2" />
                  <div className="mt-4 h-3 bg-[#13161d] rounded w-full" />
                </div>
              ))}
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {anunciosDestacados.map((anuncio) => (
                <TarjetaAnuncioHome
                  key={anuncio.id}
                  anuncio={anuncio}
                  onAbrir={() => navigate(`/anuncio/${anuncio.id}`)}
                />
              ))}
            </div>
          )}
        </section>
      )}

      {/* VITRINA */}
      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <h2 className="text-xl font-bold">
            Vehículos disponibles
            {totalRegistros > 0 && (
              <span className="ml-2 text-sm font-normal text-[#9aa1b1]">
                ({totalRegistros})
              </span>
            )}
          </h2>

          <button
            type="button"
            onClick={limpiarFiltros}
            disabled={cargando}
            className="rounded-lg border border-white/10 px-4 py-2 text-sm font-medium text-[#9aa1b1] transition-colors hover:border-white/30 hover:text-white disabled:opacity-50"
          >
            Limpiar filtros
          </button>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center py-24">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
          </div>
        ) : isError ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-400">
              {error instanceof Error ? error.message : "No se pudieron cargar los vehículos. Inténtalo nuevamente."}
            </p>
            <button
              type="button"
              onClick={() => refetch()}
              className="mt-4 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
            >
              Reintentar
            </button>
          </div>
        ) : anuncios.length === 0 ? (
          <div className="rounded-2xl border border-white/10 bg-[#13161d] p-16 text-center">
            <FaCar className="mx-auto text-5xl text-gray-600" />
            <h3 className="mt-4 text-lg font-semibold">
              No encontramos vehículos
            </h3>
            <p className="mt-2 text-sm text-[#9aa1b1]">
              Prueba ajustando o limpiando los filtros de búsqueda.
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {anuncios.map((anuncio) => (
              <TarjetaAnuncioHome
                key={anuncio.id}
                anuncio={anuncio}
                onAbrir={() => navigate(`/anuncio/${anuncio.id}`)}
              />
            ))}
          </div>
        )}

        {/* PAGINACIÓN */}
        <PaginacionHome
          pagina={pagina}
          totalPaginas={totalPaginas}
          cargando={cargando}
          isError={isError}
          onIrAPagina={irAPagina}
        />
      </main>

      {/* FOOTER */}
      <PiePaginaHome />
    </div>
  );
}