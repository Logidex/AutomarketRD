import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { adminService, type AdminResumen } from "../../services/admin.service";
import Spinner from "../../components/Spinner";

export default function AdminIndex() {
  const [resumen, setResumen] = useState<AdminResumen | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function cargarResumen() {
      try {
        const data = await adminService.obtenerResumen();
        setResumen(data);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (error: any) {
        await Swal.fire({
          icon: "error",
          title: "Error",
          text: error.message || "No se pudo cargar el resumen.",
          confirmButtonColor: "#7c3aed",
        });
      } finally {
        setLoading(false);
      }
    }

    cargarResumen();
  }, []);

  if (loading) {
    return <Spinner />;
  }

  if (!resumen) {
    return (
      <div className="p-6">
        <div className="text-gray-500">No hay datos disponibles.</div>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900">
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
        <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-semibold text-gray-800">Leads</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-gray-600">Totales:</span>
              <span className="font-bold text-gray-900">
                {resumen.totalLeads ?? 0}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">No leídos:</span>
              <span className="font-bold text-red-600">
                {resumen.leadsNoLeidos ?? 0}
              </span>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-semibold text-gray-800">
            Anuncios más vistos
          </h3>
          {!resumen.anunciosMasVistos ||
          resumen.anunciosMasVistos.length === 0 ? (
            <p className="text-gray-500">Aún no hay anuncios con vistas.</p>
          ) : (
            <ul className="space-y-2">
              {resumen.anunciosMasVistos.map((anuncio) => (
                <li
                  key={anuncio.id}
                  className="flex items-center justify-between rounded border border-gray-100 p-3 hover:bg-gray-50"
                >
                  <span className="font-medium text-gray-800">
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
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600">{titulo}</p>
          <p className="mt-2 text-3xl font-bold text-gray-900">{valor}</p>
        </div>
        <div className={`h-12 w-12 rounded-full ${color} opacity-20`} />
      </div>
    </div>
  );
}