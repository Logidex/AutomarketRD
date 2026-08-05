/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import { anuncioService } from '../services/anuncio.service';
import type { AnuncioCreateDto } from '../types/anuncio.types';

export default function PublicarVehiculo() {
  const navigate = useNavigate();
  const [cargando, setCargando] = useState(false);
  const [archivos, setArchivos] = useState<File[]>([]);
  const [accesoriosTexto, setAccesoriosTexto] = useState('');

  const [formData, setFormData] = useState<Omit<AnuncioCreateDto, 'accesorios'>>({
    usuarioId: 0,
    marca: '',
    modelo: '',
    tipoVehiculo: '',
    colorExterior: '',
    colorInterior: '',
    anio: new Date().getFullYear(),
    precio: 0,
    kilometraje: 0, // Inicia en 0 y permite mantenerlo en 0
    transmision: '',
    combustible: '',
    ubicacion: '',
    descripcion: ''
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'anio' || name === 'precio' || name === 'kilometraje' ? Number(value) : value
    }));
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const filesArray = Array.from(e.target.files);
      if (filesArray.length > 10) {
        Swal.fire('Límite excedido', 'Máximo 10 imágenes permitidas.', 'warning');
        return;
      }
      setArchivos(filesArray);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setCargando(true);

    try {
      const arrayAccesorios = accesoriosTexto
        .split(',')
        .map(item => item.trim())
        .filter(item => item.length > 0);

      const payload: AnuncioCreateDto = {
        ...formData,
        accesorios: arrayAccesorios
      };

      const response = await anuncioService.crearAnuncio(payload);
      
      if (archivos.length > 0 && response.id) {
        await anuncioService.subirImagenes(response.id, archivos);
      }

      // Alerta de Éxito
      await Swal.fire({
        title: '¡Vehículo Publicado!',
        text: 'Tu anuncio ha sido creado y guardado exitosamente.',
        icon: 'success',
        confirmButtonColor: '#2563eb'
      });

      navigate('/dashboard/mis-anuncios');
      
    } catch (err: any) {
      console.error(err);
      
      let mensajeError = 'Ocurrió un error al publicar el vehículo.';
      
      // Capturamos el error de validación exacto de C#
      if (err.response && err.response.data && err.response.data.errors) {
        mensajeError = 'Verifica que todos los campos requeridos estén llenos correctamente.';
      }

      // Alerta de Error
      Swal.fire({
        title: 'No se pudo publicar',
        text: mensajeError,
        icon: 'error',
        confirmButtonColor: '#ef4444'
      });

    } finally {
      setCargando(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto bg-white p-8 rounded-lg shadow-sm border border-gray-200">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Publicar Nuevo Vehículo</h2>
      
      <form onSubmit={handleSubmit} className="space-y-6">
        
        {/* Información Básica */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Marca</label>
            <input type="text" name="marca" required value={formData.marca} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Ej: Toyota" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Modelo</label>
            <input type="text" name="modelo" required value={formData.modelo} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Ej: Corolla" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Año</label>
            <input type="number" name="anio" required min="1950" max={new Date().getFullYear() + 1} value={formData.anio} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Precio (USD o DOP)</label>
            {/* Ocultamos las flechas con CSS de Tailwind y permitimos tipado manual limpio */}
            <input 
              type="number" 
              name="precio" 
              required 
              min="1" 
              value={formData.precio === 0 ? '' : formData.precio} 
              onChange={handleChange} 
              className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500 [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none [-moz-appearance:textfield]" 
              placeholder="0"
            />
          </div>
        </div>

        {/* Especificaciones */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-4 border-t border-gray-100">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Tipo de Vehículo</label>
            <select name="tipoVehiculo" required value={formData.tipoVehiculo} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md bg-white focus:ring-blue-500 focus:border-blue-500">
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
            <label className="block text-sm font-medium text-gray-700 mb-1">Transmisión</label>
            <select name="transmision" required value={formData.transmision} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md bg-white focus:ring-blue-500 focus:border-blue-500">
              <option value="">Selecciona...</option>
              <option value="Automatica">Automática</option>
              <option value="Manual">Manual</option>
              <option value="Secuencial">Secuencial</option>
              <option value="CVT">CVT</option>
              <option value="DobleEmbrague">Doble Embrague (DCT)</option>
              <option value="Otra">Otra</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Combustible</label>
            <select name="combustible" required value={formData.combustible} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md bg-white focus:ring-blue-500 focus:border-blue-500">
              <option value="">Selecciona...</option>
              <option value="Gasolina">Gasolina</option>
              <option value="Diesel">Diésel</option>
              <option value="Gas">GLP / Gas Natural</option>
              <option value="Electrico">Eléctrico</option>
              <option value="Hibrido">Híbrido</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Color Exterior</label>
            <input type="text" name="colorExterior" required value={formData.colorExterior} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Color Interior</label>
            <input type="text" name="colorInterior" required value={formData.colorInterior} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Kilometraje (0 si es nuevo)</label>
            {/* Ocultamos flechas y permitimos que se quede en 0 */}
            <input 
              type="number" 
              name="kilometraje" 
              required 
              min="0" 
              value={formData.kilometraje} 
              onChange={handleChange} 
              className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500 [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none [-moz-appearance:textfield]" 
            />
          </div>
        </div>

        {/* Detalles y Ubicación */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4 border-t border-gray-100">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Ubicación</label>
            <input type="text" name="ubicacion" required value={formData.ubicacion} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Ej: Santo Domingo" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Accesorios (Separados por coma)</label>
            <input type="text" name="accesorios" value={accesoriosTexto} onChange={(e) => setAccesoriosTexto(e.target.value)} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Sunroof, Cámara, Asientos en piel..." />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
            <textarea name="descripcion" rows={4} required value={formData.descripcion} onChange={handleChange} className="w-full p-2.5 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500" placeholder="Detalles adicionales sobre las condiciones..." />
          </div>
        </div>

        {/* Subida de Imágenes */}
        <div className="pt-4 border-t border-gray-100">
          <label className="block text-sm font-medium text-gray-700 mb-2">Fotos del Vehículo (Máximo 10)</label>
          <input 
            type="file" 
            multiple 
            accept="image/png, image/jpeg" 
            onChange={handleImageChange}
            className="block w-full text-sm text-gray-500 file:mr-4 file:py-2.5 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 border border-gray-300 rounded-md cursor-pointer"
          />
          {archivos.length > 0 && (
            <p className="mt-2 text-sm text-green-600 font-medium">{archivos.length} archivo(s) listos para subir.</p>
          )}
        </div>

        {/* Botones de Acción */}
        <div className="pt-6 flex justify-end gap-4 border-t border-gray-100">
          <button 
            type="button"
            onClick={() => navigate('/dashboard/mis-anuncios')}
            className="px-6 py-2.5 border border-gray-300 text-gray-700 rounded-md font-medium hover:bg-gray-50 transition-colors"
          >
            Cancelar
          </button>
          <button 
            type="submit" 
            disabled={cargando}
            className={`px-6 py-2.5 bg-blue-600 text-white rounded-md font-medium transition-colors ${cargando ? 'opacity-50 cursor-not-allowed' : 'hover:bg-blue-700'}`}
          >
            {cargando ? 'Guardando e Subiendo...' : 'Publicar Vehículo'}
          </button>
        </div>

      </form>
    </div>
  );
}