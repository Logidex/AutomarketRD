import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaArrowLeft, FaUpload, FaCheck } from 'react-icons/fa';
import Swal from 'sweetalert2';
import {
  useSlotsDisponibles,
  useSubirImagenAdSlot,
  useCrearAnuncioPublicitario,
} from '../../hooks/useAdSlots';
import type { AdSlot, AdSlotPublico, CrearAdSlotAnuncioDto } from '../../types/adslot.types';
import Spinner from '../ui/Spinner';

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

export default function CrearAnuncioPublicitario() {
  const navigate = useNavigate();
  const { data: slots, isLoading } = useSlotsDisponibles();
  const subirImagenMutation = useSubirImagenAdSlot();
  const crearAnuncioMutation = useCrearAnuncioPublicitario();

  const [paso, setPaso] = useState<1 | 2 | 3 | 4>(1);
  const [slotSeleccionado, setSlotSeleccionado] = useState<AdSlotPublico | null>(null);
  const [duracionSeleccionada, setDuracionSeleccionada] = useState<number | null>(null);
  const [imagenSubida, setImagenSubida] = useState<{ original: string; redimensionada: string } | null>(null);
  const [imagenFile, setImagenFile] = useState<File | null>(null);
  const [imagenPreview, setImagenPreview] = useState<string | null>(null);
  const [enlace, setEnlace] = useState('');
  const [titulo, setTitulo] = useState('');

  const handleSeleccionarSlot = (slot: AdSlotPublico) => {
    setSlotSeleccionado(slot);
    setPaso(2);
  };

  const handleSeleccionarDuracion = (dias: number) => {
    setDuracionSeleccionada(dias);
    setPaso(3);
  };

  const handleFileChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      Swal.fire('Error', 'La imagen no puede exceder 5 MB.', 'error');
      return;
    }

    const extensiones = ['image/jpeg', 'image/png', 'image/webp'];
    if (!extensiones.includes(file.type)) {
      Swal.fire('Error', 'Formato no permitido. Use JPG, PNG o WebP.', 'error');
      return;
    }

    setImagenFile(file);
    setImagenPreview(URL.createObjectURL(file));
  }, []);

  const handleSubirYCrear = async () => {
    if (!slotSeleccionado || !duracionSeleccionada || !imagenFile) return;

    try {
      // Subir imagen
      const resultado = await subirImagenMutation.mutateAsync({
        imagen: imagenFile,
        ubicacion: slotSeleccionado.ubicacion,
      });

      // Crear anuncio
      const dto: CrearAdSlotAnuncioDto = {
        adSlotId: slotSeleccionado.id,
        duracionDias: duracionSeleccionada,
        imagenUrl: resultado.imagenRedimensionada || resultado.imagenOriginal,
        enlace: enlace || undefined,
        titulo: titulo || undefined,
      };

      await crearAnuncioMutation.mutateAsync(dto);

      Swal.fire({
        title: 'Anuncio creado',
        text: 'Tu anuncio está activo y aparecerá en el sitio.',
        icon: 'success',
        confirmButtonColor: '#f59e0b',
      }).then(() => {
        navigate('/dashboard/ads');
      });
    } catch (error: any) {
      Swal.fire('Error', error.message || 'No se pudo crear el anuncio.', 'error');
    }
  };

  if (isLoading) return <Spinner />;

  const precioSeleccionado = slotSeleccionado?.precios.find(
    (p) => p.duracionDias === duracionSeleccionada
  );

  return (
    <div className="mx-auto max-w-2xl space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={() => {
            if (paso === 1) navigate('/dashboard/ads');
            else setPaso((p) => (p - 1) as 1 | 2 | 3 | 4);
          }}
          className="rounded-lg border border-line bg-surface p-2 text-ink-2 hover:bg-hover"
        >
          <FaArrowLeft className="h-4 w-4" />
        </button>
        <div>
          <h2 className="text-2xl font-bold text-ink">Crear anuncio publicitario</h2>
          <p className="text-sm text-ink-2">
            Paso {paso} de 4: {
              paso === 1 ? 'Selecciona ubicación' :
              paso === 2 ? 'Selecciona duración' :
              paso === 3 ? 'Sube tu imagen' :
              'Confirma y paga'
            }
          </p>
        </div>
      </div>

      {/* Barra de progreso */}
      <div className="flex gap-2">
        {[1, 2, 3, 4].map((p) => (
          <div
            key={p}
            className={`h-1.5 flex-1 rounded-full transition-colors ${
              p <= paso ? 'bg-amber-500' : 'bg-line'
            }`}
          />
        ))}
      </div>

      {/* Paso 1: Seleccionar ubicación */}
      {paso === 1 && (
        <div className="space-y-3">
          <h3 className="text-lg font-semibold text-ink">¿Dónde quieres tu anuncio?</h3>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {slots?.map((slot) => (
              <button
                key={slot.id}
                type="button"
                onClick={() => handleSeleccionarSlot(slot)}
                className="flex flex-col items-start rounded-xl border border-line bg-surface p-4 text-left transition-colors hover:border-amber-500 hover:bg-amber-500/5"
              >
                <span className="text-sm font-semibold text-ink">
                  {UBICACIONES_LABELS[slot.ubicacion] || slot.ubicacion}
                </span>
                <span className="mt-1 text-xs text-ink-3">
                  {slot.anchoPx}×{slot.altoPx}px • {slot.precios.length} duraciones
                </span>
                <span className="mt-2 text-sm font-bold text-amber-600">
                  Desde RD$ {slot.precios.length > 0 ? Math.min(...slot.precios.map((p) => p.precio)).toLocaleString() : '—'}
                </span>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Paso 2: Seleccionar duración */}
      {paso === 2 && slotSeleccionado && (
        <div className="space-y-3">
          <h3 className="text-lg font-semibold text-ink">
            Selecciona la duración — {UBICACIONES_LABELS[slotSeleccionado.ubicacion]}
          </h3>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            {slotSeleccionado.precios.filter((p) => p.activo).map((precio) => (
              <button
                key={precio.duracionDias}
                type="button"
                onClick={() => handleSeleccionarDuracion(precio.duracionDias)}
                className="flex flex-col items-center rounded-xl border border-line bg-surface p-6 text-center transition-colors hover:border-amber-500 hover:bg-amber-500/5"
              >
                <span className="text-3xl font-bold text-ink">
                  {precio.duracionDias}
                </span>
                <span className="text-xs text-ink-3">días</span>
                <span className="mt-3 text-lg font-bold text-amber-600">
                  RD$ {precio.precio.toLocaleString()}
                </span>
                {precio.descuentoProElitePorcentaje > 0 && (
                  <span className="mt-1 text-xs text-green-600">
                    {precio.descuentoProElitePorcentaje}% OFF para Pro/Elite
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Paso 3: Subir imagen */}
      {paso === 3 && slotSeleccionado && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-ink">Sube tu imagen</h3>
          <p className="text-sm text-ink-2">
            Dimensiones recomendadas: {slotSeleccionado.anchoPx}×{slotSeleccionado.altoPx}px.
            La imagen se redimensionará automáticamente.
          </p>

          <div className="rounded-xl border-2 border-dashed border-line bg-surface/50 p-8 text-center">
            {imagenPreview ? (
              <div className="space-y-4">
                <img
                  src={imagenPreview}
                  alt="Preview"
                  className="mx-auto max-h-48 rounded-lg object-cover"
                />
                <button
                  type="button"
                  onClick={() => {
                    setImagenFile(null);
                    setImagenPreview(null);
                  }}
                  className="text-sm text-red-500 hover:text-red-600"
                >
                  Cambiar imagen
                </button>
              </div>
            ) : (
              <label className="cursor-pointer">
                <FaUpload className="mx-auto text-4xl text-ink-3" />
                <p className="mt-2 text-sm text-ink-2">
                  Arrastra o haz click para seleccionar
                </p>
                <p className="mt-1 text-xs text-ink-3">
                  JPG, PNG o WebP • Máximo 5 MB
                </p>
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  className="hidden"
                  onChange={handleFileChange}
                />
              </label>
            )}
          </div>

          {/* Campos opcionales */}
          <div className="space-y-3">
            <div>
              <label className="mb-1 block text-sm font-medium text-ink">
                Título del anuncio (opcional)
              </label>
              <input
                type="text"
                value={titulo}
                onChange={(e) => setTitulo(e.target.value)}
                placeholder="Ej: Promoción especial de verano"
                className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-amber-500 focus:ring-2 focus:ring-amber-200"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-ink">
                Enlace destino (opcional)
              </label>
              <input
                type="url"
                value={enlace}
                onChange={(e) => setEnlace(e.target.value)}
                placeholder="https://tusitio.com/promo"
                className="w-full rounded-lg border border-line px-3 py-2 text-sm text-ink outline-none focus:border-amber-500 focus:ring-2 focus:ring-amber-200"
              />
            </div>
          </div>

          <button
            type="button"
            onClick={() => setPaso(4)}
            disabled={!imagenFile}
            className="w-full rounded-lg bg-amber-500 py-3 text-sm font-semibold text-white hover:bg-amber-600 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Continuar
          </button>
        </div>
      )}

      {/* Paso 4: Confirmar */}
      {paso === 4 && slotSeleccionado && precioSeleccionado && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-ink">Confirma tu anuncio</h3>

          <div className="rounded-xl border border-line bg-surface p-4">
            <div className="flex items-start gap-4">
              {imagenPreview && (
                <img
                  src={imagenPreview}
                  alt=""
                  className="h-20 w-20 rounded-lg object-cover"
                />
              )}
              <div className="flex-1">
                <p className="font-semibold text-ink">
                  {UBICACIONES_LABELS[slotSeleccionado.ubicacion]}
                </p>
                <p className="text-sm text-ink-2">
                  {duracionSeleccionada} días
                </p>
                {titulo && (
                  <p className="mt-1 text-sm text-ink-3">{titulo}</p>
                )}
              </div>
              <div className="text-right">
                <p className="text-2xl font-bold text-amber-600">
                  RD$ {precioSeleccionado.precio.toLocaleString()}
                </p>
                <p className="text-xs text-ink-3">
                  {precioSeleccionado.descuentoProElitePorcentaje > 0
                    ? `${precioSeleccionado.descuentoProElitePorcentaje}% OFF Pro/Elite`
                    : 'Precio base'}
                </p>
              </div>
            </div>
          </div>

          <p className="text-xs text-ink-3">
            Tu anuncio se activará inmediatamente después del pago. Podrás pagarlo
            con PayPal o transferencia bancaria.
          </p>

          <button
            type="button"
            onClick={handleSubirYCrear}
            disabled={subirImagenMutation.isPending || crearAnuncioMutation.isPending}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-amber-500 py-3 text-sm font-semibold text-white hover:bg-amber-600 disabled:opacity-50"
          >
            {subirImagenMutation.isPending || crearAnuncioMutation.isPending ? (
              <>Procesando...</>
            ) : (
              <>
                <FaCheck className="h-4 w-4" />
                Crear anuncio
              </>
            )}
          </button>
        </div>
      )}
    </div>
  );
}
