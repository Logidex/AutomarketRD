import Swal from "sweetalert2";
import Spinner from "../../components/ui/Spinner";
import { useAdminResumen } from "../../hooks/useAdmin";

export default function AdminIndex() {
  const { data: resumen, isLoading, isError, error } = useAdminResumen();

  if (isError) {
    void Swal.fire({
      icon: "error",
      title: "Error",
      text: error instanceof Error ? error.message : "No se pudo cargar el resumen.",
      confirmButtonColor: "#7c3aed",
    });
  }

  if (isLoading) {
    return <Spinner />;
  }

  if (!resumen) {
    return (
      <div className="p-6">
        <div className="text-ink-3">No hay datos disponibles.</div>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-ink">
          Resumen general de la plataforma
        </h2>
      </div>

      {/* Fila 1: Usuarios, anuncios, leads */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        <MetricaCard
          titulo="Usuarios totales"
          valor={resumen.totalUsuarios ?? 0}
          color="bg-violet-500"
        />
        <MetricaCard
          titulo="Anuncios totales"
          valor={resumen.totalAnuncios ?? 0}
          color="bg-blue-500"
        />
        <MetricaCard
          titulo="Leads totales"
          valor={resumen.totalLeads ?? 0}
          color="bg-emerald-500"
        />
      </div>

      {/* Fila 2: Anuncios por estado */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
        <MetricaCard
          titulo="Anuncios activos"
          valor={resumen.anunciosActivos ?? 0}
          color="bg-green-500"
        />
        <MetricaCard
          titulo="Borradores"
          valor={resumen.anunciosBorrador ?? 0}
          color="bg-gray-400"
        />
        <MetricaCard
          titulo="Pausados"
          valor={resumen.anunciosPausados ?? 0}
          color="bg-yellow-500"
        />
        <MetricaCard
          titulo="Vendidos"
          valor={resumen.anunciosVendidos ?? 0}
          color="bg-blue-500"
        />
      </div>

      {/* Fila 3: Leads no leídos + más vistos */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="rounded-lg border border-line bg-surface p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-semibold text-ink">Leads</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-ink-2">Totales:</span>
              <span className="font-bold text-ink">
                {resumen.totalLeads ?? 0}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-ink-2">No leídos:</span>
              <span className="font-bold text-red-600">
                {resumen.leadsNoLeidos ?? 0}
              </span>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-line bg-surface p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-semibold text-ink">
            Anuncios más vistos
          </h3>
          {!resumen.anunciosMasVistos ||
          resumen.anunciosMasVistos.length === 0 ? (
            <p className="text-ink-3">Aún no hay anuncios con vistas.</p>
          ) : (
            <ul className="space-y-2">
              {resumen.anunciosMasVistos.map((anuncio) => (
                <li
                  key={anuncio.id}
                  className="flex items-center justify-between rounded border border-line p-3 hover:bg-surface-2"
                >
                  <span className="font-medium text-ink">
                    {anuncio.nombreAnuncio}
                  </span>
                  <span className="rounded bg-violet-100 px-2 py-1 text-sm font-semibold text-violet-700">
                    {anuncio.vistas} vistas
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}

function MetricaCard({
  titulo,
  valor,
  color = "bg-blue-500",
}: {
  titulo: string;
  valor: number;
  color?: string;
}) {
  return (
    <div className="rounded-lg border border-line bg-surface p-6 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-ink-2">{titulo}</p>
          <p className="mt-2 text-3xl font-bold text-ink">{valor}</p>
        </div>
        <div className={`h-12 w-12 rounded-full ${color} opacity-20`} />
      </div>
    </div>
  );
}