import { useEffect, useState } from 'react';

const MINIMO_IMAGENES = 5;
const MAXIMO_IMAGENES = 10;

interface ImagenPreviewProps {
  archivo: File;
  indice: number;
  onEliminar: (indice: number) => void;
}

function ImagenPreview({ archivo, indice, onEliminar }: ImagenPreviewProps) {
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
    <div className="group relative h-36 w-full overflow-hidden rounded-lg border border-gray-200 bg-gray-100">
      {previewUrl ? (
        <img 
          src={previewUrl} 
          alt={`Vista previa ${indice + 1}`} 
          className="absolute inset-0 h-full w-full object-cover" 
        />
      ) : (
        <div className="flex h-full items-center justify-center text-xs text-gray-400">
          Cargando...
        </div>
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
  onImageChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onEliminarArchivo: (indice: number) => void;
  onEliminarFotoGuardada: (indice: number) => void;
}

export default function GestorImagenes({
  archivos,
  fotosGuardadas,
  onImageChange,
  onEliminarArchivo,
  onEliminarFotoGuardada
}: GestorImagenesProps) {
  const totalImagenes = archivos.length + fotosGuardadas.length;

  return (
    <div className="border-t border-gray-100 pt-4">
      <div className="mb-2 flex items-center justify-between">
        <label className="block text-sm font-medium text-gray-700">
          Fotos del Vehículo
        </label>

        <span className="text-sm font-semibold text-gray-600">
          {totalImagenes}/{MAXIMO_IMAGENES}
        </span>
      </div>

      <input
        type="file"
        multiple
        accept="image/png, image/jpeg"
        onChange={onImageChange}
        className="block w-full cursor-pointer rounded-md border border-gray-300 text-sm text-gray-500 file:mr-4 file:rounded-md file:border-0 file:bg-blue-50 file:px-4 file:py-2.5 file:text-sm file:font-semibold file:text-blue-700 hover:file:bg-blue-100"
      />

      <p className="mt-1 text-xs text-gray-500">
        Agrega imágenes una por una o varias a la vez. Debes tener entre {MINIMO_IMAGENES} y {MAXIMO_IMAGENES} para guardar el anuncio.
      </p>

      {totalImagenes > 0 && (
        <div className="mt-4 grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
          {/* 1. Fotos viejas (S3) */}
          {fotosGuardadas.map((foto, indice) => (
            <div key={`old-${indice}`} className="group relative overflow-hidden rounded-lg border border-gray-200 bg-gray-50">
              <img src={foto} alt={`Guardada ${indice + 1}`} className="h-36 w-full object-cover" />
              
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
              onEliminar={onEliminarArchivo}
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