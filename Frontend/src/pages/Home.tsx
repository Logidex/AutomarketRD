// Ubicación: src/pages/Home.tsx
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
  FaArrowRight,
  FaCheckCircle,
} from "react-icons/fa";
import { useVehiculos } from "../hooks/useVehiculos";
import type { AnuncioListado } from "../types/anuncio.types";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";
import { anuncioService } from "../services/anuncio.service";
import { catalogoService } from "../services/catalogo.service";
import { useQuery } from "@tanstack/react-query";
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from "../constants/vehiculo.opciones";
import { nombrePlan } from "../constants/planes";
import BadgeVerificado from "../components/BadgeVerificado";
import FiltroTipoVehiculo from "../components/FiltroTipoVehiculo";
import HeaderPublico from "../components/layout/HeaderPublico";

const TAMANO_PAGINA = 12;

type PlanNivel = "Gratis" | "Basico" | "Pro" | "Elite";

/**
 * Solo Elite lleva el gradiente dorado (elemento distintivo).
 * El resto usa chips discretos basados en tokens para no competir
 * con la foto del vehículo ni con el precio de marca.
 */
const COLOR_PLAN: Record<PlanNivel, string> = {
  Elite: "bg-gradient-to-r from-amber-500 to-yellow-600 text-white shadow-sm",
  Pro: "bg-brand-soft text-brand border border-brand/20",
  Basico: "bg-surface-2 text-ink-2 border border-line",
  Gratis: "bg-surface-2 text-ink-3 border border-line",
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
  busqueda: string;
  tipoVehiculo: string;
  transmision: string;
  combustible: string;
  precioMinimo: string;
  precioMaximo: string;
}

const FILTROS_INICIALES: Filtros = {
  busqueda: "",
  tipoVehiculo: "",
  transmision: "",
  combustible: "",
  precioMinimo: "",
  precioMaximo: "",
};

const fotoPrincipal = (anuncio: AnuncioListado): string =>
  urlImagen(anuncio.fotos?.[0]) || "https://via.placeholder.com/600x400?text=Sin+Foto";

function useBusquedaVehiculos() {
  const navigate = useNavigate();
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
      busqueda: filtrosAplicados.busqueda || undefined,
      tipoVehiculo: filtrosAplicados.tipoVehiculo || undefined,
      transmision: filtrosAplicados.transmision || undefined,
      combustible: filtrosAplicados.combustible || undefined,
      precioMinimo: filtrosAplicados.precioMinimo ? Number(filtrosAplicados.precioMinimo) : undefined,
      precioMaximo: filtrosAplicados.precioMaximo ? Number(filtrosAplicados.precioMaximo) : undefined,
      excluirDestacados: true,
    },
    pagina,
  });

  const { data: datosTotales } = useQuery({
    queryKey: ["total-anuncios-publicados"],
    queryFn: () => catalogoService.buscar({ paginaActual: 1, cantidadAnuncios: 1 }),
    staleTime: 1000 * 60 * 5,
  });
  const totalPublicados = datosTotales?.totalRegistros ?? 0;

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
    const params = new URLSearchParams();
    if (filtros.busqueda.trim()) params.set("busqueda", filtros.busqueda.trim());
    if (filtros.tipoVehiculo) params.set("tipo", filtros.tipoVehiculo);
    if (filtros.transmision) params.set("transmision", filtros.transmision);
    if (filtros.combustible) params.set("combustible", filtros.combustible);
    if (filtros.precioMinimo) params.set("precioMinimo", filtros.precioMinimo);
    if (filtros.precioMaximo) params.set("precioMaximo", filtros.precioMaximo);
    navigate(`/vehiculos${params.toString() ? `?${params.toString()}` : ""}`);
  };

  const limpiarFiltros = () => {
    setFiltros(FILTROS_INICIALES);
    setFiltrosAplicados({ ...FILTROS_INICIALES });
    setPagina(1);
  };

  const seleccionarTipo = (valor: string) => {
    setFiltros((prev) => ({ ...prev, tipoVehiculo: valor }));
    setPagina(1);
    setFiltrosAplicados((prev) => ({ ...prev, tipoVehiculo: valor }));
    navigate(valor ? `/vehiculos?tipo=${encodeURIComponent(valor)}` : "/vehiculos");
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
    totalPublicados,
    totalPaginas,
    isLoading,
    isError,
    error,
    refetch,
    handleChange,
    aplicarBusqueda,
    limpiarFiltros,
    seleccionarTipo,
    irAPagina,
  };
}

interface PropsHero {
  filtros: Filtros;
  cargando: boolean;
  totalPublicados: number;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  onSubmit: (e: React.FormEvent) => void;
  onSeleccionarTipo: (valor: string) => void;
}

function HeroBusqueda({ filtros, cargando, totalPublicados, onChange, onSubmit, onSeleccionarTipo }: PropsHero) {
  return (
    <section className="border-b border-line bg-gradient-to-b from-surface-2 to-page">
      <div className="mx-auto max-w-6xl px-6 py-14 text-center sm:px-8">
        <h1 className="text-balance text-4xl font-bold sm:text-5xl">
          Compra y vende vehículos
          <span className="block text-brand">en República Dominicana</span>
        </h1>
        <p className="mx-auto mt-4 max-w-2xl text-pretty text-ink-2">
          Explora el inventario de agencias y vendedores particulares. Encuentra
          el vehículo que buscas y contacta al vendedor directamente.
        </p>

        {/* Métricas reales (solo se muestran si hay datos) */}
        {totalPublicados > 0 && (
          <div className="mt-6 flex flex-wrap items-center justify-center gap-x-8 gap-y-2 text-sm text-ink-2">
            <span className="inline-flex items-center gap-2">
              <FaCar className="text-brand" />
              <strong className="font-semibold text-ink">
                {totalPublicados.toLocaleString("es-DO")}
              </strong>{" "}
              vehículos publicados
            </span>
            <span className="inline-flex items-center gap-2">
              <FaCheckCircle className="text-success" />
              Vendedores verificados
            </span>
          </div>
        )}

        <div className="mt-10">
          <p className="mb-4 text-sm font-semibold uppercase tracking-wide text-ink-2">
            ¿Qué tipo de vehículo buscas?
          </p>
          <FiltroTipoVehiculo
            valor={filtros.tipoVehiculo}
            onSeleccionar={onSeleccionarTipo}
          />
        </div>

        {/* Buscador protagónico: elevado con sombra y foco de marca */}
        <form onSubmit={onSubmit} className="mx-auto mt-8 max-w-5xl">
          <div className="flex flex-col gap-3 rounded-2xl border border-line bg-surface p-4 shadow-lg shadow-black/5 sm:flex-row">
            <div className="relative flex-1">
              <FaSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-ink-3" />
              <label htmlFor="buscarMarca" className="sr-only">
                Buscar por marca o modelo
              </label>
              <input
                id="buscarMarca"
                name="busqueda"
                type="text"
                value={filtros.busqueda}
                onChange={onChange}
                placeholder="Buscar por marca o modelo..."
                className="w-full rounded-xl border border-line bg-page py-3 pl-12 pr-4 text-sm text-ink placeholder:text-ink-3 transition-colors focus:border-brand focus:outline-none"
              />
            </div>

            <button
              type="submit"
              disabled={cargando}
              className="rounded-xl bg-brand px-8 py-3 text-sm font-semibold text-white transition-colors hover:bg-brand-hover disabled:cursor-not-allowed disabled:opacity-60"
            >
              {cargando ? "Buscando..." : "Buscar"}
            </button>
          </div>

          <div className="mt-4 grid grid-cols-2 gap-3 md:grid-cols-3">
            <label htmlFor="homeFiltroTransmision" className="sr-only">
              Transmisión
            </label>
            <select
              id="homeFiltroTransmision"
              name="transmision"
              value={filtros.transmision}
              onChange={onChange}
              className="rounded-xl border border-line bg-surface px-4 py-3 text-sm text-ink transition-colors focus:border-brand focus:outline-none"
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
              className="rounded-xl border border-line bg-surface px-4 py-3 text-sm text-ink transition-colors focus:border-brand focus:outline-none"
            >
              <option value="">Combustible</option>
              {COMBUSTIBLES.map((opcion) => (
                <option key={opcion.valor} value={opcion.valor}>
                  {opcion.etiqueta}
                </option>
              ))}
            </select>

            <div className="flex items-center gap-2">
              <label htmlFor="precioMinimo" className="sr-only">
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
                className="w-full rounded-xl border border-line bg-surface px-4 py-3 text-sm text-ink placeholder:text-ink-3 transition-colors focus:border-brand focus:outline-none"
              />
              <span className="text-ink-3">-</span>
              <label htmlFor="precioMaximo" className="sr-only">
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
                className="w-full rounded-xl border border-line bg-surface px-4 py-3 text-sm text-ink placeholder:text-ink-3 transition-colors focus:border-brand focus:outline-none"
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
      className="group flex flex-col overflow-hidden rounded-2xl border border-line bg-surface text-left transition-[border-color,box-shadow] hover:border-brand/40 hover:shadow-lg"
    >
      <div className="relative aspect-[16/10] overflow-hidden">
        <img
          src={fotoPrincipal(anuncio)}
          alt={`${anuncio.marca} ${anuncio.modelo}`}
          className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
        <BadgePlan nivel={planNivel} className="absolute top-3 right-3 z-10" />
        {anuncio.esDealerVerificado && (
          <BadgeVerificado className="absolute left-3 bottom-3 z-10" />
        )}
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
          <h3 className="truncate text-lg font-bold text-ink">
            {anuncio.marca} {anuncio.modelo}
            <span className="ml-2 text-sm font-normal text-ink-3">
              {anuncio.version}
            </span>
          </h3>
          <span className="shrink-0 rounded-full bg-success-soft px-3 py-1 text-xs font-semibold text-success">
            {anuncio.anio}
          </span>
        </div>

        <p className="mt-2 text-xl font-bold text-brand">
          {formatearPrecio(anuncio.precio, anuncio.moneda)}
        </p>

        <div className="mt-4 flex flex-wrap gap-x-4 gap-y-2 border-t border-line pt-4 text-xs text-ink-2">
          <span className="inline-flex items-center gap-1.5">
            <FaTachometerAlt className="text-ink-3" />
            {anuncio.kilometraje.toLocaleString("es-DO")} km
          </span>
          <span className="inline-flex items-center gap-1.5">
            <FaCalendarAlt className="text-ink-3" />
            {anuncio.anio}
          </span>
          {anuncio.ubicacion && (
            <span className="inline-flex items-center gap-1.5">
              <FaMapMarkerAlt className="text-ink-3" />
              {anuncio.ubicacion}
            </span>
          )}
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          {anuncio.tipoVehiculo && (
            <span className="rounded-full bg-brand-soft px-3 py-1 text-xs font-medium text-brand">
              {etiquetaDe(anuncio.tipoVehiculo, TIPOS_VEHICULO)}
            </span>
          )}
          {anuncio.combustible && (
            <span className="rounded-full bg-surface-2 px-3 py-1 text-xs font-medium text-ink-2">
              {etiquetaDe(anuncio.combustible, COMBUSTIBLES)}
            </span>
          )}
          {anuncio.transmision && (
            <span className="rounded-full bg-surface-2 px-3 py-1 text-xs font-medium text-ink-2">
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
        className="rounded-lg border border-line px-4 py-2 text-sm font-medium transition-colors hover:border-ink-3/40 disabled:cursor-not-allowed disabled:opacity-40"
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
                ? "bg-brand text-white"
                : "border border-line hover:border-ink-3/40"
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
        className="rounded-lg border border-line px-4 py-2 text-sm font-medium transition-colors hover:border-ink-3/40 disabled:cursor-not-allowed disabled:opacity-40"
      >
        Siguiente
      </button>
    </div>
  );
}

/** NUEVO: bloque CTA para captar vendedores antes del footer */
function CtaVendedor() {
  return (
    <section className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
      <div className="flex flex-col items-start justify-between gap-6 rounded-2xl border border-brand/20 bg-brand-soft p-8 sm:flex-row sm:items-center">
        <div>
          <h2 className="text-2xl font-bold text-ink">
            ¿Quieres vender tu vehículo?
          </h2>
          <p className="mt-2 max-w-xl text-pretty text-ink-2">
            Publica tu anuncio en minutos y llega a miles de compradores en todo
            el país. Elige el plan que mejor se ajuste a ti.
          </p>
        </div>
        <Link
          to="/precios"
          className="inline-flex shrink-0 items-center gap-2 rounded-xl bg-brand px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-brand-hover"
        >
          Publicar vehículo
          <FaArrowRight className="h-3.5 w-3.5" />
        </Link>
      </div>
    </section>
  );
}

function PiePaginaHome() {
  return (
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
    totalPublicados,
    totalPaginas,
    isLoading,
    isError,
    error,
    refetch,
    handleChange,
    aplicarBusqueda,
    limpiarFiltros,
    seleccionarTipo,
    irAPagina,
  } = useBusquedaVehiculos();

  // Anuncios destacados (desde endpoint del backend con cuotas por plan)
  const {
    data: destacadosData,
    isLoading: destacadosCargando,
    isError: destacadosError,
    refetch: destacadosRefetch,
  } = useQuery({
    queryKey: ["anuncios-destacados"],
    queryFn: () => anuncioService.obtenerDestacados(1, 6),
    staleTime: 1000 * 60 * 10,
    retry: 1,
  });

  const anunciosDestacados = destacadosData?.items ?? [];

  return (
    <div className="min-h-screen bg-page text-ink">
      {/* HEADER unificado (mismo componente que LayoutPublico) */}
      <HeaderPublico />

      {/* HERO + BÚSQUEDA */}
      <HeroBusqueda
        filtros={filtros}
        cargando={cargando}
        totalPublicados={totalPublicados}
        onChange={handleChange}
        onSubmit={aplicarBusqueda}
        onSeleccionarTipo={seleccionarTipo}
      />

      {/* DESTACADOS POR SUSCRIPCIÓN */}
      <section className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <FaCrown className="text-amber-500" />
            <h2 className="text-xl font-bold text-ink">
              Destacados <span className="text-amber-500">Premium</span>
            </h2>
          </div>
        </div>

        {destacadosError ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-500">
              No se pudieron cargar los anuncios destacados. Inténtalo nuevamente.
            </p>
            <button
              type="button"
              onClick={() => destacadosRefetch()}
              className="mt-4 rounded-lg bg-brand px-6 py-2 text-sm font-semibold text-white transition-colors hover:bg-brand-hover"
            >
              Reintentar
            </button>
          </div>
        ) : destacadosCargando && anunciosDestacados.length === 0 ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="animate-pulse">
                <div className="aspect-[16/10] rounded-2xl bg-surface" />
                <div className="mt-4 h-4 bg-surface rounded w-3/4" />
                <div className="mt-2 h-4 bg-surface rounded w-1/2" />
                <div className="mt-4 h-3 bg-surface rounded w-full" />
              </div>
            ))}
          </div>
        ) : anunciosDestacados.length > 0 ? (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {anunciosDestacados.map((anuncio) => (
              <TarjetaAnuncioHome
                key={anuncio.id}
                anuncio={anuncio}
                onAbrir={() => navigate(`/anuncio/${anuncio.id}`)}
              />
            ))}
          </div>
        ) : (
          <p className="text-sm text-ink-2">
            Aún no hay anuncios destacados. Los anunciantes con planes Pro y Elite
            pueden destacar sus vehículos desde su panel.
          </p>
        )}
      </section>

      {/* VITRINA */}
      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <h2 className="text-xl font-bold text-ink">
            Vehículos disponibles
            {totalRegistros > 0 && (
              <span className="ml-2 text-sm font-normal text-ink-2">
                ({totalRegistros})
              </span>
            )}
          </h2>

          <button
            type="button"
            onClick={limpiarFiltros}
            disabled={cargando}
            className="rounded-lg border border-line px-4 py-2 text-sm font-medium text-ink-2 transition-colors hover:border-ink-3/40 hover:text-ink disabled:opacity-50"
          >
            Limpiar filtros
          </button>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center py-24">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-line border-t-brand" />
          </div>
        ) : isError ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-500">
              {error instanceof Error ? error.message : "No se pudieron cargar los vehículos. Inténtalo nuevamente."}
            </p>
            <button
              type="button"
              onClick={() => refetch()}
              className="mt-4 rounded-lg bg-brand px-6 py-2 text-sm font-semibold text-white transition-colors hover:bg-brand-hover"
            >
              Reintentar
            </button>
          </div>
        ) : anuncios.length === 0 ? (
          <div className="rounded-2xl border border-line bg-surface p-16 text-center">
            <FaCar className="mx-auto text-5xl text-ink-3" />
            <h3 className="mt-4 text-lg font-semibold text-ink">
              No encontramos vehículos
            </h3>
            <p className="mt-2 text-sm text-ink-2">
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

      {/* CTA VENDEDOR */}
      <CtaVendedor />

      {/* FOOTER */}
      <PiePaginaHome />
    </div>
  );
}