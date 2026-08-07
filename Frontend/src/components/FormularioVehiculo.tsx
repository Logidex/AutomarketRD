import React from 'react';

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
  kilometraje: number | string;
  transmision: string;
  combustible: string;
  ubicacion: string;
  descripcion: string;
}

interface FormularioVehiculoProps {
  formData: FormularioVehiculoData;
  kilometraje: string;
  accesoriosTexto: string;
  mostrarTransmisionPersonalizada: boolean;
  transmisionPersonalizada: string;
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
  onChange,
  onKilometrajeChange,
  onAccesoriosChange,
  onTransmisionPersonalizadaChange,
}: FormularioVehiculoProps) {
  const kilometrajeNumero = kilometraje.trim() === '' ? 0 : Number(kilometraje);
  const esNuevo = kilometrajeNumero <= 100;

  return (
    <div className="space-y-6">
      {/* INFORMACIÓN BÁSICA */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Marca</label>
          <input
            type="text"
            name="marca"
            required
            value={formData.marca}
            onChange={onChange}
            placeholder="Ej: Toyota"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Modelo</label>
          <input
            type="text"
            name="modelo"
            required
            value={formData.modelo}
            onChange={onChange}
            placeholder="Ej: Corolla"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Versión</label>
          <input
            type="text"
            name="version"
            required
            value={formData.version}
            onChange={onChange}
            placeholder="Ej: EX, Sport, Limited"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Año</label>
          <input
            type="number"
            name="anio"
            required
            min="1950"
            max={new Date().getFullYear() + 1}
            value={formData.anio}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Precio</label>
          <input
            type="number"
            name="precio"
            required
            min="1"
            value={formData.precio === 0 ? '' : formData.precio}
            onChange={onChange}
            placeholder="0"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Kilometraje</label>
          <input
            type="number"
            name="kilometraje"
            required
            min="0"
            value={kilometraje}
            onChange={onKilometrajeChange}
            placeholder="Ej: 50000"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
          <p className="mt-1 text-xs text-gray-500">
            {kilometraje.trim() === ''
              ? 'Indica el kilometraje del vehículo.'
              : esNuevo
                ? 'Se mostrará como vehículo nuevo.'
                : 'Se mostrará como vehículo usado.'}
          </p>
        </div>
      </div>

      {/* ESPECIFICACIONES */}
      <div className="grid grid-cols-1 gap-6 border-t border-gray-100 pt-4 md:grid-cols-3">
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Tipo de Vehículo</label>
          <select
            name="tipoVehiculo"
            required
            value={formData.tipoVehiculo}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
          >
            <option value="">Selecciona...</option>
            <option value="Sedan">Sedán</option>
            <option value="Jeepeta">Jeepeta (SUV)</option>
            <option value="Camioneta">Camioneta (Pick-up)</option>
            <option value="Deportivo">Deportivo</option>
            <option value="SuperDeportivo">Súper Deportivo</option>
            <option value="Hypercar">Hypercar</option>
            <option value="Coupe">Coupé</option>
            <option value="Convertible">Convertible</option>
            <option value="Minivan">Minivan</option>
            <option value="Hatchback">Hatchback</option>
            <option value="Otro">Otro</option>
          </select>
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Motor</label>
          <input
            type="text"
            name="motor"
            required
            value={formData.motor}
            onChange={onChange}
            placeholder="Ej: 1.6L 4 cilindros"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Tracción</label>
          <select
            name="traccion"
            required
            value={formData.traccion}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
          >
            <option value="">Selecciona...</option>
            <option value="Delantera">Delantera</option>
            <option value="Trasera">Trasera</option>
            <option value="AWD">AWD</option>
            <option value="4x4">4x4</option>
          </select>
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Transmisión</label>
          <select
            name="transmision"
            required
            value={formData.transmision}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
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
              <input
                type="text"
                required
                maxLength={MAXIMO_TRANSMISION}
                value={transmisionPersonalizada}
                onChange={onTransmisionPersonalizadaChange}
                placeholder="Escribe la transmisión"
                className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
              />
              <p className="mt-1 text-xs text-gray-500">
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
          <label className="mb-1 block text-sm font-medium text-gray-700">Combustible</label>
          <select
            name="combustible"
            required
            value={formData.combustible}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
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
          <label className="mb-1 block text-sm font-medium text-gray-700">Color Exterior</label>
          <input
            type="text"
            name="colorExterior"
            required
            value={formData.colorExterior}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Color Interior</label>
          <input
            type="text"
            name="colorInterior"
            required
            value={formData.colorInterior}
            onChange={onChange}
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* DETALLES Y UBICACIÓN */}
      <div className="grid grid-cols-1 gap-6 border-t border-gray-100 pt-4 md:grid-cols-2">
        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Ubicación</label>
          <input
            type="text"
            name="ubicacion"
            required
            value={formData.ubicacion}
            onChange={onChange}
            placeholder="Ej: Santo Domingo"
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-700">Accesorios</label>
          <input
            type="text"
            value={accesoriosTexto}
            onChange={onAccesoriosChange}
            placeholder="Sunroof, Cámara, Asientos en piel..."
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
          <p className="mt-1 text-xs text-gray-500">Separa los accesorios usando comas.</p>
        </div>

        <div className="md:col-span-2">
          <label className="mb-1 block text-sm font-medium text-gray-700">Descripción</label>
          <textarea
            name="descripcion"
            rows={6}
            required
            maxLength={5000}
            value={formData.descripcion}
            onChange={onChange}
            placeholder="Detalles adicionales sobre el vehículo..."
            className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
          />
          <p className="mt-1 text-xs text-gray-500">
            {formData.descripcion.length}/5000 caracteres
          </p>
        </div>
      </div>
    </div>
  );
}