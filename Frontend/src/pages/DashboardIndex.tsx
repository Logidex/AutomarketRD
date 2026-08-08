import { useEffect, useState } from 'react';
import { dashboardService, type DashboardResumen } from '../services/dashboard.service';
import Swal from 'sweetalert2';

export default function DashboardIndex() {
  const [resumen, setResumen] = useState<DashboardResumen | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function cargarResumen() {
      try {
        const data = await dashboardService.obtenerResumen();
        setResumen(data);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (error: any) {
        await Swal.fire({
          icon: 'error',
          title: 'Error',
          text: error.message || 'No se pudo cargar el resumen del dashboard',
          confirmButtonColor: '#3b82f6',
        });
      } finally {
        setLoading(false);
      }
    }

    cargarResumen();
  }, []);

  if (loading) {
    return (
      <div className="p-6">
        <div className="text-gray-500">Cargando métricas...</div>
      </div>
    );
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
      {/* Banner de estado del plan */}
      <PlanBanner
        planActual={resumen.planActual}
        diasRestantes={resumen.diasRestantesSuscripcion}
        anunciosActivos={resumen.anunciosActivos ?? 0}
        totalAnuncios={resumen.totalAnuncios ?? 0}
      />

      {/* Fila 1: Métricas principales */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
        <MetricaCard
          titulo="Anuncios Activos"
          valor={resumen.anunciosActivos ?? 0}
          subtitulo={`De ${resumen.totalAnuncios ?? 0} totales`}
          color="bg-green-500"
        />
        <MetricaCard
          titulo="Borradores"
          valor={resumen.anunciosBorrador ?? 0}
          color="bg-gray-400"
        />
        <MetricaCard
          titulo="Vendidos"
          valor={resumen.anunciosVendidos ?? 0}
          color="bg-blue-500"
        />
        <MetricaCard
          titulo="Pausados"
          valor={resumen.anunciosPausados ?? 0}
          color="bg-yellow-500"
        />
      </div>

      {/* Fila 2: Leads y Suscripción */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-semibold text-gray-800">Leads</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-gray-600">Total:</span>
              <span className="font-bold text-gray-900">{resumen.totalLeads ?? 0}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">No leídos:</span>
              <span className="font-bold text-red-600">{resumen.leadsNoLeidos ?? 0}</span>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-semibold text-gray-800">Suscripción</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-gray-600">Plan actual:</span>
              <span className="font-bold text-gray-900">{resumen.planActual ?? 'N/A'}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Días restantes:</span>
              <span className={`font-bold ${(resumen.diasRestantesSuscripcion ?? 0) <= 7 ? 'text-red-600' : 'text-green-600'}`}>
                {resumen.diasRestantesSuscripcion ?? 0}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Fila 3: Anuncios más vistos */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-lg font-semibold text-gray-800">Anuncios más vistos</h3>
        {!resumen.anunciosMasVistos || resumen.anunciosMasVistos.length === 0 ? (
          <p className="text-gray-500">Aún no hay anuncios con vistas.</p>
        ) : (
          <ul className="space-y-2">
            {resumen.anunciosMasVistos.map((anuncio) => (
              <li
                key={anuncio.id}
                className="flex items-center justify-between rounded border border-gray-100 p-3 hover:bg-gray-50"
              >
                <span className="font-medium text-gray-800">{anuncio.nombreAnuncio}</span>
                <span className="rounded bg-blue-100 px-2 py-1 text-sm font-semibold text-blue-700">
                  {anuncio.vistas} vistas
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

// Componente reutilizable para las tarjetas de métricas
function MetricaCard({
  titulo,
  valor,
  subtitulo,
  color = 'bg-blue-500',
}: {
  titulo: string;
  valor: number;
  subtitulo?: string;
  color?: string;
}) {
  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600">{titulo}</p>
          <p className="mt-2 text-3xl font-bold text-gray-900">{valor}</p>
          {subtitulo && <p className="mt-1 text-xs text-gray-500">{subtitulo}</p>}
        </div>
        <div className={`h-12 w-12 rounded-full ${color} opacity-20`} />
      </div>
    </div>
  );
}

function PlanBanner({
  planActual,
  diasRestantes,
  anunciosActivos,
  totalAnuncios,
}: {
  planActual: string | null | undefined;
  diasRestantes: number | null | undefined;
  anunciosActivos: number;
  totalAnuncios: number;
}) {
  const esPlanGratis = planActual === "Gratis";
  const sinPlan = !planActual || planActual === "N/A";

  if (!esPlanGratis && !sinPlan) return null;

  const mensaje = sinPlan
    ? "Aún no tienes un plan configurado. Contacta a soporte para activar tu suscripción."
    : `Estás en el Plan Gratis (${anunciosActivos}/${totalAnuncios} anuncios activos) y vence en ${diasRestantes ?? 0} días.`;

  return (
    <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 p-4">
      <span className="mt-0.5 flex h-6 w-6 flex-shrink-0 items-center justify-center rounded-full bg-amber-400 text-sm font-bold text-white">
        !
      </span>
      <div>
        <p className="font-semibold text-amber-900">Plan {planActual || "no configurado"}</p>
        <p className="text-sm text-amber-800">{mensaje}</p>
      </div>
    </div>
  );
}