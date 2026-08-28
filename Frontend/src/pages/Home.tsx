import { useState, useCallback, useRef } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion } from "motion/react";
import {
  FaArrowRight,
  FaCar,
  FaChevronRight,
  FaMapMarkerAlt,
  FaSearch,
  FaTachometerAlt,
} from "react-icons/fa";
import { useQuery } from "@tanstack/react-query";
import type { AnuncioListado } from "../types/anuncio.types";
import { anuncioService } from "../services/anuncio.service";
import { catalogoService } from "../services/catalogo.service";
import { urlImagen } from "../utils/imagen";
import { urlAnuncio } from "../utils/slug";
import { formatearPrecio } from "../utils/formato";
import { COMBUSTIBLES, TRANSMISIONES } from "../constants/vehiculo.opciones";
import BadgeVerificado from "../components/BadgeVerificado";
import FiltroTipoVehiculo from "../components/FiltroTipoVehiculo";
import AdUnit from "../components/ads/AdUnit";
import HeaderPublico from "../components/layout/HeaderPublico";
import SectionBackground from "../components/SectionBackground";
import MeshGradientCanvas from "../components/MeshGradientCanvas";
import ShinyText from "../components/ShinyText";

const CANTIDAD_DESTACADOS = 5;
const CANTIDAD_RECIENTES = 6;

type FiltrosBusqueda = {
  busqueda: string;
  tipoVehiculo: string;
  transmision: string;
  combustible: string;
  precioMinimo: string;
  precioMaximo: string;
};

const FILTROS_INICIALES: FiltrosBusqueda = {
  busqueda: "",
  tipoVehiculo: "",
  transmision: "",
  combustible: "",
  precioMinimo: "",
  precioMaximo: "",
};

const fotoPrincipal = (anuncio: AnuncioListado): string =>
  urlImagen(anuncio.fotos?.[0]) || "/sin-foto.svg";

function construirUrlCatalogo(filtros: FiltrosBusqueda): string {
  const params = new URLSearchParams();

  if (filtros.busqueda.trim()) params.set("busqueda", filtros.busqueda.trim());
  if (filtros.tipoVehiculo) params.set("tipo", filtros.tipoVehiculo);
  if (filtros.transmision) params.set("transmision", filtros.transmision);
  if (filtros.combustible) params.set("combustible", filtros.combustible);
  if (filtros.precioMinimo) params.set("precioMinimo", filtros.precioMinimo);
  if (filtros.precioMaximo) params.set("precioMaximo", filtros.precioMaximo);

  return `/vehiculos${params.size ? `?${params.toString()}` : ""}`;
}

function TarjetaVehiculo({
  anuncio,
  variante = "normal",
  prioridad = false,
}: {
  anuncio: AnuncioListado;
  variante?: "principal" | "normal" | "compacta";
  prioridad?: boolean;
}) {
  const titulo = `${anuncio.marca} ${anuncio.modelo}`;
  const destacable =
    anuncio.esDestacado &&
    anuncio.fechaDestacadoHasta &&
    new Date(anuncio.fechaDestacadoHasta) > new Date();

  const esPrincipal = variante === "principal";
  const esCompacta = variante === "compacta";

  const imgRef = useRef<HTMLImageElement>(null);

  const handleMouseMove = useCallback(
    (e: React.MouseEvent<HTMLAnchorElement>) => {
      const img = imgRef.current;
      if (!img) return;
      const rect = e.currentTarget.getBoundingClientRect();
      const x = ((e.clientX - rect.left) / rect.width - 0.5) * 8;
      const y = ((e.clientY - rect.top) / rect.height - 0.5) * 8;
      img.style.transform = `scale(1.10) translate(${x}px, ${y}px)`;
    },
    [],
  );

  const handleMouseLeave = useCallback(() => {
    const img = imgRef.current;
    if (img) img.style.transform = "";
  }, []);

  return (
    <Link
      to={urlAnuncio(anuncio)}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
      className={`group relative block overflow-hidden rounded-2xl bg-slate-950 text-white shadow-lg shadow-slate-950/15 transition duration-300 hover:-translate-y-1 hover:shadow-2xl hover:shadow-slate-950/25 focus:outline-none focus:ring-4 focus:ring-brand/30 ${
        esPrincipal
          ? "min-h-[420px] sm:min-h-[470px]"
          : esCompacta
            ? "min-h-[200px]"
            : "min-h-[290px]"
      }`}
    >
      <img
        ref={imgRef}
        src={fotoPrincipal(anuncio)}
        alt={titulo}
        loading={prioridad ? "eager" : "lazy"}
        fetchPriority={prioridad ? "high" : "auto"}
        decoding="async"
        className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 ease-out will-change-transform"
      />

      <div className="absolute inset-0 bg-gradient-to-t from-slate-950 via-slate-950/35 to-slate-950/5" />

      <div className="absolute left-4 right-4 top-4 flex items-start justify-between gap-3">
        {destacable ? (
          <span
            className={`rounded-full bg-amber-400 font-bold text-amber-950 shadow-sm ${
              esCompacta ? "px-2.5 py-1 text-[11px]" : "px-3 py-1.5 text-xs"
            }`}
          >
            Destacado
          </span>
        ) : (
          <span className="rounded-full bg-black/35 px-3 py-1.5 text-xs font-semibold text-white/95 backdrop-blur">
            {anuncio.anio}
          </span>
        )}

        {anuncio.esDealerVerificado && (
          <BadgeVerificado compacto={esCompacta} className="shadow-sm" />
        )}
      </div>

      <div className="absolute inset-x-0 bottom-0 p-4 sm:p-5">
        <p
          className={`${esPrincipal ? "text-2xl sm:text-3xl" : "text-lg"} font-bold tracking-tight text-white`}
        >
          {titulo}
        </p>
        {anuncio.version && !esCompacta && (
          <p className="mt-1 truncate text-sm text-white/75">
            {anuncio.version}
          </p>
        )}

        <p
          className={`${esPrincipal ? "mt-3 text-xl" : "mt-2 text-base"} font-bold text-cyan-200`}
        >
          {formatearPrecio(anuncio.precio, anuncio.moneda)}
        </p>

        {!esCompacta && (
          <div className="mt-4 flex flex-wrap gap-x-4 gap-y-2 text-xs text-white/80">
            <span className="inline-flex items-center gap-1.5">
              <FaTachometerAlt />
              {anuncio.kilometraje.toLocaleString("es-DO")} km
            </span>
            {anuncio.ubicacion && (
              <span className="inline-flex min-w-0 items-center gap-1.5">
                <FaMapMarkerAlt className="shrink-0" />
                <span className="truncate">{anuncio.ubicacion}</span>
              </span>
            )}
          </div>
        )}
      </div>
    </Link>
  );
}

function GaleriaDestacada({ anuncios }: { anuncios: AnuncioListado[] }) {
  if (anuncios.length === 0) return null;

  const principal = anuncios[0];
  const secundarios = anuncios.slice(1, 5);

  return (
    <section className="relative overflow-hidden border-b border-line py-12 sm:py-16">
      <SectionBackground
        variant="gallery"
        className="mx-auto max-w-6xl px-6 sm:px-8"
      >
        <div className="mb-6 flex items-center justify-between gap-4">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.18em] text-brand">
              Selección de la semana
            </p>
            <h2 className="mt-1 text-2xl font-bold tracking-tight text-ink sm:text-3xl">
              Vehículos que merecen tu atención
            </h2>
          </div>
          <Link
            to="/vehiculos"
            className="hidden items-center gap-2 text-sm font-bold text-brand transition hover:text-brand-hover sm:inline-flex"
          >
            Ver inventario
            <FaArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        <div className="grid gap-4 lg:grid-cols-[1.45fr_1fr]">
          <TarjetaVehiculo anuncio={principal} variante="principal" prioridad />

          <div className="grid grid-cols-2 gap-4">
            {secundarios.map((anuncio, indice) => (
              <TarjetaVehiculo
                key={anuncio.id}
                anuncio={anuncio}
                variante="compacta"
                prioridad={indice < 2}
              />
            ))}

            {secundarios.length < 4 &&
              Array.from({ length: 4 - secundarios.length }).map(
                (_, indice) => (
                  <Link
                    key={`ver-todos-${indice}`}
                    to="/vehiculos"
                    className="flex min-h-[200px] flex-col items-center justify-center rounded-2xl border border-dashed border-line bg-surface p-5 text-center transition hover:border-brand/40 hover:bg-brand-soft"
                  >
                    <FaCar className="text-2xl text-brand" />
                    <span className="mt-3 text-sm font-bold text-ink">
                      Explorar vehículos
                    </span>
                    <span className="mt-1 text-xs text-ink-2">
                      Ver todo el inventario
                    </span>
                  </Link>
                ),
              )}
          </div>
        </div>

        <Link
          to="/vehiculos"
          className="mt-6 inline-flex items-center gap-2 text-sm font-bold text-brand sm:hidden"
        >
          Ver todo el inventario
          <FaArrowRight className="h-3.5 w-3.5" />
        </Link>
      </SectionBackground>
    </section>
  );
}

function BuscadorCatalogo() {
  const navigate = useNavigate();
  const [filtros, setFiltros] = useState<FiltrosBusqueda>(FILTROS_INICIALES);

  const cambiar = (
    event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value } = event.target;
    setFiltros((actual) => ({ ...actual, [name]: value }));
  };

  const buscar = (event: React.FormEvent) => {
    event.preventDefault();
    navigate(construirUrlCatalogo(filtros));
  };

  // Solo selecciona el tipo visualmente: la navegación al directorio ocurre
  // al presionar "Buscar en el catálogo", así el usuario puede combinar
  // más filtros (transmisión, combustible, precios) antes de buscar.
  const seleccionarTipo = (tipoVehiculo: string) => {
    setFiltros((actual) => ({ ...actual, tipoVehiculo }));
  };

  return (
    <section className="relative overflow-hidden border-y border-line">
      <SectionBackground
        variant="search"
        className="mx-auto max-w-6xl px-6 py-14 sm:px-8 sm:py-16"
      >
        <div className="grid gap-10 lg:grid-cols-[0.8fr_1.2fr] lg:items-center">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.18em] text-brand">
              Encuentra tu opción
            </p>
            <h2 className="mt-3 text-balance text-3xl font-bold tracking-tight text-ink sm:text-4xl">
              Busca a tu manera.
            </h2>
            <p className="mt-4 max-w-md text-pretty leading-7 text-ink-2">
              Filtra por marca, modelo, tipo de vehículo, transmisión,
              combustible o presupuesto. El catálogo completo está listo para
              que compares opciones.
            </p>

            <div className="mt-7">
              <p className="mb-3 text-sm font-semibold text-ink">
                ¿Qué tipo de vehículo buscas?
              </p>
              <FiltroTipoVehiculo
                valor={filtros.tipoVehiculo}
                onSeleccionar={seleccionarTipo}
                size="compact"
              />
            </div>
          </div>

          <form
            onSubmit={buscar}
            className="rounded-3xl border border-line bg-surface p-5 shadow-xl shadow-slate-900/5 sm:p-7"
          >
            <div className="relative">
              <FaSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-ink-3" />
              <label htmlFor="busquedaVehiculo" className="sr-only">
                Buscar por marca o modelo
              </label>
              <input
                id="busquedaVehiculo"
                name="busqueda"
                type="search"
                value={filtros.busqueda}
                onChange={cambiar}
                placeholder="Ej.: Toyota Corolla, Honda CR-V..."
                className="w-full rounded-xl border border-line bg-page py-3.5 pl-11 pr-4 text-sm text-ink outline-none transition focus:border-brand focus:ring-4 focus:ring-brand/10"
              />
            </div>

            <div className="mt-4 grid gap-3 sm:grid-cols-2">
              <select
                name="transmision"
                value={filtros.transmision}
                onChange={cambiar}
                className="rounded-xl border border-line bg-page px-4 py-3 text-sm text-ink outline-none transition focus:border-brand focus:ring-4 focus:ring-brand/10"
              >
                <option value="">Transmisión</option>
                {TRANSMISIONES.map((opcion) => (
                  <option key={opcion.valor} value={opcion.valor}>
                    {opcion.etiqueta}
                  </option>
                ))}
              </select>

              <select
                name="combustible"
                value={filtros.combustible}
                onChange={cambiar}
                className="rounded-xl border border-line bg-page px-4 py-3 text-sm text-ink outline-none transition focus:border-brand focus:ring-4 focus:ring-brand/10"
              >
                <option value="">Combustible</option>
                {COMBUSTIBLES.map((opcion) => (
                  <option key={opcion.valor} value={opcion.valor}>
                    {opcion.etiqueta}
                  </option>
                ))}
              </select>
            </div>

            <div className="mt-3 grid grid-cols-2 gap-3">
              <label className="sr-only" htmlFor="precioMinimo">
                Precio mínimo
              </label>
              <input
                id="precioMinimo"
                name="precioMinimo"
                type="number"
                min="0"
                inputMode="numeric"
                value={filtros.precioMinimo}
                onChange={cambiar}
                placeholder="Precio mínimo"
                className="min-w-0 rounded-xl border border-line bg-page px-4 py-3 text-sm text-ink outline-none transition placeholder:text-ink-3 focus:border-brand focus:ring-4 focus:ring-brand/10"
              />
              <label className="sr-only" htmlFor="precioMaximo">
                Precio máximo
              </label>
              <input
                id="precioMaximo"
                name="precioMaximo"
                type="number"
                min="0"
                inputMode="numeric"
                value={filtros.precioMaximo}
                onChange={cambiar}
                placeholder="Precio máximo"
                className="min-w-0 rounded-xl border border-line bg-page px-4 py-3 text-sm text-ink outline-none transition placeholder:text-ink-3 focus:border-brand focus:ring-4 focus:ring-brand/10"
              />
            </div>

            <button
              type="submit"
              className="mt-5 inline-flex w-full items-center justify-center gap-2 rounded-xl bg-brand px-6 py-3.5 text-sm font-bold text-white transition hover:bg-brand-hover focus:outline-none focus:ring-4 focus:ring-brand/20"
            >
              Buscar en el catálogo
              <FaSearch className="h-3.5 w-3.5" />
            </button>
          </form>
        </div>
      </SectionBackground>
    </section>
  );
}

function TarjetaReciente({ anuncio }: { anuncio: AnuncioListado }) {
  const titulo = `${anuncio.marca} ${anuncio.modelo}`;

  return (
    <Link
      to={urlAnuncio(anuncio)}
      className="group flex overflow-hidden rounded-2xl border border-line bg-surface transition hover:border-brand/35 hover:shadow-lg hover:shadow-slate-900/5 sm:block"
    >
      <div className="relative h-32 w-36 shrink-0 overflow-hidden bg-surface-2 sm:h-auto sm:w-auto sm:aspect-[16/10]">
        <img
          src={fotoPrincipal(anuncio)}
          alt={titulo}
          loading="lazy"
          decoding="async"
          className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
        />
        {anuncio.esDealerVerificado && (
          <BadgeVerificado className="absolute bottom-2 left-2 scale-90 origin-bottom-left" />
        )}
      </div>

      <div className="min-w-0 flex-1 p-4">
        <div className="flex items-start justify-between gap-2">
          <h3 className="truncate font-bold text-ink">{titulo}</h3>
          <span className="shrink-0 text-xs font-semibold text-ink-2">
            {anuncio.anio}
          </span>
        </div>
        {anuncio.version && (
          <p className="mt-1 truncate text-xs text-ink-2">{anuncio.version}</p>
        )}
        <p className="mt-2 font-bold text-brand">
          {formatearPrecio(anuncio.precio, anuncio.moneda)}
        </p>
        <div className="mt-2 flex items-center gap-3 text-xs text-ink-2">
          <span className="inline-flex items-center gap-1">
            <FaTachometerAlt />
            {anuncio.kilometraje.toLocaleString("es-DO")} km
          </span>
          {anuncio.ubicacion && (
            <span className="hidden items-center gap-1 sm:inline-flex">
              <FaMapMarkerAlt />
              {anuncio.ubicacion}
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}

function EstadoCarga() {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: CANTIDAD_RECIENTES }).map((_, indice) => (
        <div
          key={indice}
          className="overflow-hidden rounded-2xl border border-line bg-surface sm:block"
        >
          <div className="h-32 w-36 shimmer sm:h-auto sm:w-auto sm:aspect-[16/10]" />
          <div className="space-y-3 p-4">
            <div className="h-4 w-3/4 shimmer rounded" />
            <div className="h-5 w-1/2 shimmer rounded" />
          </div>
        </div>
      ))}
    </div>
  );
}

function Recientes({
  anuncios,
  cargando,
  error,
  reintentar,
}: {
  anuncios: AnuncioListado[];
  cargando: boolean;
  error: boolean;
  reintentar: () => void;
}) {
  return (
    <section className="relative overflow-hidden py-16">
      <SectionBackground
        variant="recent"
        className="mx-auto max-w-6xl px-6 sm:px-8"
      >
        <div className="mb-7 flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.18em] text-brand">
              Lo más reciente
            </p>
            <h2 className="mt-2 text-2xl font-bold tracking-tight text-ink sm:text-3xl">
              Recién publicados
            </h2>
          </div>
          <Link
            to="/vehiculos"
            className="inline-flex items-center gap-2 text-sm font-bold text-brand transition hover:text-brand-hover"
          >
            Ver todos <FaChevronRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        {cargando ? (
          <EstadoCarga />
        ) : error ? (
          <div className="rounded-2xl border border-red-500/20 bg-red-500/5 p-8 text-center">
            <p className="text-sm text-red-600">
              No se pudieron cargar los vehículos.
            </p>
            <button
              type="button"
              onClick={reintentar}
              className="mt-4 rounded-xl bg-brand px-5 py-2.5 text-sm font-semibold text-white"
            >
              Reintentar
            </button>
          </div>
        ) : anuncios.length > 0 ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {anuncios.map((anuncio) => (
              <TarjetaReciente key={anuncio.id} anuncio={anuncio} />
            ))}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed border-line bg-surface p-12 text-center">
            <motion.div
              initial={{ scale: 0.8, opacity: 0 }}
              whileInView={{ scale: 1, opacity: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, type: "spring", stiffness: 200 }}
            >
              <FaCar className="mx-auto text-4xl text-ink-3" />
            </motion.div>
            <h2 className="mt-4 text-xl font-bold text-ink">
              Próximamente habrá vehículos disponibles
            </h2>
            <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-ink-2">
              Estamos preparando el inventario. Si deseas vender, puedes ser de
              los primeros en publicar.
            </p>
            <Link
              to="/precios"
              className="mt-6 inline-flex items-center gap-2 rounded-xl bg-brand px-5 py-3 text-sm font-semibold text-white"
            >
              Publicar vehículo <FaArrowRight className="h-3.5 w-3.5" />
            </Link>
          </div>
        )}
      </SectionBackground>
    </section>
  );
}

function CtaVendedor() {
  return (
    <section className="relative overflow-hidden pt-12 pb-16 sm:pt-16">
      <SectionBackground
        variant="cta"
        className="mx-auto max-w-6xl px-6 sm:px-8"
      >
        <div className="overflow-hidden rounded-3xl border border-brand/15 bg-brand-soft">
          <div className="grid lg:grid-cols-[1.15fr_0.85fr]">
            <div className="p-8 sm:p-10">
              <p className="text-xs font-bold uppercase tracking-[0.18em] text-brand">
                Para vendedores y dealers
              </p>
              <h2 className="mt-3 text-balance text-3xl font-bold tracking-tight text-ink sm:text-4xl">
                Tu inventario merece una vitrina profesional.
              </h2>
              <p className="mt-4 max-w-xl leading-7 text-ink-2">
                Crea anuncios detallados, agrega fotos, comparte tus enlaces y
                recibe consultas directamente de compradores interesados.
              </p>
              <Link
                to="/precios"
                className="mt-7 inline-flex items-center gap-2 rounded-xl bg-brand px-6 py-3.5 text-sm font-bold text-white transition hover:bg-brand-hover"
              >
                Publicar un vehículo <FaArrowRight className="h-3.5 w-3.5" />
              </Link>
            </div>
            <div className="cta-mesh relative flex items-center justify-center bg-slate-950 p-8 text-center text-white sm:p-10">
              <div className="cta-orb absolute -left-12 -top-12 h-48 w-48 rounded-full bg-cyan-500/15 blur-3xl" />
              <div className="cta-orb absolute -bottom-10 -right-10 h-40 w-40 rounded-full bg-violet-500/15 blur-3xl" style={{ animationDelay: "2s" }} />
              <div className="relative">
                <FaCar className="mx-auto text-5xl text-cyan-300" />
                <p className="mt-5 text-xl font-bold">
                  Muestra. Conecta. Vende.
                </p>
                <p className="mt-2 max-w-xs text-sm leading-6 text-slate-300">
                  Una forma simple de mostrar tus vehículos a compradores de
                  República Dominicana.
                </p>
              </div>
            </div>
          </div>
        </div>
      </SectionBackground>
    </section>
  );
}

function PiePaginaHome() {
  return (
    <footer className="border-t border-line bg-surface py-9">
      <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-6 text-sm text-ink-2 sm:flex-row sm:px-8">
        <span>© 2026 AutoMarket RD. Todos los derechos reservados.</span>
        <nav className="flex flex-wrap items-center justify-center gap-x-5 gap-y-2">
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
  const { data: destacadosData, isLoading: destacadosCargando } = useQuery({
    queryKey: ["anuncios-destacados-home", CANTIDAD_DESTACADOS],
    queryFn: () => anuncioService.obtenerDestacados(1, CANTIDAD_DESTACADOS),
    staleTime: 1000 * 60 * 10,
    retry: 1,
  });

  const {
    data: recientesData,
    isLoading: recientesCargando,
    isError: recientesError,
    refetch: recargarRecientes,
  } = useQuery({
    queryKey: ["anuncios-recientes-home", CANTIDAD_RECIENTES],
    queryFn: () =>
      catalogoService.buscar({
        paginaActual: 1,
        cantidadAnuncios: CANTIDAD_RECIENTES,
      }),
    staleTime: 1000 * 60 * 5,
    retry: 1,
  });

  const destacados = destacadosData?.items ?? [];
  const recientes = recientesData?.items ?? [];

  return (
    <div className="min-h-screen bg-page text-ink">
      <HeaderPublico />

      <section className="relative overflow-hidden border-b border-line bg-surface">
        <MeshGradientCanvas />
        <div className="relative mx-auto max-w-6xl px-6 py-16 text-center sm:px-8 sm:py-24">
          <h1 className="text-balance text-4xl font-bold tracking-tight text-ink sm:text-5xl">
              <ShinyText>Compra y vende vehículos</ShinyText>
              <span className="block text-brand">en República Dominicana</span>
            </h1>
          <motion.p
            className="mx-auto mt-5 max-w-2xl text-pretty leading-7 text-ink-2"
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.4 }}
          >
            Explora vehículos de agencias verificadas y vendedores particulares
            en toda República Dominicana. Compara, contacta directo y cierra tu
            trato hoy.
          </motion.p>
        </div>
      </section>

      {destacadosCargando ? (
        <section className="relative overflow-hidden py-12 sm:py-16">
          <SectionBackground
            variant="gallery"
            className="mx-auto max-w-6xl px-6 sm:px-8"
          >
            <div className="h-8 w-72 shimmer rounded" />
            <div className="mt-5 grid gap-4 lg:grid-cols-[1.45fr_1fr]">
              <div className="min-h-[420px] shimmer rounded-2xl" />
              <div className="grid grid-cols-2 gap-4">
                {Array.from({ length: 4 }).map((_, indice) => (
                  <div
                    key={indice}
                    className="min-h-[200px] shimmer rounded-2xl"
                  />
                ))}
              </div>
            </div>
          </SectionBackground>
        </section>
      ) : (
        <motion.div
          initial={{ opacity: 0, y: 24 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: "-60px" }}
          transition={{ duration: 0.45 }}
        >
          <GaleriaDestacada anuncios={destacados} />
        </motion.div>
      )}

      <motion.div
        initial={{ opacity: 0, y: 24 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true, margin: "-60px" }}
        transition={{ duration: 0.45, delay: 0.05 }}
      >
        <BuscadorCatalogo />
      </motion.div>

      <div className="mx-auto max-w-6xl px-6 pb-4 sm:px-8">
        <AdUnit
          className="rounded-2xl border border-line bg-surface/60 px-4 py-6"
        />
      </div>

      <motion.div
        initial={{ opacity: 0, y: 24 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true, margin: "-60px" }}
        transition={{ duration: 0.45, delay: 0.1 }}
      >
        <Recientes
          anuncios={recientes}
          cargando={recientesCargando}
          error={recientesError}
          reintentar={() => recargarRecientes()}
        />
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 24 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true, margin: "-60px" }}
        transition={{ duration: 0.45, delay: 0.15 }}
      >
        <CtaVendedor />
      </motion.div>
      <PiePaginaHome />
    </div>
  );
}
