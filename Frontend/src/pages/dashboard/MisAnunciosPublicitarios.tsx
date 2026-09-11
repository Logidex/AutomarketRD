import { Link } from 'react-router-dom';
import { FaAd, FaPlus, FaEye, FaMousePointer, FaCalendarAlt, FaTrash } from 'react-icons/fa';
import Swal from 'sweetalert2';
import {
  useMisAnunciosPublicitarios,
  useEstadisticasAnuncios,
  useCancelarAnuncioPublicitario,
} from '../../hooks/useAdSlots';
import { urlImagen } from '../../utils/imagen';
import Spinner from '../../components/ui/Spinner';

const UBICACIONES_LABELS: Record<string, string> = {
  HomepageLateral: 'Homepage - Lateral',
  HomepageBuscador: 'Homepage - Buscador',
  HomepageFooter: 'Homepage - Footer',
  VehiculosLateral: 'Vehículos - Lateral',
  VehiculosGrid: 'Vehículos - Grid',
  VehiculosFooter: 'Vehículos - Footer',
  DetalleLateral: 'Detalle - Lateral',
  DetalleFooter: 'Detalle - Footer',
  AgenciasLateral: 'Agencias - Lateral',
  AgenciasFooter: 'Agencias - Footer',
};

export default function MisAnunciosPublicitarios() {
  const { data: anuncios, isLoading } = useMisAnunciosPublicitarios();
  const { data: stats } = useEstadisticasAnuncios();
  const cancelarMutation = useCancelarAnuncioPublicitario();

  const handleCancelar = (id: number) => {
    Swal.fire({
      title: '¿Cancelar anuncio?',
      text: 'Este anuncio dejará de mostrarse inmediatamente.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, cancelar',
      cancelButtonText: 'No',
    }).then((result) => {
      if (result.isConfirmed) {
        cancelarMutation.mutate(id, {
          onSuccess: () => Swal.fire('Cancelado', 'El anuncio ha sido cancelado.', 'success'),
        });
      }
    });
  };

  if (isLoading) return <Spinner />;

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-ink">Mis Anuncios Publicitarios</h2>
          <p className="text-sm text-ink-2">
            Gestiona tus anuncios en los espacios laterales del sitio.
          </p>
        </div>
        <Link
          to="/dashboard/ads/crear"
          className="flex items-center gap-2 rounded-lg bg-amber-500 px-4 py-2 text-sm font-semibold text-white hover:bg-amber-600"
        >
          <FaPlus className="h-4 w-4" />
          Crear anuncio
        </Link>
      </div>

      {/* Estadísticas resumen */}
      {stats && stats.totalImpresiones > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div className="rounded-lg border border-line bg-surface p-4">
            <div className="flex items-center gap-3">
              <FaEye className="h-5 w-5 text-blue-500" />
              <div>
                <p className="text-xs text-ink-3">Impresiones</p>
                <p className="text-lg font-bold text-ink">
                  {stats.totalImpresiones.toLocaleString()}
                </p>
              </div>
            </div>
          </div>
          <div className="rounded-lg border border-line bg-surface p-4">
            <div className="flex items-center gap-3">
              <FaMousePointer className="h-5 w-5 text-green-500" />
              <div>
                <p className="text-xs text-ink-3">Clicks</p>
                <p className="text-lg font-bold text-ink">
                  {stats.totalClicks.toLocaleString()}
                </p>
              </div>
            </div>
          </div>
          <div className="rounded-lg border border-line bg-surface p-4">
            <div className="flex items-center gap-3">
              <FaAd className="h-5 w-5 text-amber-500" />
              <div>
                <p className="text-xs text-ink-3">CTR Promedio</p>
                <p className="text-lg font-bold text-ink">
                  {stats.ctrPromedio}%
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Lista de anuncios */}
      {!anuncios || anuncios.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-line bg-surface/50 p-16 text-center">
          <FaAd className="mx-auto text-5xl text-ink-3" />
          <h3 className="mt-4 text-lg font-semibold text-ink">
            Sin anuncios publicitarios
          </h3>
          <p className="mt-2 text-sm text-ink-2">
            Crea tu primer anuncio para promocionar tu negocio en AutoMarket RD.
          </p>
          <Link
            to="/dashboard/ads/crear"
            className="mt-6 inline-flex items-center gap-2 rounded-lg bg-amber-500 px-6 py-3 text-sm font-semibold text-white hover:bg-amber-600"
          >
            <FaPlus className="h-4 w-4" />
            Crear mi primer anuncio
          </Link>
        </div>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
              <tr>
                <th className="px-4 py-3">Anuncio</th>
                <th className="px-4 py-3">Ubicación</th>
                <th className="px-4 py-3">Vence</th>
                <th className="px-4 py-3 text-right">Impresiones</th>
                <th className="px-4 py-3 text-right">Clicks</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {anuncios.map((anuncio) => (
                <tr key={anuncio.id} className="border-b border-line last:border-b-0">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <img
                        src={urlImagen(anuncio.imagenUrl)}
                        alt=""
                        className="h-12 w-12 rounded-lg object-cover"
                      />
                      <div>
                        <p className="font-medium text-ink">
                          {anuncio.titulo || 'Anuncio sin título'}
                        </p>
                        <p className="text-xs text-ink-3">{anuncio.nombreDealer}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {UBICACIONES_LABELS[anuncio.ubicacion] || anuncio.ubicacion}
                  </td>
                  <td className="px-4 py-3">
                    <span className="flex items-center gap-1 text-ink-2">
                      <FaCalendarAlt className="h-3 w-3" />
                      {new Date(anuncio.fechaFinUtc).toLocaleDateString('es-DO')}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right font-medium text-ink">
                    {anuncio.impresiones.toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-right font-medium text-ink">
                    {anuncio.clicks.toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      type="button"
                      onClick={() => handleCancelar(anuncio.id)}
                      className="rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 hover:bg-red-50"
                    >
                      <FaTrash className="inline h-3 w-3" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
