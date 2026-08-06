import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import axios from 'axios';

import { anuncioService } from '../services/anuncio.service';

import type {
  AnuncioCreateRequestDto,
  AnuncioCreateFormDto
} from '../types/anuncio.types';

export default function PublicarVehiculo() {
  const navigate = useNavigate();

  const [cargando, setCargando] = useState(false);
  const [archivos, setArchivos] = useState<File[]>([]);
  const [accesoriosTexto, setAccesoriosTexto] = useState('');

  const [
    mostrarTransmisionPersonalizada,
    setMostrarTransmisionPersonalizada
  ] = useState(false);

  const [
    transmisionPersonalizada,
    setTransmisionPersonalizada
  ] = useState('');

  const [formData, setFormData] =
    useState<AnuncioCreateFormDto>({
      marca: '',
      modelo: '',
      version: '',

      tipoVehiculo: '',
      motor: '',
      traccion: '',

      colorExterior: '',
      colorInterior: '',

      anio: new Date().getFullYear(),
      precio: 0,
      kilometraje: 0,

      transmision: '',
      combustible: '',

      ubicacion: '',
      descripcion: ''
    });

  const [kilometraje, setKilometraje] =
    useState<string>('');

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
    >
  ) => {
    const { name, value } = e.target;

    if (name === 'transmision') {
      setFormData((prev) => ({
        ...prev,
        transmision: value
      }));

      setMostrarTransmisionPersonalizada(
        value === 'Otra'
      );

      if (value !== 'Otra') {
        setTransmisionPersonalizada('');
      }

      return;
    }

    setFormData((prev) => ({
      ...prev,
      [name]:
        name === 'anio' || name === 'precio'
          ? Number(value)
          : value
    }));
  };

  const handleImageChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (!e.target.files) {
      return;
    }

    const filesArray = Array.from(e.target.files);

    if (filesArray.length > 10) {
      Swal.fire(
        'Límite excedido',
        'Máximo 10 imágenes permitidas.',
        'warning'
      );

      e.target.value = '';
      return;
    }

    setArchivos(filesArray);
  };

  const handleSubmit = async (
    e: React.FormEvent
  ) => {
    e.preventDefault();
    setCargando(true);

    try {
      const kilometrajeTexto = kilometraje.trim();

      if (kilometrajeTexto === '') {
        await Swal.fire({
          title: 'Falta el kilometraje',
          text: 'Debes indicar el kilometraje del vehículo.',
          icon: 'warning',
          confirmButtonColor: '#2563eb'
        });

        return;
      }

      const kilometrajeNumero = Number(
        kilometrajeTexto
      );

      if (
        !Number.isInteger(kilometrajeNumero) ||
        kilometrajeNumero < 0
      ) {
        await Swal.fire({
          title: 'Kilometraje inválido',
          text:
            'El kilometraje debe ser un número entero mayor o igual a cero.',
          icon: 'warning',
          confirmButtonColor: '#2563eb'
        });

        return;
      }

      const arrayAccesorios = accesoriosTexto
        .split(',')
        .map((item) => item.trim())
        .filter((item) => item.length > 0);

      const transmisionFinal =
        formData.transmision === 'Otra'
          ? transmisionPersonalizada.trim()
          : formData.transmision;

      if (!transmisionFinal) {
        await Swal.fire({
          title: 'Falta la transmisión',
          text:
            'Debes seleccionar o escribir la transmisión.',
          icon: 'warning',
          confirmButtonColor: '#2563eb'
        });

        return;
      }

      const payload: AnuncioCreateRequestDto = {
        ...formData,
        kilometraje: kilometrajeNumero,
        transmision: transmisionFinal,
        accesorios: arrayAccesorios
      };

      console.log('Payload del anuncio:', payload);

      const response =
        await anuncioService.crearAnuncio(payload);

      if (archivos.length > 0 && response.id) {
        await anuncioService.subirImagenes(
          response.id,
          archivos
        );
      }

      await Swal.fire({
        title: '¡Vehículo guardado!',
        text:
          'Tu anuncio ha sido creado correctamente.',
        icon: 'success',
        confirmButtonColor: '#2563eb'
      });

      navigate('/dashboard/mis-anuncios');
    } catch (err: unknown) {
      console.error(err);

      let mensajeError =
        'Ocurrió un error al publicar el vehículo.';

      if (axios.isAxiosError(err)) {
        const data = err.response?.data;

        if (data?.mensaje) {
          mensajeError = data.mensaje;
        } else if (data?.error) {
          mensajeError = data.error;
        } else if (data?.errors) {
          mensajeError =
            'Verifica que todos los campos requeridos estén llenos correctamente.';
        }
      }

      await Swal.fire({
        title: 'No se pudo publicar',
        text: mensajeError,
        icon: 'error',
        confirmButtonColor: '#ef4444'
      });
    } finally {
      setCargando(false);
    }
  };

  const kilometrajeNumero =
    kilometraje.trim() === ''
      ? 0
      : Number(kilometraje);

  const esNuevo = kilometrajeNumero <= 100;

  return (
    <div className="mx-auto max-w-4xl rounded-lg border border-gray-200 bg-white p-8 shadow-sm">
      <h2 className="mb-6 text-2xl font-bold text-gray-900">
        Publicar Nuevo Vehículo
      </h2>

      <form
        onSubmit={handleSubmit}
        className="space-y-6"
      >
        {/* INFORMACIÓN BÁSICA */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Marca
            </label>

            <input
              type="text"
              name="marca"
              required
              value={formData.marca}
              onChange={handleChange}
              placeholder="Ej: Toyota"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Modelo
            </label>

            <input
              type="text"
              name="modelo"
              required
              value={formData.modelo}
              onChange={handleChange}
              placeholder="Ej: Corolla"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Versión
            </label>

            <input
              type="text"
              name="version"
              required
              value={formData.version}
              onChange={handleChange}
              placeholder="Ej: EX, Sport, Limited"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Año
            </label>

            <input
              type="number"
              name="anio"
              required
              min="1950"
              max={new Date().getFullYear() + 1}
              value={formData.anio}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Precio
            </label>

            <input
              type="number"
              name="precio"
              required
              min="1"
              value={
                formData.precio === 0
                  ? ''
                  : formData.precio
              }
              onChange={handleChange}
              placeholder="0"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Kilometraje
            </label>

            <input
              type="number"
              name="kilometraje"
              required
              min="0"
              value={kilometraje}
              onChange={(e) =>
                setKilometraje(e.target.value)
              }
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
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Tipo de Vehículo
            </label>

            <select
              name="tipoVehiculo"
              required
              value={formData.tipoVehiculo}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="">
                Selecciona...
              </option>
              <option value="Sedan">
                Sedán
              </option>
              <option value="Jeepeta">
                Jeepeta (SUV)
              </option>
              <option value="Camioneta">
                Camioneta (Pick-up)
              </option>
              <option value="Deportivo">
                Deportivo
              </option>
              <option value="SuperDeportivo">
                Súper Deportivo
              </option>
              <option value="Hypercar">
                Hypercar
              </option>
              <option value="Coupe">
                Coupé
              </option>
              <option value="Convertible">
                Convertible
              </option>
              <option value="Minivan">
                Minivan
              </option>
              <option value="Hatchback">
                Hatchback
              </option>
              <option value="Otro">
                Otro
              </option>
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Motor
            </label>

            <input
              type="text"
              name="motor"
              required
              value={formData.motor}
              onChange={handleChange}
              placeholder="Ej: 1.6L 4 cilindros"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Tracción
            </label>

            <select
              name="traccion"
              required
              value={formData.traccion}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="">
                Selecciona...
              </option>
              <option value="Delantera">
                Delantera
              </option>
              <option value="Trasera">
                Trasera
              </option>
              <option value="AWD">
                AWD
              </option>
              <option value="4x4">
                4x4
              </option>
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Transmisión
            </label>

            <select
              name="transmision"
              required
              value={formData.transmision}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="">
                Selecciona...
              </option>
              <option value="Automatica">
                Automática
              </option>
              <option value="Manual">
                Manual
              </option>
              <option value="Secuencial">
                Secuencial
              </option>
              <option value="CVT">
                CVT
              </option>
              <option value="DobleEmbrague">
                Doble Embrague (DCT)
              </option>
              <option value="Otra">
                Otra
              </option>
            </select>

            {mostrarTransmisionPersonalizada && (
              <input
                type="text"
                required
                value={transmisionPersonalizada}
                onChange={(e) =>
                  setTransmisionPersonalizada(
                    e.target.value
                  )
                }
                placeholder="Escribe la transmisión"
                className="mt-2 w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
              />
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Combustible
            </label>

            <select
              name="combustible"
              required
              value={formData.combustible}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 bg-white p-2.5 focus:border-blue-500 focus:ring-blue-500"
            >
              <option value="">
                Selecciona...
              </option>
              <option value="Gasolina">
                Gasolina
              </option>
              <option value="Diesel">
                Diésel
              </option>
              <option value="Gas">
                GLP / Gas Natural
              </option>
              <option value="Electrico">
                Eléctrico
              </option>
              <option value="Hibrido">
                Híbrido
              </option>
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Color Exterior
            </label>

            <input
              type="text"
              name="colorExterior"
              required
              value={formData.colorExterior}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Color Interior
            </label>

            <input
              type="text"
              name="colorInterior"
              required
              value={formData.colorInterior}
              onChange={handleChange}
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* DETALLES Y UBICACIÓN */}
        <div className="grid grid-cols-1 gap-6 border-t border-gray-100 pt-4 md:grid-cols-2">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Ubicación
            </label>

            <input
              type="text"
              name="ubicacion"
              required
              value={formData.ubicacion}
              onChange={handleChange}
              placeholder="Ej: Santo Domingo"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Accesorios
            </label>

            <input
              type="text"
              value={accesoriosTexto}
              onChange={(e) =>
                setAccesoriosTexto(e.target.value)
              }
              placeholder="Sunroof, Cámara, Asientos en piel..."
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />

            <p className="mt-1 text-xs text-gray-500">
              Separa los accesorios usando comas.
            </p>
          </div>

          <div className="md:col-span-2">
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Descripción
            </label>

            <textarea
              name="descripcion"
              rows={4}
              required
              value={formData.descripcion}
              onChange={handleChange}
              placeholder="Detalles adicionales sobre el vehículo..."
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* IMÁGENES */}
        <div className="border-t border-gray-100 pt-4">
          <label className="mb-2 block text-sm font-medium text-gray-700">
            Fotos del Vehículo
          </label>

          <input
            type="file"
            multiple
            accept="image/png, image/jpeg"
            onChange={handleImageChange}
            className="block w-full cursor-pointer rounded-md border border-gray-300 text-sm text-gray-500 file:mr-4 file:rounded-md file:border-0 file:bg-blue-50 file:px-4 file:py-2.5 file:text-sm file:font-semibold file:text-blue-700 hover:file:bg-blue-100"
          />

          <p className="mt-1 text-xs text-gray-500">
            Máximo 10 imágenes.
          </p>

          {archivos.length > 0 && (
            <p className="mt-2 font-medium text-green-600">
              {archivos.length} archivo(s) listos para subir.
            </p>
          )}
        </div>

        {/* BOTONES */}
        <div className="flex justify-end gap-4 border-t border-gray-100 pt-6">
          <button
            type="button"
            onClick={() =>
              navigate('/dashboard/mis-anuncios')
            }
            className="rounded-md border border-gray-300 px-6 py-2.5 font-medium text-gray-700 transition-colors hover:bg-gray-50"
          >
            Cancelar
          </button>

          <button
            type="submit"
            disabled={cargando}
            className={`rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors ${
              cargando
                ? 'cursor-not-allowed opacity-50'
                : 'hover:bg-blue-700'
            }`}
          >
            {cargando
              ? 'Guardando y subiendo...'
              : 'Publicar vehículo'}
          </button>
        </div>
      </form>
    </div>
  );
}
