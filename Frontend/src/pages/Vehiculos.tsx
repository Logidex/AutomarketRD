import { useState, useEffect, useCallback } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import {
  FaBalanceScale,
  FaCar,
  FaCalendarAlt,
  FaMapMarkerAlt,
  FaSearch,
  FaTachometerAlt,
} from "react-icons/fa";
import { useVehiculos } from "../hooks/useVehiculos";
import type { AnuncioListado } from "../types/anuncio.types";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";
import { useComparador } from "../context/ComparadorContext";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from "../constants/vehiculo.opciones";

const ANIO_ACTUAL = new Date().getFullYear();

interface Filtros {
  marca: string;
  modelo: string;
  tipoVehiculo: string;
  transmision: string;
  combustible: string;
  ubicacion: string;
  condicion: string;
  enOferta: string;
  anioDesde: string;
  anioHasta: string;
  precioMinimo: string;
  precioMaximo: string;
  kilometrajeMaximo: string;
}

const FILTROS_INICIALES: Filtros = {
  marca: "",
  modelo: "",
  tipoVehiculo: "",
  transmision: "",
  combustible: "",
  ubicacion: "",
  condicion: "",
  enOferta: "",
  anioDesde: "",
  anioHasta: "",
  precioMinimo: "",
  precioMaximo: "",
  kilometrajeMaximo: "",
};

function filtrosDesdeParams(params: URLSearchParams): Filtros {
  return {
    marca: params.get("marca") ?? "",
    modelo: params.get("modelo") ?? "",
    tipoVehiculo: params.get("tipo") ?? "",
    transmision: params.get("transmision") ?? "",
    combustible: params.get("combustible") ?? "",
    ubicacion: params.get("ubicacion") ?? "",
    condicion: params.get("condicion") ?? "",
    enOferta: params.get("enOferta") ?? "",
    anioDesde: params.get("anioDesde") ?? "",
    anioHasta: params.get("anioHasta") ?? "",
    precioMinimo: params.get("precioMinimo") ?? "",
    precioMaximo: params.get("precioMaximo") ?? "",
    kilometrajeMaximo: params.get("kmMax") ?? "",
  };
}

function paramsDesdeFiltros(filtros: Filtros): URLSearchParams {
  const params = new URLSearchParams();
  if (filtros.marca) params.set("marca", filtros.marca);
  if (filtros.modelo) params.set("modelo", filtros.modelo);
  if (filtros.tipoVehiculo) params.set("tipo", filtros.tipoVehiculo);
  if (filtros.transmision) params.set("transmision", filtros.transmision);
  if (filtros.combustible) params.set("combustible", filtros.combustible);
  if (filtros.ubicacion) params.set("ubicacion", filtros.ubicacion);
  if (filtros.condicion) params.set("condicion", filtros.condicion);
  if (filtros.enOferta) params.set("enOferta", "true");
  if (filtros.anioDesde) params.set("anioDesde", filtros.anioDesde);
  if (filtros.anioHasta) params.set("anioHasta", filtros.anioHasta);
  if (filtros.precioMinimo) params.set("precioMinimo", filtros.precioMinimo);
  if (filtros.precioMaximo) params.set("precioMaximo", filtros.precioMaximo);
  if (filtros.kilometrajeMaximo) params.set("kmMax", filtros.kilometrajeMaximo);
  return params;
}

export default function Vehiculos() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  const [pagina, setPagina] = useState(1);
  const [filtros, setFiltros] = useState<Filtros>(FILTROS_INICIALES);
  const [filtrosAplicados, setFiltrosAplicados] = useState<Filtros>(FILTROS_INICIALES);

  // Selección para el comparador (compartida y persistida en localStorage)
  const {
    seleccionados,
    esSeleccionado,
    toggle: toggleComparar,
    limpiar: limpiarSeleccion,
    maxVehiculos,
  } = useComparador();

  const irAComparador = useCallback(() => {
    if (seleccionados.length < 2) return;
    const qs = seleccionados.map((id) => `ids=${id}`).join("&");
    navigate(`/comparador?${qs}`);
  }, [seleccionados, navigate]);

  // Sincronizar filtros con URL al cargar
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setFiltros(filtrosDesdeParams(searchParams));
  }, [searchParams]);

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
      modelo: filtrosAplicados.modelo || undefined,
      tipoVehiculo: filtrosAplicados.tipoVehiculo || undefined,
      transmision: filtrosAplicados.transmision || undefined,
      combustible: filtrosAplicados.combustible || undefined,
      ubicacion: filtrosAplicados.ubicacion || undefined,
      condicion: filtrosAplicados.condicion || undefined,
      enOferta: filtrosAplicados.enOferta ? true : undefined,
      anioDesde: filtrosAplicados.anioDesde ? Number(filtrosAplicados.anioDesde) : undefined,
      anioHasta: filtrosAplicados.anioHasta ? Number(filtrosAplicados.anioHasta) : undefined,
      precioMinimo: filtrosAplicados.precioMinimo ? Number(filtrosAplicados.precioMinimo) : undefined,
      precioMaximo: filtrosAplicados.precioMaximo ? Number(filtrosAplicados.precioMaximo) : undefined,
      kilometrajeMaximo: filtrosAplicados.kilometrajeMaximo ? Number(filtrosAplicados.kilometrajeMaximo) : undefined,
    },
    pagina,
  });

  const anuncios = data?.items ?? [];
  const totalRegistros = data?.totalRegistros ?? 0;
  const cantidadPorPagina = data?.cantidadPorPagina ?? 12;
  const totalPaginas = cantidadPorPagina > 0
    ? Math.ceil(totalRegistros / cantidadPorPagina)
    : 1;

  const cargando = isLoading || isFetching;

  const aplicarBusqueda = (e: React.FormEvent) => {
    e.preventDefault();
    const aplicados = { ...filtros };
    const params = paramsDesdeFiltros(aplicados);
    setSearchParams(params, { replace: true });
    setFiltrosAplicados(aplicados);
    setPagina(1);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const limpiarFiltros = () => {
    setSearchParams({}, { replace: true });
    setFiltros(FILTROS_INICIALES);
    setFiltrosAplicados({ ...FILTROS_INICIALES });
    setPagina(1);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const irAPagina = (p: number) => {
    if (p < 1 || p > totalPaginas || p === pagina) return;
    window.scrollTo({ top: 0, behavior: "smooth" });
    setPagina(p);
  };

  const fotoPrincipal = (anuncio: AnuncioListado): string =>
    urlImagen(anuncio.fotos?.[0]) ||
    "https://via.placeholder.com/600x400?text=Sin+Foto";

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

      {/* CABECERA + FILTROS */}
      <section className="border-b border-white/10 bg-gradient-to-b from-[#11161f] to-[#0c101b]">
        <div className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
          <h1 className="text-3xl font-bold">
            Todos los vehículos
            {totalRegistros > 0 && (
              <span className="ml-2 text-base font-normal text-[#9aa1b1]">
                ({totalRegistros})
              </span>
            )}
          </h1>
          <p className="mt-2 text-sm text-[#9aa1b1]">
            Filtra por marca, modelo, año, precio, kilometraje y más para
            encontrar tu próximo vehículo.
          </p>

          <form onSubmit={aplicarBusqueda} className="mt-8">
            <div className="grid grid-cols-1 gap-3 rounded-2xl border border-white/10 bg-[#13161d] p-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              <div className="relative">
                <FaSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-gray-500" />
                <input
                  type="text"
                  value={filtros.marca}
                  onChange={(e) =>
                    setFiltros({ ...filtros, marca: e.target.value })
                  }
                  placeholder="Marca (ej. Toyota)"
                  className="w-full rounded-xl border border-white/10 bg-[#0c101b] py-3 pl-12 pr-4 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
              </div>

              <input
                type="text"
                value={filtros.modelo}
                onChange={(e) =>
                  setFiltros({ ...filtros, modelo: e.target.value })
                }
                placeholder="Modelo"
                className="w-full rounded-xl border border-white/10 bg-[#0c101b] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />

              <select
                value={filtros.condicion}
                onChange={(e) =>
                  setFiltros({ ...filtros, condicion: e.target.value })
                }
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Condición</option>
                <option value="Nuevo">Nuevo</option>
                <option value="Usado">Usado</option>
              </select>

              <select
                value={filtros.enOferta}
                onChange={(e) =>
                  setFiltros({ ...filtros, enOferta: e.target.value })
                }
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Ofertas</option>
                <option value="true">En oferta</option>
              </select>

              <select
                value={filtros.tipoVehiculo}
                onChange={(e) =>
                  setFiltros({ ...filtros, tipoVehiculo: e.target.value })
                }
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Tipo de vehículo</option>
                {TIPOS_VEHICULO.map((opcion) => (
                  <option key={opcion.valor} value={opcion.valor}>
                    {opcion.etiqueta}
                  </option>
                ))}
              </select>

              <select
                value={filtros.transmision}
                onChange={(e) =>
                  setFiltros({ ...filtros, transmision: e.target.value })
                }
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Transmisión</option>
                {TRANSMISIONES.map((opcion) => (
                  <option key={opcion.valor} value={opcion.valor}>
                    {opcion.etiqueta}
                  </option>
                ))}
              </select>

              <select
                value={filtros.combustible}
                onChange={(e) =>
                  setFiltros({ ...filtros, combustible: e.target.value })
                }
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Combustible</option>
                {COMBUSTIBLES.map((opcion) => (
                  <option key={opcion.valor} value={opcion.valor}>
                    {opcion.etiqueta}
                  </option>
                ))}
              </select>

              <input
                type="text"
                value={filtros.ubicacion}
                onChange={(e) =>
                  setFiltros({ ...filtros, ubicacion: e.target.value })
                }
                placeholder="Ubicación (ej. Santo Domingo)"
                className="w-full rounded-xl border border-white/10 bg-[#0c101b] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />

              <div className="flex items-center gap-2">
                <select
                  value={filtros.anioDesde}
                  onChange={(e) =>
                    setFiltros({ ...filtros, anioDesde: e.target.value })
                  }
                  className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
                >
                  <option value="">Año desde</option>
                  {Array.from(
                    { length: ANIO_ACTUAL - 1969 },
                    (_, i) => ANIO_ACTUAL - i,
                  ).map((anio) => (
                    <option key={anio} value={anio}>
                      {anio}
                    </option>
                  ))}
                </select>
                <span className="text-gray-500">-</span>
                <select
                  value={filtros.anioHasta}
                  onChange={(e) =>
                    setFiltros({ ...filtros, anioHasta: e.target.value })
                  }
                  className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
                >
                  <option value="">Año hasta</option>
                  {Array.from(
                    { length: ANIO_ACTUAL - 1969 },
                    (_, i) => ANIO_ACTUAL - i,
                  ).map((anio) => (
                    <option key={anio} value={anio}>
                      {anio}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="text"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  value={filtros.precioMinimo}
                  onChange={(e) =>
                    setFiltros({ ...filtros, precioMinimo: e.target.value })
                  }
                  placeholder="Precio mín."
                  className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
                <span className="text-gray-500">-</span>
                <input
                  type="text"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  value={filtros.precioMaximo}
                  onChange={(e) =>
                    setFiltros({ ...filtros, precioMaximo: e.target.value })
                  }
                  placeholder="Precio máx."
                  className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
              </div>

              <input
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                value={filtros.kilometrajeMaximo}
                onChange={(e) =>
                  setFiltros({ ...filtros, kilometrajeMaximo: e.target.value })
                }
                placeholder="Kilometraje máx. (km)"
                className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>

            <div className="mt-4 flex flex-wrap items-center gap-3">
              <button
                type="submit"
                disabled={cargando}
                className="rounded-xl bg-blue-500 px-8 py-3 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed"
              >
                {cargando ? "Buscando..." : "Buscar"}
              </button>
              <button
                type="button"
                onClick={limpiarFiltros}
                disabled={cargando}
                className="rounded-xl border border-white/10 px-6 py-3 text-sm font-medium text-[#9aa1b1] transition-colors hover:border-white/30 hover:text-white disabled:opacity-50"
              >
                Limpiar filtros
              </button>
            </div>
          </form>
        </div>
      </section>

      {/* VITRINA */}
      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
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
              <div
                key={anuncio.id}
                className={`group relative flex flex-col overflow-hidden rounded-2xl border text-left transition-all hover:shadow-lg ${
                  esSeleccionado(anuncio.id)
                    ? "border-blue-500 bg-[#16202e]"
                    : "border-white/10 bg-[#13161d] hover:border-blue-500/40"
                }`}
              >
                <button
                  type="button"
                  onClick={() => navigate(`/anuncio/${anuncio.id}`)}
                  className="flex flex-col text-left"
                >
                  <div className="relative aspect-[16/10] overflow-hidden">
                    <img
                      src={fotoPrincipal(anuncio)}
                      alt={`${anuncio.marca} ${anuncio.modelo}`}
                      className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                    />
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

                    {anuncio.enOferta && anuncio.precioAnterior != null && (
                      <p className="text-sm text-[#9aa1b1]">
                        <span className="mr-2 line-through">
                          {formatearPrecio(anuncio.precioAnterior, anuncio.moneda)}
                        </span>
                        <span className="font-semibold text-green-400">Oferta</span>
                      </p>
                    )}

                    <div className="mt-4 flex flex-wrap gap-x-4 gap-y-2 border-t border-white/10 pt-4 text-xs text-[#9aa1b1]">
                      <span className="inline-flex items-center gap-1.5">
                        {anuncio.condicion === "Nuevo" ? (
                          <span className="rounded-full bg-blue-500/10 px-3 py-1 text-xs font-semibold text-blue-400">
                            Nuevo
                          </span>
                        ) : (
                          <span className="rounded-full bg-white/5 px-3 py-1 text-xs font-medium text-gray-300">
                            Usado
                          </span>
                        )}
                      </span>
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

                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    toggleComparar(anuncio.id);
                  }}
                  className={`mt-0 border-t px-5 py-3 text-left text-sm font-medium transition-colors ${
                    esSeleccionado(anuncio.id)
                      ? "border-blue-500/40 bg-blue-500/10 text-blue-400"
                      : "border-white/10 text-[#9aa1b1] hover:text-white"
                  }`}
                >
                  {esSeleccionado(anuncio.id) ? (
                    <>Quitar de comparar</>
                  ) : (
                    <>+ Agregar a comparar</>
                  )}
                </button>
              </div>
            ))}
          </div>
        )}

        {/* PAGINACIÓN */}
        {!cargando && !isError && totalPaginas > 1 && (
          <div className="mt-10 flex flex-wrap items-center justify-center gap-2">
            <button
              type="button"
              onClick={() => irAPagina(pagina - 1)}
              disabled={pagina <= 1}
              className="rounded-lg border border-white/10 px-4 py-2 text-sm font-medium transition-colors hover:border-white/30 disabled:cursor-not-allowed disabled:opacity-40"
            >
              Anterior
            </button>

            {Array.from({ length: totalPaginas }, (_, i) => i + 1)
              .filter((p) => {
                if (
                  totalPaginas > 7 &&
                  p !== 1 &&
                  p !== totalPaginas &&
                  Math.abs(p - pagina) > 2
                )
                  return false;
                return true;
              })
              .map((p) => (
                <button
                  key={p}
                  type="button"
                  onClick={() => irAPagina(p)}
                  className={`min-w-10 rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
                    p === pagina
                      ? "bg-blue-500 text-white"
                      : "border border-white/10 hover:border-white/30"
                  }`}
                >
                  {p}
                </button>
              ))}

            <button
              type="button"
              onClick={() => irAPagina(pagina + 1)}
              disabled={pagina >= totalPaginas}
              className="rounded-lg border border-white/10 px-4 py-2 text-sm font-medium transition-colors hover:border-white/30 disabled:cursor-not-allowed disabled:opacity-40"
            >
              Siguiente
            </button>
          </div>
        )}
      </main>

      {/* ESPACIO PARA QUE EL FOOTER NO QUEDE DETRÁS DE LA BARRA FLOTANTE */}
      <div className="h-20" />

      {/* BARRA FLOTANTE DE COMPARACIÓN */}
      <div className="fixed inset-x-0 bottom-0 z-40 border-t border-blue-500/40 bg-[#0d1117]/95 px-6 py-4 backdrop-blur">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-6 sm:px-8">
          <div className="flex flex-wrap items-center gap-2 text-sm text-[#9aa1b1]">
            {seleccionados.length === 0 ? (
              <span className="inline-flex items-center gap-2">
                <FaBalanceScale className="text-blue-500" />
                Selecciona 2 o más vehículos para compararlos
              </span>
            ) : (
              <>
                <span className="font-semibold text-white">
                  {seleccionados.length} seleccionado{seleccionados.length !== 1 && "s"}
                </span>
                {seleccionados.length >= maxVehiculos && (
                  <span className="text-xs text-amber-400">
                    Máximo {maxVehiculos} vehículos
                  </span>
                )}
              </>
            )}
            {seleccionados.length > 0 && (
              <button
                type="button"
                onClick={limpiarSeleccion}
                className="text-xs text-[#9aa1b1] underline-offset-2 transition-colors hover:text-white hover:underline"
              >
                Limpiar
              </button>
            )}
          </div>

          <button
            type="button"
            onClick={irAComparador}
            disabled={seleccionados.length < 2}
            className={`inline-flex items-center gap-2 rounded-lg px-6 py-2.5 text-sm font-semibold transition-colors ${
              seleccionados.length < 2
                ? "cursor-not-allowed bg-blue-500/40 text-white/60"
                : "bg-blue-500 text-white hover:bg-blue-600"
            }`}
          >
            <FaBalanceScale />
            {seleccionados.length < 2
              ? `Faltan ${2 - seleccionados.length}`
              : `Comparar (${seleccionados.length})`}
          </button>
        </div>
      </div>

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