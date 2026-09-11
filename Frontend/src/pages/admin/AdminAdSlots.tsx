import { useState } from 'react';
import { FaPlus, FaEdit, FaTrash } from 'react-icons/fa';
import Swal from 'sweetalert2';
import {
  useAdminAdSlots,
  useCrearAdSlot,
  useActualizarAdSlot,
  useEliminarAdSlot,
  useAdminAdSlotAnuncios,
  useRechazarAnuncioAdSlot,
} from '../../hooks/useAdSlots';
import type {
  AdSlot,
  CrearAdSlotAdminDto,
  ActualizarAdSlotAdminDto,
  UbicacionAdSlot,
} from '../../types/adslot.types';
import Spinner from '../../components/ui/Spinner';

const UBICACIONES: { valor: UbicacionAdSlot; etiqueta: string }[] = [
  { valor: 'HomepageLateral', etiqueta: 'Homepage - Lateral' },
  { valor: 'HomepageBuscador', etiqueta: 'Homepage - Buscador' },
  { valor: 'HomepageFooter', etiqueta: 'Homepage - Footer' },
  { valor: 'VehiculosLateral', etiqueta: 'Vehículos - Lateral' },
  { valor: 'VehiculosGrid', etiqueta: 'Vehículos - Grid' },
  { valor: 'VehiculosFooter', etiqueta: 'Vehículos - Footer' },
  { valor: 'DetalleLateral', etiqueta: 'Detalle - Lateral' },
  { valor: 'DetalleFooter', etiqueta: 'Detalle - Footer' },
  { valor: 'AgenciasLateral', etiqueta: 'Agencias - Lateral' },
  { valor: 'AgenciasFooter', etiqueta: 'Agencias - Footer' },
];

const UBICACIONES_MAP = Object.fromEntries(UBICACIONES.map((u) => [u.valor, u.etiqueta]));

const SLOT_VACIO: CrearAdSlotAdminDto = {
  titulo: '',
  ubicacion: 'HomepageLateral',
  anchoPx: 300,
  altoPx: 250,
  intervaloRotacionSeg: 5,
  maxAnunciosSimultaneos: 3,
  activo: true,
  orden: 0,
  precios: [
    { duracionDias: 7, precio: 500, descuentoProElitePorcentaje: 10 },
    { duracionDias: 30, precio: 1500, descuentoProElitePorcentaje: 15 },
    { duracionDias: 90, precio: 3500, descuentoProElitePorcentaje: 15 },
  ],
};

export default function AdminAdSlots() {
  const { data: slots, isLoading: cargandoSlots } = useAdminAdSlots();
  const { data: anuncios } = useAdminAdSlotAnuncios();
  const crearMutation = useCrearAdSlot();
  const actualizarMutation = useActualizarAdSlot();
  const eliminarMutation = useEliminarAdSlot();
  const rechazarMutation = useRechazarAnuncioAdSlot();

  const [modalAbierto, setModalAbierto] = useState(false);
  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [form, setForm] = useState<CrearAdSlotAdminDto>(SLOT_VACIO);
  const [guardando, setGuardando] = useState(false);
  const [tab, setTab] = useState<'slots' | 'anuncios'>('slots');

  const abrirNuevo = () => {
    setEditandoId(null);
    setForm({ ...SLOT_VACIO, precios: [...SLOT_VACIO.precios] });
    setModalAbierto(true);
  };

  const abrirEditar = (slot: AdSlot) => {
    setEditandoId(slot.id);
    setForm({
      titulo: slot.titulo,
      ubicacion: slot.ubicacion,
      anchoPx: slot.anchoPx,
      altoPx: slot.altoPx,
      intervaloRotacionSeg: slot.intervaloRotacionSeg,
      maxAnunciosSimultaneos: slot.maxAnunciosSimultaneos,
      activo: slot.activo,
      orden: slot.orden,
      precios: slot.precios.map((p) => ({
        duracionDias: p.duracionDias,
        precio: p.precio,
        descuentoProElitePorcentaje: p.descuentoProElitePorcentaje,
      })),
    });
    setModalAbierto(true);
  };

  const guardar = async () => {
    if (!form.titulo.trim()) {
      Swal.fire('Error', 'El título es obligatorio.', 'error');
      return;
    }

    setGuardando(true);
    try {
      if (editandoId) {
        const dto: ActualizarAdSlotAdminDto = {
          titulo: form.titulo,
          ubicacion: form.ubicacion,
          anchoPx: form.anchoPx,
          altoPx: form.altoPx,
          intervaloRotacionSeg: form.intervaloRotacionSeg,
          maxAnunciosSimultaneos: form.maxAnunciosSimultaneos,
          activo: form.activo,
          orden: form.orden,
        };
        await actualizarMutation.mutateAsync({ id: editandoId, dto });
        Swal.fire('Actualizado', 'Slot actualizado correctamente.', 'success');
      } else {
        await crearMutation.mutateAsync(form);
        Swal.fire('Creado', 'Slot creado correctamente.', 'success');
      }
      setModalAbierto(false);
    } catch (error: unknown) {
      const msg = error instanceof Error ? error.message : 'No se pudo guardar.';
      Swal.fire('Error', msg, 'error');
    } finally {
      setGuardando(false);
    }
  };

  const eliminar = (id: number) => {
    Swal.fire({
      title: '¿Desactivar slot?',
      text: 'El slot dejará de estar disponible.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, desactivar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        eliminarMutation.mutate(id, {
          onSuccess: () => Swal.fire('Desactivado', 'Slot desactivado.', 'success'),
        });
      }
    });
  };

  const rechazarAnuncio = (id: number) => {
    Swal.fire({
      title: '¿Rechazar anuncio?',
      text: 'El anuncio dejará de mostrarse.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'Rechazar',
    }).then((result) => {
      if (result.isConfirmed) {
        rechazarMutation.mutate(id, {
          onSuccess: () => Swal.fire('Rechazado', 'Anuncio rechazado.', 'success'),
        });
      }
    });
  };

  if (cargandoSlots) return <Spinner />;

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-ink">Ad Slots</h2>
        <button
          type="button"
          onClick={abrirNuevo}
          className="flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-semibold text-white hover:bg-violet-700"
        >
          <FaPlus className="h-4 w-4" />
          Nuevo slot
        </button>
      </div>

      {/* Tabs */}
      <div className="flex gap-2">
        <button
          type="button"
          onClick={() => setTab('slots')}
          className={`rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
            tab === 'slots'
              ? 'bg-violet-600 text-white'
              : 'bg-surface text-ink-2 hover:bg-hover'
          }`}
        >
          Slots ({slots?.length ?? 0})
        </button>
        <button
          type="button"
          onClick={() => setTab('anuncios')}
          className={`rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
            tab === 'anuncios'
              ? 'bg-violet-600 text-white'
              : 'bg-surface text-ink-2 hover:bg-hover'
          }`}
        >
          Anuncios ({anuncios?.length ?? 0})
        </button>
      </div>

      {/* Tab: Slots */}
      {tab === 'slots' && (
        <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
              <tr>
                <th className="px-4 py-3">Título</th>
                <th className="px-4 py-3">Ubicación</th>
                <th className="px-4 py-3">Dimensiones</th>
                <th className="px-4 py-3">Rotación</th>
                <th className="px-4 py-3">Anuncios</th>
                <th className="px-4 py-3">Estado</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {slots?.map((slot) => (
                <tr key={slot.id} className="border-b border-line last:border-b-0">
                  <td className="px-4 py-3 font-medium text-ink">{slot.titulo}</td>
                  <td className="px-4 py-3 text-ink-2">
                    {UBICACIONES_MAP[slot.ubicacion] || slot.ubicacion}
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {slot.anchoPx}×{slot.altoPx}px
                  </td>
                  <td className="px-4 py-3 text-ink-2">{slot.intervaloRotacionSeg}s</td>
                  <td className="px-4 py-3 text-ink-2">{slot.anunciosActivosCount}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-semibold ${
                        slot.activo
                          ? 'bg-green-100 text-green-700'
                          : 'bg-red-100 text-red-700'
                      }`}
                    >
                      {slot.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        type="button"
                        onClick={() => abrirEditar(slot)}
                        className="rounded-lg border border-line bg-surface px-3 py-1.5 text-xs font-semibold text-ink hover:bg-hover"
                      >
                        <FaEdit className="inline h-3 w-3" />
                      </button>
                      <button
                        type="button"
                        onClick={() => eliminar(slot.id)}
                        className="rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 hover:bg-red-50"
                      >
                        <FaTrash className="inline h-3 w-3" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Tab: Anuncios */}
      {tab === 'anuncios' && (
        <div className="overflow-x-auto rounded-lg border border-line bg-surface shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-line bg-surface-2 text-xs uppercase text-ink-3">
              <tr>
                <th className="px-4 py-3">Dealer</th>
                <th className="px-4 py-3">Slot</th>
                <th className="px-4 py-3">Vence</th>
                <th className="px-4 py-3 text-right">Impresiones</th>
                <th className="px-4 py-3 text-right">Clicks</th>
                <th className="px-4 py-3">Estado</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {anuncios?.map((anuncio) => (
                <tr key={anuncio.id} className="border-b border-line last:border-b-0">
                  <td className="px-4 py-3 font-medium text-ink">
                    {anuncio.nombreDealer}
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {UBICACIONES_MAP[anuncio.ubicacion] || anuncio.ubicacion}
                  </td>
                  <td className="px-4 py-3 text-ink-2">
                    {new Date(anuncio.fechaFinUtc).toLocaleDateString('es-DO')}
                  </td>
                  <td className="px-4 py-3 text-right text-ink">
                    {anuncio.impresiones.toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-right text-ink">
                    {anuncio.clicks.toLocaleString()}
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-semibold ${
                        anuncio.estado === 'Activo'
                          ? 'bg-green-100 text-green-700'
                          : anuncio.estado === 'Vencido'
                          ? 'bg-gray-100 text-gray-700'
                          : 'bg-red-100 text-red-700'
                      }`}
                    >
                      {anuncio.estado}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    {anuncio.estado === 'Activo' && (
                      <button
                        type="button"
                        onClick={() => rechazarAnuncio(anuncio.id)}
                        className="rounded-lg border border-red-200 bg-surface px-3 py-1.5 text-xs font-semibold text-red-700 hover:bg-red-50"
                      >
                        Rechazar
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal */}
      {modalAbierto && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-black/40"
            onClick={() => setModalAbierto(false)}
          />
          <div className="relative w-full max-w-lg rounded-xl bg-surface p-6 shadow-2xl">
            <h3 className="mb-4 text-lg font-bold text-ink">
              {editandoId ? 'Editar slot' : 'Nuevo slot'}
            </h3>

            <div className="max-h-[60vh] space-y-4 overflow-y-auto">
              <div>
                <label className="mb-1 block text-sm font-medium text-ink">Título</label>
                <input
                  type="text"
                  value={form.titulo}
                  onChange={(e) => setForm({ ...form, titulo: e.target.value })}
                  className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-200"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-medium text-ink">Ubicación</label>
                <select
                  value={form.ubicacion}
                  onChange={(e) => setForm({ ...form, ubicacion: e.target.value as UbicacionAdSlot })}
                  className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-violet-500"
                >
                  {UBICACIONES.map((u) => (
                    <option key={u.valor} value={u.valor}>
                      {u.etiqueta}
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="mb-1 block text-sm font-medium text-ink">Ancho (px)</label>
                  <input
                    type="number"
                    value={form.anchoPx}
                    onChange={(e) => setForm({ ...form, anchoPx: +e.target.value })}
                    className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-violet-500"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-sm font-medium text-ink">Alto (px)</label>
                  <input
                    type="number"
                    value={form.altoPx}
                    onChange={(e) => setForm({ ...form, altoPx: +e.target.value })}
                    className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-violet-500"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="mb-1 block text-sm font-medium text-ink">
                    Rotación (seg)
                  </label>
                  <input
                    type="number"
                    value={form.intervaloRotacionSeg}
                    onChange={(e) =>
                      setForm({ ...form, intervaloRotacionSeg: +e.target.value })
                    }
                    className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-violet-500"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-sm font-medium text-ink">
                    Max. anuncios
                  </label>
                  <input
                    type="number"
                    value={form.maxAnunciosSimultaneos}
                    onChange={(e) =>
                      setForm({ ...form, maxAnunciosSimultaneos: +e.target.value })
                    }
                    className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-violet-500"
                  />
                </div>
              </div>
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setModalAbierto(false)}
                className="rounded-lg border border-line bg-surface px-4 py-2 text-sm font-semibold text-ink-2 hover:bg-hover"
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={guardar}
                disabled={guardando}
                className="flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-semibold text-white hover:bg-violet-700 disabled:opacity-50"
              >
                {guardando ? 'Guardando...' : 'Guardar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
