import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  FaCar,
  FaCalendarAlt,
  FaMapMarkerAlt,
  FaSearch,
  FaTachometerAlt,
} from "react-icons/fa";
import { catalogoService } from "../services/catalogo.service";
import { authService } from "../services/auth.service";
import type { AnuncioListado } from "../types/anuncio.types";
import logo from "../assets/AutoMarketRD_Logo.svg";

const TAMANO_PAGINA = 12;

const TIPOS_VEHICULO = [
  "Sedán",
  "SUV",
  "Pickup",
  "Hatchback",
  "Coupé",
  "Convertible",
  "Van",
  "Camioneta",
  "Otro",
];

const TRANSMISIONES = ["Automática", "Manual"];

const COMBUSTIBLES = [
  "Gasolina",
  "Diésel",
  "Híbrido",
  "Eléctrico",
];

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

export default function Home() {
  const [anuncios, setAnuncios] = useState<AnuncioListado[]>([]);
  const [pagina, setPagina] = useState(1);
  const [totalRegistros, setTotalRegistros] = useState(0);
  const [totalPaginas, setTotalPaginas] = useState(1);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");
  const [filtros, setFiltros] = useState<Filtros>(FILTROS_INICIALES);
  const [filtrosAplicados, setFiltrosAplicados] =
    useState<Filtros>(FILTROS_INICIALES);
  const [aplicando, setAplicando] = useState(false);

  const navigate = useNavigate();

  const cargar = useCallback(
    async (paginaActual: number, aplicados: Filtros) => {
      try {
        const resultado = await catalogoService.buscar({
          marca: aplicados.marca || undefined,
          tipoVehiculo: aplicados.tipoVehiculo || undefined,
          transmision: aplicados.transmision || undefined,
          combustible: aplicados.combustible || undefined,
          precioMinimo: aplicados.precioMinimo
            ? Number(aplicados.precioMinimo)
            : undefined,
          precioMaximo: aplicados.precioMaximo
            ? Number(aplicados.precioMaximo)
            : undefined,
          paginaActual,
          cantidadAnuncios: TAMANO_PAGINA,
        });

        const items = resultado.items.map((a) => ({
          ...a,
          precio: Number(a.precio ?? 0),
          kilometraje: Number(a.kilometraje ?? 0),
        }));

        const totalPag =
          resultado.cantidadPorPagina > 0
            ? Math.ceil(
                (resultado.totalRegistros ?? 0) / resultado.cantidadPorPagina,
              )
            : 1;

        setAnuncios(items);
        setTotalRegistros(resultado.totalRegistros ?? 0);
        setTotalPaginas(totalPag > 0 ? totalPag : 1);
        setError("");
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "No se pudieron cargar los vehículos. Inténtalo nuevamente.",
        );
      } finally {
        setCargando(false);
        setAplicando(false);
      }
    },
    [],
  );

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    cargar(pagina, filtrosAplicados);
  }, [cargar, pagina, filtrosAplicados]);

  const aplicarBusqueda = (e: React.FormEvent) => {
    e.preventDefault();
    setAplicando(true);
    setCargando(true);
    setPagina(1);
    setFiltrosAplicados({ ...filtros });
  };

  const limpiarFiltros = () => {
    setFiltros(FILTROS_INICIALES);
    setFiltrosAplicados({ ...FILTROS_INICIALES });
    setAplicando(true);
    setCargando(true);
    setPagina(1);
  };

  const irAPagina = (p: number) => {
    if (p < 1 || p > totalPaginas || p === pagina) return;
    window.scrollTo({ top: 0, behavior: "smooth" });
    setCargando(true);
    setPagina(p);
  };

  const fotoPrincipal = (anuncio: AnuncioListado): string =>
    anuncio.fotos && anuncio.fotos.length > 0
      ? anuncio.fotos[0]
      : "https://via.placeholder.com/600x400?text=Sin+Foto";

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      {/* HEADER */}
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link to="/" className="flex items-center gap-4">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-12 w-auto object-contain"
          />
        </Link>

        <nav className="flex items-center gap-6 text-sm font-medium">
          <Link
            to="/precios"
            className="text-[#9aa1b1] transition-colors hover:text-white"
          >
            Precios
          </Link>
          <Link
            to={authService.isAuthenticated() ? "/dashboard" : "/login"}
            className="rounded-lg bg-blue-500 px-5 py-2 font-semibold text-white transition-colors hover:bg-blue-600"
          >
            {authService.isAuthenticated() ? "Mi Panel" : "Iniciar Sesión"}
          </Link>
        </nav>
      </header>

      {/* HERO + BÚSQUEDA */}
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

          <form onSubmit={aplicarBusqueda} className="mx-auto mt-10 max-w-4xl">
            <div className="flex flex-col gap-3 rounded-2xl border border-white/10 bg-[#13161d] p-4 sm:flex-row">
              <div className="relative flex-1">
                <FaSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-gray-500" />
                <input
                  type="text"
                  value={filtros.marca}
                  onChange={(e) =>
                    setFiltros({ ...filtros, marca: e.target.value })
                  }
                  placeholder="Buscar por marca o modelo..."
                  className="w-full rounded-xl border border-white/10 bg-[#0c101b] py-3 pl-12 pr-4 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
              </div>

              <button
                type="submit"
                disabled={aplicando || cargando}
                className="rounded-xl bg-blue-500 px-8 py-3 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed"
              >
                {aplicando ? "Buscando..." : "Buscar"}
              </button>
            </div>

            <div className="mt-4 grid grid-cols-2 gap-3 md:grid-cols-4">
              <select
                value={filtros.tipoVehiculo}
                onChange={(e) =>
                  setFiltros({ ...filtros, tipoVehiculo: e.target.value })
                }
                className="rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Tipo de vehículo</option>
                {TIPOS_VEHICULO.map((tipo) => (
                  <option key={tipo} value={tipo}>
                    {tipo}
                  </option>
                ))}
              </select>

              <select
                value={filtros.transmision}
                onChange={(e) =>
                  setFiltros({ ...filtros, transmision: e.target.value })
                }
                className="rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Transmisión</option>
                {TRANSMISIONES.map((transmision) => (
                  <option key={transmision} value={transmision}>
                    {transmision}
                  </option>
                ))}
              </select>

              <select
                value={filtros.combustible}
                onChange={(e) =>
                  setFiltros({ ...filtros, combustible: e.target.value })
                }
                className="rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm text-gray-200 transition-colors focus:border-blue-500 focus:outline-none"
              >
                <option value="">Combustible</option>
                {COMBUSTIBLES.map((combustible) => (
                  <option key={combustible} value={combustible}>
                    {combustible}
                  </option>
                ))}
              </select>

              <div className="flex items-center gap-2">
                <input
                  type="number"
                  min={0}
                  value={filtros.precioMinimo}
                  onChange={(e) =>
                    setFiltros({ ...filtros, precioMinimo: e.target.value })
                  }
                  placeholder="Precio mín."
                  className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
                <span className="text-gray-500">-</span>
                <input
                  type="number"
                  min={0}
                  value={filtros.precioMaximo}
                  onChange={(e) =>
                    setFiltros({ ...filtros, precioMaximo: e.target.value })
                  }
                  placeholder="Precio máx."
                  className="w-full rounded-xl border border-white/10 bg-[#13161d] px-4 py-3 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
              </div>
            </div>
          </form>
        </div>
      </section>

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

        {cargando ? (
          <div className="flex items-center justify-center py-24">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
          </div>
        ) : error ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-400">{error}</p>
            <button
              type="button"
              onClick={() => {
                setCargando(true);
                cargar(pagina, filtrosAplicados);
              }}
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
              <button
                key={anuncio.id}
                type="button"
                onClick={() => navigate(`/anuncio/${anuncio.id}`)}
                className="group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] text-left transition-all hover:border-blue-500/40 hover:shadow-lg"
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
                    RD$ {anuncio.precio.toLocaleString("es-DO")}
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
                        {anuncio.tipoVehiculo}
                      </span>
                    )}
                    {anuncio.combustible && (
                      <span className="rounded-full bg-white/5 px-3 py-1 text-xs font-medium text-gray-300">
                        {anuncio.combustible}
                      </span>
                    )}
                    {anuncio.transmision && (
                      <span className="rounded-full bg-white/5 px-3 py-1 text-xs font-medium text-gray-300">
                        {anuncio.transmision}
                      </span>
                    )}
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}

        {/* PAGINACIÓN */}
        {!cargando && !error && totalPaginas > 1 && (
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