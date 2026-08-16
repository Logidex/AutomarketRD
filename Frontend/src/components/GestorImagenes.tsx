import { useEffect, useState } from 'react';
import { urlImagen } from '../utils/imagen';

const MINIMO_IMAGENES = 5;

interface ImagenPreviewProps {
  archivo: File;
  indice: number;
  esPrincipal: boolean;
  onEliminar: (indice: number) => void;
  onEstablecerPrincipal: (indice: number) => void;
}

function ImagenPreview({
  archivo,
  indice,
  esPrincipal,
  onEliminar,
  onEstablecerPrincipal
}: ImagenPreviewProps) {
  const [previewUrl, setPreviewUrl] = useState<string>('');

  useEffect(() => {
    const url = URL.createObjectURL(archivo);
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setPreviewUrl(url);

    return () => {
      URL.revokeObjectURL(url);
    };
  }, [archivo]);

  return (
    <div className="group relative h-36 w-full overflow-hidden rounded-lg border-2 border-line bg-surface-2">
      {previewUrl ? (
        <img 
          src={previewUrl} 
          alt={`Vista previa ${indice + 1}`} 
          className="absolute inset-0 h-full w-full object-cover" 
        />
      ) : (
        <div className="flex h-full items-center justify-center text-xs text-ink-3">
          Cargando...
        </div>
      )}

      <button
        type="button"
        onClick={() => onEstablecerPrincipal(indice)}
        title={esPrincipal ? "Este es la foto principal" : "Establecer como foto principal"}
        className={`absolute left-2 top-2 z-10 flex h-8 w-8 items-center justify-center rounded-full text-lg transition-colors ${
          esPrincipal
            ? "bg-amber-400 text-white"
            : "bg-black/50 text-white opacity-0 hover:bg-amber-400 group-hover:opacity-100"
        }`}
      >
        ★
      </button>

      {esPrincipal && (
        <span className="absolute left-2 top-11 z-10 rounded bg-amber-400 px-1.5 py-0.5 text-[10px] font-bold uppercase text-white">
          Principal
        </span>
      )}

      <button
        type="button"
        onClick={() => onEliminar(indice)}
        title="Eliminar imagen"
        className="absolute right-2 top-2 z-10 flex h-8 w-8 items-center justify-center rounded-full bg-red-600 text-lg font-bold text-white opacity-0 transition-opacity hover:bg-red-700 group-hover:opacity-100"
      >
        ×
      </button>

      <div className="absolute inset-x-0 bottom-0 bg-black/50 px-2 py-1 text-xs text-white truncate">
        {archivo.name}
      </div>
    </div>
  );
}

interface GestorImagenesProps {
  archivos: File[];
  fotosGuardadas: string[];
  fotoPrincipal: File | string | null;
  maxImagenes?: number;
  onImageChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onEliminarArchivo: (indice: number) => void;
  onEliminarFotoGuardada: (indice: number) => void;
  onEstablecerPrincipal: (tipo: "archivo" | "guardada", indice: number) => void;
}

export default function GestorImagenes({
  archivos,
  fotosGuardadas,
  fotoPrincipal,
  maxImagenes = 10,
  onImageChange,
  onEliminarArchivo,
  onEliminarFotoGuardada,
  onEstablecerPrincipal,
}: GestorImagenesProps) {
  const totalImagenes = archivos.length + fotosGuardadas.length;

  return (
    <div className="border-t border-line pt-4">
      <div className="mb-2 flex items-center justify-between">
        <label
          htmlFor="fotosVehiculo"
          className="block text-sm font-medium text-ink-2"
        >
          Fotos del Vehículo
        </label>

        <span className="text-sm font-semibold text-ink-2">
          {totalImagenes}/{maxImagenes}
        </span>
      </div>

      <input
        id="fotosVehiculo"
        type="file"
        multiple
        accept="image/png, image/jpeg"
        onChange={onImageChange}
        className="block w-full cursor-pointer rounded-md border border-line text-sm text-ink-3 file:mr-4 file:rounded-md file:border-0 file:bg-blue-50 file:px-4 file:py-2.5 file:text-sm file:font-semibold file:text-blue-700 hover:file:bg-blue-100"
      />

      <p className="mt-1 text-xs text-ink-3">
        Agrega imágenes una por una o varias a la vez. Haz clic en la estrella (★) de una foto para elegir la principal.
        Debes tener entre {MINIMO_IMAGENES} y {maxImagenes} para guardar el anuncio.
      </p>

      {totalImagenes > 0 && (
        <div className="mt-4 grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
          {/* 1. Fotos viejas (S3) */}
          {fotosGuardadas.map((foto, indice) => (
            <div
              key={`old-${indice}`}
              className={`group relative overflow-hidden rounded-lg border-2 bg-surface-2 ${
                fotoPrincipal === foto ? "border-amber-400" : "border-line"
              }`}
            >
              <img src={urlImagen(foto)} alt={`Guardada ${indice + 1}`} className="h-36 w-full object-cover" />
              
              <button
                type="button"
                onClick={() => onEstablecerPrincipal("guardada", indice)}
                title={fotoPrincipal === foto ? "Este es la foto principal" : "Establecer como foto principal"}
                className={`absolute left-2 top-2 z-10 flex h-8 w-8 items-center justify-center rounded-full text-lg transition-colors ${
                  fotoPrincipal === foto
                    ? "bg-amber-400 text-white"
                    : "bg-black/50 text-white opacity-0 hover:bg-amber-400 group-hover:opacity-100"
                }`}
              >
                ★
              </button>

              {fotoPrincipal === foto && (
                <span className="absolute left-2 top-11 z-10 rounded bg-amber-400 px-1.5 py-0.5 text-[10px] font-bold uppercase text-white">
                  Principal
                </span>
              )}

              <button
                type="button"
                onClick={() => onEliminarFotoGuardada(indice)}
                title="Eliminar imagen"
                className="absolute right-2 top-2 flex h-8 w-8 items-center justify-center rounded-full bg-red-600 text-lg font-bold text-white opacity-0 transition-opacity hover:bg-red-700 group-hover:opacity-100"
              >
                ×
              </button>
            </div>
          ))}

          {/* 2. Fotos nuevas (PC) */}
          {archivos.map((archivo, indice) => (
            <ImagenPreview
              key={`${archivo.name}-${archivo.size}`}
              archivo={archivo}
              indice={indice}
              esPrincipal={fotoPrincipal === archivo}
              onEliminar={onEliminarArchivo}
              onEstablecerPrincipal={() => onEstablecerPrincipal("archivo", indice)}
            />
          ))}
        </div>
      )}

      {totalImagenes > 0 && totalImagenes < MINIMO_IMAGENES && (
        <p className="mt-3 text-sm font-medium text-orange-600">
          Te faltan {MINIMO_IMAGENES - totalImagenes} imagen(es) para poder guardar el anuncio.
        </p>
      )}

      {totalImagenes >= MINIMO_IMAGENES && (
        <p className="mt-3 text-sm font-medium text-green-600">
          Tienes suficientes imágenes para guardar el anuncio.
        </p>
      )}
    </div>
  );
}