import { useRef, useState } from "react";
import {
  InformacionBasica,
  Especificaciones,
  DetallesYUbicacion,
  type FormularioVehiculoData,
} from "./FormularioVehiculo";
import GestorImagenes from "./GestorImagenes";
import { formatearPrecio, parsearNumeroInput } from "../utils/formato";

const MINIMO_IMAGENES = 5;

const PASOS = [
  "Información básica",
  "Especificaciones",
  "Detalles y ubicación",
  "Fotos y publicación",
  "Revisar y publicar",
] as const;

interface FormularioVehiculoWizardProps {
  formData: FormularioVehiculoData;
  kilometraje: string;
  accesoriosTexto: string;
  mostrarTransmisionPersonalizada: boolean;
  transmisionPersonalizada: string;
  archivos: File[];
  fotosGuardadas: string[];
  fotoPrincipal: File | string | null;
  maxImagenes: number;
  publicarAlGuardar: boolean;
  onPublicarAlGuardarChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  destacarAlPublicar: boolean;
  onDestacarAlPublicarChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  mostrarDestacado?: boolean;
  onChange: (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => void;
  onKilometrajeChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onAccesoriosChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onTransmisionPersonalizadaChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onImageChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onEliminarArchivo: (indice: number) => void;
  onEliminarFotoGuardada: (indice: number) => void;
  onEstablecerPrincipal: (tipo: "archivo" | "guardada", indice: number) => void;
  submitting: boolean;
  tituloBoton: string;
  textoBotonSubmitting?: string;
  onCancelar?: () => void;
  onSubmit: (e: React.FormEvent) => void;
}

function FilaResumen({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-line/60 py-2 last:border-b-0">
      <dt className="shrink-0 text-sm text-ink-3">{etiqueta}</dt>
      <dd className="text-right text-sm font-medium text-ink">{valor || "—"}</dd>
    </div>
  );
}

export default function FormularioVehiculoWizard({
  formData,
  kilometraje,
  accesoriosTexto,
  mostrarTransmisionPersonalizada,
  transmisionPersonalizada,
  archivos,
  fotosGuardadas,
  fotoPrincipal,
  maxImagenes,
  publicarAlGuardar,
  onPublicarAlGuardarChange,
  destacarAlPublicar,
  onDestacarAlPublicarChange,
  mostrarDestacado = true,
  onChange,
  onKilometrajeChange,
  onAccesoriosChange,
  onTransmisionPersonalizadaChange,
  onImageChange,
  onEliminarArchivo,
  onEliminarFotoGuardada,
  onEstablecerPrincipal,
  submitting,
  tituloBoton,
  textoBotonSubmitting = "Guardando...",
  onCancelar,
  onSubmit,
}: FormularioVehiculoWizardProps) {
  const formRef = useRef<HTMLFormElement>(null);
  const [paso, setPaso] = useState(1);

  const totalImagenes = archivos.length + fotosGuardadas.length;
  const transmisionMostrada = mostrarTransmisionPersonalizada
    ? transmisionPersonalizada
    : formData.transmision;

  const moverPaso = (nuevo: number) => {
    setPaso(nuevo);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const irSiguiente = () => {
    if (formRef.current && !formRef.current.reportValidity()) return;
    moverPaso(Math.min(paso + 1, PASOS.length));
  };

  const irAnterior = () => {
    moverPaso(Math.max(paso - 1, 1));
  };

  return (
    <form ref={formRef} onSubmit={onSubmit} className="space-y-6">
      {/* Stepper */}
      <ol className="flex items-center gap-1 overflow-x-auto pb-1 sm:gap-2">
        {PASOS.map((etiqueta, indice) => {
          const numero = indice + 1;
          const activo = numero === paso;
          const completado = numero < paso;
          const clickeable = completado;
          return (
            <li key={etiqueta} className="flex items-center gap-1 sm:gap-2">
              <button
                type="button"
                onClick={() => clickeable && moverPaso(numero)}
                disabled={!clickeable}
                className={`flex items-center gap-2 rounded-full px-2 py-1 text-xs font-medium transition-colors sm:text-sm ${
                  activo
                    ? "bg-blue-50 text-blue-700"
                    : completado
                      ? "text-blue-600 hover:bg-hover"
                      : "text-ink-3"
                }`}
              >
                <span
                  className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-xs font-bold ${
                    activo
                      ? "bg-blue-600 text-white"
                      : completado
                        ? "bg-blue-100 text-blue-700"
                        : "bg-surface-2 text-ink-3"
                  }`}
                >
                  {completado ? "✓" : numero}
                </span>
                <span className="whitespace-nowrap">{etiqueta}</span>
              </button>
              {numero < PASOS.length && (
                <div
                  className={`h-0.5 w-4 sm:w-6 ${numero < paso ? "bg-blue-400" : "bg-line"}`}
                />
              )}
            </li>
          );
        })}
      </ol>

      {/* Paso 1: Información básica */}
      {paso === 1 && (
        <div className="rounded-lg border border-line bg-surface p-5 sm:p-6">
          <h3 className="mb-4 text-sm font-semibold text-ink">
            Información básica
          </h3>
          <InformacionBasica
            formData={formData}
            kilometraje={kilometraje}
            onChange={onChange}
            onKilometrajeChange={onKilometrajeChange}
          />
        </div>
      )}

      {/* Paso 2: Especificaciones */}
      {paso === 2 && (
        <div className="rounded-lg border border-line bg-surface p-5 sm:p-6">
          <h3 className="mb-4 text-sm font-semibold text-ink">
            Especificaciones
          </h3>
          <Especificaciones
            formData={formData}
            mostrarTransmisionPersonalizada={mostrarTransmisionPersonalizada}
            transmisionPersonalizada={transmisionPersonalizada}
            onChange={onChange}
            onTransmisionPersonalizadaChange={onTransmisionPersonalizadaChange}
          />
        </div>
      )}

      {/* Paso 3: Detalles y ubicación */}
      {paso === 3 && (
        <div className="rounded-lg border border-line bg-surface p-5 sm:p-6">
          <h3 className="mb-4 text-sm font-semibold text-ink">
            Detalles y ubicación
          </h3>
          <DetallesYUbicacion
            formData={formData}
            accesoriosTexto={accesoriosTexto}
            onChange={onChange}
            onAccesoriosChange={onAccesoriosChange}
          />
        </div>
      )}

      {/* Paso 4: Fotos y publicación */}
      {paso === 4 && (
        <div className="rounded-lg border border-line bg-surface p-5 sm:p-6">
          <h3 className="mb-4 text-sm font-semibold text-ink">
            Fotos y publicación
          </h3>
          <div className="space-y-6">
            <GestorImagenes
              archivos={archivos}
              fotosGuardadas={fotosGuardadas}
              fotoPrincipal={fotoPrincipal}
              maxImagenes={maxImagenes}
              onImageChange={onImageChange}
              onEliminarArchivo={onEliminarArchivo}
              onEliminarFotoGuardada={onEliminarFotoGuardada}
              onEstablecerPrincipal={onEstablecerPrincipal}
            />

            <div className="flex items-center gap-3 border-t border-line pt-4">
              <input
                id="publicarAlGuardar"
                type="checkbox"
                checked={publicarAlGuardar}
                onChange={onPublicarAlGuardarChange}
                className="h-4 w-4 rounded border-line text-blue-600 focus:ring-blue-500"
              />
              <label
                htmlFor="publicarAlGuardar"
                className="text-sm font-medium text-ink-2"
              >
                Publicar al guardar
              </label>
              <span className="text-xs text-ink-3">
                El vehículo aparecerá de inmediato en la vitrina pública.
              </span>
            </div>

            {publicarAlGuardar && mostrarDestacado && (
              <div className="flex items-center gap-3 rounded-lg border border-amber-100 bg-amber-50/50 p-4">
                <input
                  id="destacarAlPublicar"
                  type="checkbox"
                  checked={destacarAlPublicar}
                  onChange={onDestacarAlPublicarChange}
                  className="h-4 w-4 rounded border-amber-300 text-amber-500 focus:ring-amber-400"
                />
                <div>
                  <label
                    htmlFor="destacarAlPublicar"
                    className="text-sm font-medium text-ink"
                  >
                    Destacar este anuncio
                  </label>
                  <p className="text-xs text-ink-3">
                    Aparecerá en la sección de destacados de la página principal (usa una
                    cuota de tu plan). Si la cuota está llena, el anuncio se publica igual
                    sin destacar.
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Paso 5: Revisar y publicar */}
      {paso === 5 && (
        <div className="rounded-lg border border-line bg-surface p-5 sm:p-6">
          <h3 className="mb-4 text-sm font-semibold text-ink">
            Revisar y publicar
          </h3>

          <dl className="rounded-lg border border-line bg-surface-2 px-4 py-2">
            <FilaResumen
              etiqueta="Vehículo"
              valor={`${formData.marca} ${formData.modelo} ${formData.version}`.trim()}
            />
            <FilaResumen etiqueta="Año" valor={formData.anio ? String(formData.anio) : ""} />
            <FilaResumen
              etiqueta="Precio"
              valor={formatearPrecio(formData.precio, formData.moneda)}
            />
            {formData.precioAnterior > 0 && (
              <FilaResumen
                etiqueta="Precio anterior"
                valor={formatearPrecio(formData.precioAnterior, formData.moneda)}
              />
            )}
            <FilaResumen
              etiqueta="Kilometraje"
              valor={`${parsearNumeroInput(kilometraje).toLocaleString("es-DO")} km`}
            />
            <FilaResumen etiqueta="Tipo de vehículo" valor={formData.tipoVehiculo} />
            <FilaResumen etiqueta="Motor" valor={formData.motor} />
            <FilaResumen etiqueta="Tracción" valor={formData.traccion} />
            <FilaResumen etiqueta="Transmisión" valor={transmisionMostrada} />
            <FilaResumen etiqueta="Combustible" valor={formData.combustible} />
            <FilaResumen
              etiqueta="Colores"
              valor={[formData.colorExterior, formData.colorInterior]
                .filter(Boolean)
                .join(" / ")}
            />
            <FilaResumen etiqueta="Ubicación" valor={formData.ubicacion} />
            <FilaResumen etiqueta="Accesorios" valor={accesoriosTexto} />
            <FilaResumen
              etiqueta="Fotos"
              valor={`${totalImagenes} de ${maxImagenes}`}
            />
            <FilaResumen
              etiqueta="Publicar al guardar"
              valor={publicarAlGuardar ? "Sí" : "No"}
            />
            {mostrarDestacado && (
              <FilaResumen
                etiqueta="Destacar anuncio"
                valor={publicarAlGuardar && destacarAlPublicar ? "Sí" : "No"}
              />
            )}
          </dl>

          {totalImagenes < MINIMO_IMAGENES && (
            <p className="mt-4 text-sm font-medium text-orange-600">
              Te faltan {MINIMO_IMAGENES - totalImagenes} foto(s) para poder
              publicar el anuncio.
            </p>
          )}
        </div>
      )}

      {/* Navegación */}
      <div className="flex items-center justify-between gap-4 border-t border-line pt-6">
        <div>
          {onCancelar && (
            <button
              type="button"
              onClick={onCancelar}
              disabled={submitting}
              className="rounded-md border border-line px-6 py-2.5 font-medium text-ink-2 transition-colors hover:bg-hover disabled:cursor-not-allowed disabled:opacity-50"
            >
              Cancelar
            </button>
          )}
        </div>

        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={irAnterior}
            disabled={paso === 1 || submitting}
            className="rounded-md border border-line px-6 py-2.5 font-medium text-ink-2 transition-colors hover:bg-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            Anterior
          </button>

          {paso < PASOS.length ? (
            <button
              type="button"
              onClick={irSiguiente}
              disabled={submitting}
              className="rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-400"
            >
              Siguiente
            </button>
          ) : (
            <button
              type="submit"
              disabled={submitting}
              className="rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-400"
            >
              {submitting ? textoBotonSubmitting : tituloBoton}
            </button>
          )}
        </div>
      </div>
    </form>
  );
}