import React from 'react';
import { TIPOS_VEHICULO } from '../constants/vehiculo.opciones';
import InputNumerico from './InputNumerico';

const MAXIMO_TRANSMISION = 50;

export interface FormularioVehiculoData {
  marca: string;
  modelo: string;
  version: string;
  tipoVehiculo: string;
  motor: string;
  traccion: string;
  colorExterior: string;
  colorInterior: string;
  anio: number;
  precio: number;
  moneda: string;
  precioAnterior: number;
  kilometraje: number | string;
  transmision: string;
  combustible: string;
  ubicacion: string;
  descripcion: string;
}

export interface InformacionBasicaProps {
  formData: FormularioVehiculoData;
  kilometraje: string;
  onChange: (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => void;
  onKilometrajeChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export function InformacionBasica({
  formData,
  kilometraje,
  onChange,
  onKilometrajeChange,
}: InformacionBasicaProps) {
  const kilometrajeNumero = kilometraje.trim() === '' ? 0 : Number(kilometraje);
  const esNuevo = kilometrajeNumero <= 100;

  return (
    <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
      <div>
        <label
          htmlFor="marca"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Marca
        </label>
        <input
          id="marca"
          type="text"
          name="marca"
          required
          value={formData.marca}
          onChange={onChange}
          placeholder="Ej: Toyota"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="modelo"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Modelo
        </label>
        <input
          id="modelo"
          type="text"
          name="modelo"
          required
          value={formData.modelo}
          onChange={onChange}
          placeholder="Ej: Corolla"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="version"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Versión
        </label>
        <input
          id="version"
          type="text"
          name="version"
          required
          value={formData.version}
          onChange={onChange}
          placeholder="Ej: EX, Sport, Limited"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="anio"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Año
        </label>
        <input
          id="anio"
          type="number"
          name="anio"
          required
          min="1950"
          max={new Date().getFullYear() + 1}
          value={formData.anio}
          onChange={onChange}
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="precio"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Precio
        </label>
        <InputNumerico
          id="precio"
          nombre="precio"
          required
          minimo={1}
          value={formData.precio}
          onCambio={onChange}
          placeholder="0"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="moneda"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Moneda
        </label>
        <select
          id="moneda"
          name="moneda"
          required
          value={formData.moneda}
          onChange={onChange}
          className="w-full rounded-md border border-line bg-surface p-2.5 focus:border-blue-500 focus:ring-blue-500"
        >
          <option value="DOP">Peso dominicano (RD$)</option>
          <option value="USD">Dólar (US$)</option>
        </select>
      </div>

      <div>
        <label
          htmlFor="precioAnterior"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Precio anterior (opcional)
        </label>
        <InputNumerico
          id="precioAnterior"
          nombre="precioAnterior"
          minimo={0}
          value={formData.precioAnterior}
          onCambio={onChange}
          placeholder="Dejar vacío si no está en oferta"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
        <p className="mt-1 text-xs text-ink-3">
          Si es mayor que el precio actual, el vehículo se muestra como "En oferta".
        </p>
      </div>

      <div>
        <label
          htmlFor="kilometraje"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Kilometraje
        </label>
        <InputNumerico
          id="kilometraje"
          nombre="kilometraje"
          required
          minimo={0}
          value={kilometraje}
          onCambio={onKilometrajeChange}
          placeholder="Ej: 50000"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
        <p className="mt-1 text-xs text-ink-3">
          {kilometraje.trim() === ''
            ? 'Indica el kilometraje del vehículo.'
            : esNuevo
              ? 'Se mostrará como vehículo nuevo.'
              : 'Se mostrará como vehículo usado.'}
        </p>
      </div>
    </div>
  );
}

export interface EspecificacionesProps {
  formData: FormularioVehiculoData;
  mostrarTransmisionPersonalizada: boolean;
  transmisionPersonalizada: string;
  onChange: (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => void;
  onTransmisionPersonalizadaChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export function Especificaciones({
  formData,
  mostrarTransmisionPersonalizada,
  transmisionPersonalizada,
  onChange,
  onTransmisionPersonalizadaChange,
}: EspecificacionesProps) {
  return (
    <div className="grid grid-cols-1 gap-6 border-t border-line pt-4 md:grid-cols-3">
      <div>
        <label
          htmlFor="tipoVehiculo"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Tipo de Vehículo
        </label>
        <select
          id="tipoVehiculo"
          name="tipoVehiculo"
          required
          value={formData.tipoVehiculo}
          onChange={onChange}
          className="w-full rounded-md border border-line bg-surface p-2.5 focus:border-blue-500 focus:ring-blue-500"
        >
          <option value="">Selecciona...</option>
          {TIPOS_VEHICULO.map((opcion) => (
            <option key={opcion.valor} value={opcion.valor}>
              {opcion.etiqueta}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label
          htmlFor="motor"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Motor
        </label>
        <input
          id="motor"
          type="text"
          name="motor"
          required
          value={formData.motor}
          onChange={onChange}
          placeholder="Ej: 1.6L 4 cilindros"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="traccion"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Tracción
        </label>
        <select
          id="traccion"
          name="traccion"
          required
          value={formData.traccion}
          onChange={onChange}
          className="w-full rounded-md border border-line bg-surface p-2.5 focus:border-blue-500 focus:ring-blue-500"
        >
          <option value="">Selecciona...</option>
          <option value="Delantera">Delantera</option>
          <option value="Trasera">Trasera</option>
          <option value="AWD">AWD</option>
          <option value="4x4">4x4</option>
        </select>
      </div>

      <div>
        <label
          htmlFor="transmision"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Transmisión
        </label>
        <select
          id="transmision"
          name="transmision"
          required
          value={formData.transmision}
          onChange={onChange}
          className="w-full rounded-md border border-line bg-surface p-2.5 focus:border-blue-500 focus:ring-blue-500"
        >
          <option value="">Selecciona...</option>
          <option value="Automatica">Automática</option>
          <option value="Manual">Manual</option>
          <option value="Secuencial">Secuencial</option>
          <option value="CVT">CVT</option>
          <option value="DobleEmbrague">Doble Embrague (DCT)</option>
          <option value="Otra">Otra</option>
        </select>

        {mostrarTransmisionPersonalizada && (
          <div className="mt-2">
            <label
              htmlFor="transmisionPersonalizada"
              className="mb-1 block text-sm font-medium text-ink-2"
            >
              Transmisión personalizada
            </label>
            <input
              id="transmisionPersonalizada"
              type="text"
              required
              maxLength={MAXIMO_TRANSMISION}
              value={transmisionPersonalizada}
              onChange={onTransmisionPersonalizadaChange}
              placeholder="Escribe la transmisión"
              className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
            <p className="mt-1 text-xs text-ink-3">
              {transmisionPersonalizada.length}/{MAXIMO_TRANSMISION} caracteres
            </p>
            {transmisionPersonalizada.length >= MAXIMO_TRANSMISION && (
              <p className="mt-1 text-xs font-medium text-orange-600">
                Has alcanzado el límite máximo de {MAXIMO_TRANSMISION} caracteres.
              </p>
            )}
          </div>
        )}
      </div>

      <div>
        <label
          htmlFor="combustible"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Combustible
        </label>
        <select
          id="combustible"
          name="combustible"
          required
          value={formData.combustible}
          onChange={onChange}
          className="w-full rounded-md border border-line bg-surface p-2.5 focus:border-blue-500 focus:ring-blue-500"
        >
          <option value="">Selecciona...</option>
          <option value="Gasolina">Gasolina</option>
          <option value="Diesel">Diésel</option>
          <option value="Gas">GLP / Gas Natural</option>
          <option value="Electrico">Eléctrico</option>
          <option value="Hibrido">Híbrido</option>
        </select>
      </div>

      <div>
        <label
          htmlFor="colorExterior"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Color Exterior
        </label>
        <input
          id="colorExterior"
          type="text"
          name="colorExterior"
          required
          value={formData.colorExterior}
          onChange={onChange}
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="colorInterior"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Color Interior
        </label>
        <input
          id="colorInterior"
          type="text"
          name="colorInterior"
          required
          value={formData.colorInterior}
          onChange={onChange}
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>
    </div>
  );
}

export interface DetallesYUbicacionProps {
  formData: FormularioVehiculoData;
  accesoriosTexto: string;
  onChange: (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => void;
  onAccesoriosChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export function DetallesYUbicacion({
  formData,
  accesoriosTexto,
  onChange,
  onAccesoriosChange,
}: DetallesYUbicacionProps) {
  return (
    <div className="grid grid-cols-1 gap-6 border-t border-line pt-4 md:grid-cols-2">
      <div>
        <label
          htmlFor="ubicacion"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Ubicación
        </label>
        <input
          id="ubicacion"
          type="text"
          name="ubicacion"
          required
          value={formData.ubicacion}
          onChange={onChange}
          placeholder="Ej: Santo Domingo"
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
      </div>

      <div>
        <label
          htmlFor="accesorios"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Accesorios
        </label>
        <input
          id="accesorios"
          type="text"
          value={accesoriosTexto}
          onChange={onAccesoriosChange}
          placeholder="Sunroof, Cámara, Asientos en piel..."
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
        <p className="mt-1 text-xs text-ink-3">Separa los accesorios usando comas.</p>
      </div>

      <div className="md:col-span-2">
        <label
          htmlFor="descripcion"
          className="mb-1 block text-sm font-medium text-ink-2"
        >
          Descripción
        </label>
        <textarea
          id="descripcion"
          name="descripcion"
          rows={6}
          required
          maxLength={5000}
          value={formData.descripcion}
          onChange={onChange}
          placeholder="Detalles adicionales sobre el vehículo..."
          className="w-full rounded-md border border-line p-2.5 focus:border-blue-500 focus:ring-blue-500"
        />
        <p className="mt-1 text-xs text-ink-3">
          {formData.descripcion.length}/5000 caracteres
        </p>
      </div>
    </div>
  );
}

interface FormularioVehiculoProps {
  formData: FormularioVehiculoData;
  kilometraje: string;
  accesoriosTexto: string;
  mostrarTransmisionPersonalizada: boolean;
  transmisionPersonalizada: string;
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
}

export default function FormularioVehiculo({
  formData,
  kilometraje,
  accesoriosTexto,
  mostrarTransmisionPersonalizada,
  transmisionPersonalizada,
  publicarAlGuardar,
  onPublicarAlGuardarChange,
  destacarAlPublicar,
  onDestacarAlPublicarChange,
  mostrarDestacado = true,
  onChange,
  onKilometrajeChange,
  onAccesoriosChange,
  onTransmisionPersonalizadaChange,
}: FormularioVehiculoProps) {
  return (
    <div className="space-y-6">
      {/* INFORMACIÓN BÁSICA */}
      <InformacionBasica
        formData={formData}
        kilometraje={kilometraje}
        onChange={onChange}
        onKilometrajeChange={onKilometrajeChange}
      />

      {/* ESPECIFICACIONES */}
      <Especificaciones
        formData={formData}
        mostrarTransmisionPersonalizada={mostrarTransmisionPersonalizada}
        transmisionPersonalizada={transmisionPersonalizada}
        onChange={onChange}
        onTransmisionPersonalizadaChange={onTransmisionPersonalizadaChange}
      />

      {/* DETALLES Y UBICACIÓN */}
      <DetallesYUbicacion
        formData={formData}
        accesoriosTexto={accesoriosTexto}
        onChange={onChange}
        onAccesoriosChange={onAccesoriosChange}
      />

      {/* PUBLICAR AL GUARDAR */}
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

      {/* DESTACAR AL PUBLICAR */}
      {publicarAlGuardar && mostrarDestacado && (
        <div className="flex items-center gap-3 border-t border-amber-100 bg-amber-50/50 p-4 rounded-lg">
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
  );
}