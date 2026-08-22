import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { FaChevronLeft, FaChevronRight, FaSearch, FaStore } from "react-icons/fa";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";
import Spinner from "../components/Spinner";
import BadgeVerificado from "../components/BadgeVerificado";
import { dealerService, type AgenciaListado } from "../services/dealer.service";
import { urlImagen } from "../utils/imagen";
import { nombrePlan } from "../constants/planes";

const CANTIDAD_POR_PAGINA = 12;

const OPCIONES_PLAN = [
  { valor: "", etiqueta: "Todos los planes" },
  { valor: "Basico", etiqueta: "Básico" },
  { valor: "Pro", etiqueta: "Pro" },
  { valor: "Elite", etiqueta: "Elite" },
];

function clasePlanBadge(plan?: string | null): string {
  switch (plan) {
    case "Elite":
      return "bg-amber-500/15 text-amber-600 dark:text-amber-400";
    case "Pro":
      return "bg-violet-500/15 text-violet-600 dark:text-violet-400";
    case "Basico":
      return "bg-blue-500/15 text-blue-600 dark:text-blue-400";
    default:
      return "bg-surface-2 text-ink-3";
  }
}

function useListaAgencias(
  busqueda: string,
  soloVerificadas: boolean,
  planNivel: string,
  pagina: number
) {
  return useQuery({
    queryKey: ["agencias", busqueda, soloVerificadas, planNivel, pagina],
    queryFn: () =>
      dealerService.listarAgencias({
        busqueda,
        soloVerificadas,
        planNivel,
        pagina,
        cantidadPorPagina: CANTIDAD_POR_PAGINA,
      }),
    placeholderData: (prev) => prev,
    staleTime: 1000 * 60,
  });
}

function TarjetaAgencia({ agencia }: { agencia: AgenciaListado }) {
  return (
    <Link
      to={`/vendedor/${agencia.id}`}
      className="group flex flex-col rounded-2xl border border-line bg-surface p-6 shadow-sm transition-all hover:-translate-y-0.5 hover:border-brand/40 hover:shadow-lg"
    >
      <div className="flex items-center gap-4">
        <div className="flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-2xl border border-line bg-surface-2">
          {agencia.logoUrl ? (
            <img
              src={urlImagen(agencia.logoUrl)}
              alt={agencia.nombreAgencia}
              className="h-full w-full object-cover"
            />
          ) : (
            <FaStore className="text-2xl text-ink-3" />
          )}
        </div>

        <div className="min-w-0 flex-1">
          <h3 className="truncate text-lg font-bold text-ink dark:text-white">
            {agencia.nombreAgencia}
          </h3>
          <div className="mt-1 flex flex-wrap items-center gap-2">
            {agencia.esDealerVerificado && (
              <BadgeVerificado className="!px-2 !py-0.5 !text-[10px]" />
            )}
            <span
              className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${clasePlanBadge(
                agencia.planNivel
              )}`}
            >
              {agencia.planNivel ? nombrePlan(agencia.planNivel) : "Gratis"}
            </span>
          </div>
        </div>
      </div>

      <div className="mt-4 flex flex-wrap gap-x-4 gap-y-1.5 text-sm text-ink-2">
        <span className="inline-flex items-center gap-1.5">
          <FaStore className="text-xs text-ink-3" />
          {agencia.ubicacion || "Sin ubicación"}
        </span>
        <span className="inline-flex items-center gap-1.5">
          <span className="text-xs text-ink-3">{agencia.cantidadAnuncios}</span>
          {agencia.cantidadAnuncios === 1 ? "vehículo" : "vehículos"}
        </span>
      </div>

      {agencia.descripcion && (
        <p className="mt-3 line-clamp-2 text-sm leading-relaxed text-ink-3">
          {agencia.descripcion}
        </p>
      )}

      <span className="mt-auto pt-5 inline-flex items-center gap-1.5 text-sm font-semibold text-brand transition-colors group-hover:underline">
        Ver agencia
        <FaChevronRight className="text-xs" />
      </span>
    </Link>
  );
}

export default function Agencias() {
  const [busquedaInput, setBusquedaInput] = useState("");
  const [busqueda, setBusqueda] = useState("");
  const [soloVerificadas, setSoloVerificadas] = useState(false);
  const [planNivel, setPlanNivel] = useState("");
  const [pagina, setPagina] = useState(1);

  useEffect(() => {
    const timer = setTimeout(() => {
      setBusqueda(busquedaInput.trim());
      setPagina(1);
    }, 350);
    return () => clearTimeout(timer);
  }, [busquedaInput]);

  const { data, isLoading, isFetching } = useListaAgencias(
    busqueda,
    soloVerificadas,
    planNivel,
    pagina
  );

  const agencias = data?.items ?? [];
  const total = data?.totalRegistros ?? 0;
  const totalPaginas = data?.totalPaginas ?? 1;

  return (
    <div className="min-h-screen bg-page text-ink">
      {/* Header */}
      <header className="relative flex items-center justify-between border-b border-line bg-page/80 px-4 py-2 backdrop-blur sm:px-8">
        <div className="flex items-center gap-4">
          <Link to="/" className="flex items-center">
            <img src={logo} alt="AutoMarket RD" className="h-12 w-auto object-contain sm:h-16" />
          </Link>
          <span className="text-xl font-bold">Agencias</span>
        </div>

        <MenuPublico />
      </header>

      <main className="mx-auto max-w-6xl px-6 py-12 sm:px-8">
        <div className="text-center mb-10">
          <h1 className="text-4xl font-bold mb-3">Agencias disponibles</h1>
          <p className="mx-auto max-w-2xl text-ink-2">
            Encuentra las agencias de vehículos que operan en AutoMarket RD,
            compara su inventario y contacta directamente con ellas.
          </p>
        </div>

        {/* Buscador y filtros */}
        <div className="mb-8 flex flex-col gap-4 lg:flex-row lg:items-center">
          <div className="relative flex-1">
            <FaSearch className="absolute left-4 top-1/2 -translate-y-1/2 text-ink-3" />
            <input
              type="search"
              value={busquedaInput}
              onChange={(e) => setBusquedaInput(e.target.value)}
              placeholder="Buscar agencia por nombre..."
              className="w-full rounded-xl border border-line bg-surface py-3 pl-11 pr-4 text-sm outline-none transition-colors focus:border-brand"
            />
          </div>

          <select
            value={planNivel}
            onChange={(e) => {
              setPlanNivel(e.target.value);
              setPagina(1);
            }}
            className="rounded-xl border border-line bg-surface px-4 py-3 text-sm outline-none transition-colors focus:border-brand"
          >
            {OPCIONES_PLAN.map((opcion) => (
              <option key={opcion.valor} value={opcion.valor}>
                {opcion.etiqueta}
              </option>
            ))}
          </select>

          <label className="inline-flex cursor-pointer items-center gap-2 text-sm font-medium text-ink-2 select-none">
            <input
              type="checkbox"
              checked={soloVerificadas}
              onChange={(e) => {
                setSoloVerificadas(e.target.checked);
                setPagina(1);
              }}
              className="h-4 w-4 rounded accent-blue-500"
            />
            Solo verificadas
          </label>
        </div>

        {isLoading ? (
          <Spinner />
        ) : agencias.length === 0 ? (
          <div className="rounded-2xl border border-line bg-surface p-16 text-center">
            <FaStore className="mx-auto text-5xl text-ink-3" />
            <h3 className="mt-4 text-lg font-semibold">Sin resultados</h3>
            <p className="mt-2 text-sm text-ink-2">
              No encontramos agencias con los criterios seleccionados. Prueba
              con otra búsqueda o quita los filtros.
            </p>
          </div>
        ) : (
          <>
            <p className="mb-4 text-sm text-ink-2">
              {total} {total === 1 ? "agencia" : "agencias"}
              {isFetching && !isLoading ? " · actualizando..." : ""}
            </p>

            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {agencias.map((agencia) => (
                <TarjetaAgencia key={agencia.id} agencia={agencia} />
              ))}
            </div>

            {/* Paginación */}
            {totalPaginas > 1 && (
              <div className="mt-10 flex items-center justify-center gap-4">
                <button
                  type="button"
                  onClick={() => setPagina((p) => Math.max(1, p - 1))}
                  disabled={pagina <= 1}
                  className="inline-flex items-center gap-1.5 rounded-lg border border-line bg-surface px-4 py-2 text-sm font-semibold text-ink-2 transition-colors hover:text-ink disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <FaChevronLeft className="text-xs" />
                  Anterior
                </button>
                <span className="text-sm text-ink-2">
                  Página {pagina} de {totalPaginas}
                </span>
                <button
                  type="button"
                  onClick={() => setPagina((p) => Math.min(totalPaginas, p + 1))}
                  disabled={pagina >= totalPaginas}
                  className="inline-flex items-center gap-1.5 rounded-lg border border-line bg-surface px-4 py-2 text-sm font-semibold text-ink-2 transition-colors hover:text-ink disabled:cursor-not-allowed disabled:opacity-40"
                >
                  Siguiente
                  <FaChevronRight className="text-xs" />
                </button>
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}