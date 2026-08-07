import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { anuncioService } from "../services/anuncio.service";
import Swal from "sweetalert2";
import axios from "axios";
import type { AnuncioCreateRequestDto } from "../types/anuncio.types";

const MINIMO_IMAGENES = 5;
const MAXIMO_IMAGENES = 10;
const MAXIMO_TAMANO_IMAGEN = 5 * 1024 * 1024;
const MAXIMO_TRANSMISION = 50;

interface ImagenPreviewProps {
  archivo: File;
  indice: number;
  onEliminar: (indice: number) => void;
}

function ImagenPreview({ archivo, indice, onEliminar }: ImagenPreviewProps) {
  // En lugar de useMemo, usamos un estado para la URL
  const [previewUrl, setPreviewUrl] = useState<string>("");

  useEffect(() => {
    const url = URL.createObjectURL(archivo);

    // Lo envolvemos en una promesa resuelta para que sea asíncrono
    Promise.resolve().then(() => {
      setPreviewUrl(url);
    });

    return () => {
      URL.revokeObjectURL(url);
    };
  }, [archivo]);

  return (
    <div className="group relative overflow-hidden rounded-lg border border-gray-200 bg-gray-50">
      {/* Si previewUrl está vacío temporalmente, no intentamos dibujar la imagen rota */}
      {previewUrl && (
        <img
          src={previewUrl}
          alt={`Vista previa ${indice + 1}`}
          className="h-36 w-full object-cover"
        />
      )}
      <button
        type="button"
        onClick={() => onEliminar(indice)}
        title="Eliminar imagen"
        className="absolute right-2 top-2 flex h-8 w-8 items-center justify-center rounded-full bg-red-600 text-lg font-bold text-white opacity-0 transition-opacity hover:bg-red-700 group-hover:opacity-100"
      >
        ×
      </button>
      <div className="truncate px-2 py-2 text-xs text-gray-600">
        {archivo.name}
      </div>
    </div>
  );
}

export default function EditarVehiculo() {
  const navigate = useNavigate();
  const { id } = useParams();
  // --- INICIO DE LA MEMORIA (ESTADOS) ---
  const [cargando, setCargando] = useState(false);
  const [archivos, setArchivos] = useState<File[]>([]);
  const [fotosGuardadas, setFotosGuardadas] = useState<string[]>([]);
  const [accesoriosTexto, setAccesoriosTexto] = useState("");

  const [mostrarTransmisionPersonalizada, setMostrarTransmisionPersonalizada] =
    useState(false);
  const [transmisionPersonalizada, setTransmisionPersonalizada] = useState("");
  const [kilometraje, setKilometraje] = useState<string>("");

  const [formData, setFormData] = useState({
    marca: "",
    modelo: "",
    version: "",
    tipoVehiculo: "",
    motor: "",
    traccion: "",
    colorExterior: "",
    colorInterior: "",
    anio: new Date().getFullYear(),
    precio: 0,
    kilometraje: 0,
    transmision: "",
    combustible: "",
    ubicacion: "",
    descripcion: "",
  });
  // --- FIN DE LA MEMORIA ---

  useEffect(() => {
    // Si no hay ID, no hacemos nada (por seguridad)
    if (!id) return;

    const cargarAnuncio = async () => {
      try {
        // 1. Llamamos al backend (Asegúrate de tener este método en tu servicio)
        const datos = await anuncioService.obtenerPorId(id);

        // 2. Rellenamos la memoria principal
        setFormData({
          marca: datos.marca,
          modelo: datos.modelo,
          version: datos.version,
          tipoVehiculo: datos.tipoVehiculo,
          motor: datos.motor,
          traccion: datos.traccion,
          colorExterior: datos.colorExterior,
          colorInterior: datos.colorInterior,
          anio: datos.anio,
          precio: datos.precio,
          kilometraje: datos.kilometraje,
          transmision: datos.transmision,
          combustible: datos.combustible,
          ubicacion: datos.ubicacion,
          descripcion: datos.descripcion,
        });

        // 3. Rellenamos estados sueltos
        setKilometraje(datos.kilometraje.toString());
        setAccesoriosTexto(datos.accesorios.join(", "));
        setFotosGuardadas(datos.fotos || []);

        // Manejo especial de la transmisión que ya tenías
        if (
          ![
            "Automatica",
            "Manual",
            "Secuencial",
            "CVT",
            "DobleEmbrague",
          ].includes(datos.transmision)
        ) {
          setFormData((prev) => ({ ...prev, transmision: "Otra" }));
          setTransmisionPersonalizada(datos.transmision);
          setMostrarTransmisionPersonalizada(true);
        }
      } catch (error) {
        console.log(error);
        Swal.fire(
          "Error",
          "No se pudieron cargar los datos del vehículo",
          "error",
        );
        navigate("/dashboard/mis-anuncios");
      }
    };

    cargarAnuncio();
  }, [id, navigate]);

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
    >,
  ) => {
    const { name, value } = e.target;

    if (name === "transmision") {
      setFormData((prev) => ({
        ...prev,
        transmision: value,
      }));

      setMostrarTransmisionPersonalizada(value === "Otra");

      if (value !== "Otra") {
        setTransmisionPersonalizada("");
      }

      return;
    }

    setFormData((prev) => ({
      ...prev,
      [name]: name === "anio" || name === "precio" ? Number(value) : value,
    }));
  };

  const obtenerClaveArchivo = (archivo: File): string => {
    return [archivo.name, archivo.size, archivo.lastModified].join("-");
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) {
      return;
    }

    const nuevosArchivos = Array.from(e.target.files);

    const archivosInvalidos = nuevosArchivos.filter(
      (archivo) =>
        archivo.type !== "image/png" && archivo.type !== "image/jpeg",
    );

    if (archivosInvalidos.length > 0) {
      Swal.fire({
        title: "Formato no permitido",
        text: "Solo se permiten imágenes en formato PNG o JPEG.",
        icon: "warning",
        confirmButtonColor: "#2563eb",
      });

      e.target.value = "";
      return;
    }

    const archivosMuyGrandes = nuevosArchivos.filter(
      (archivo) => archivo.size > MAXIMO_TAMANO_IMAGEN,
    );

    if (archivosMuyGrandes.length > 0) {
      Swal.fire({
        title: "Imagen demasiado grande",
        text: "Cada imagen debe pesar como máximo 5 MB.",
        icon: "warning",
        confirmButtonColor: "#2563eb",
      });

      e.target.value = "";
      return;
    }

    const archivosCombinados = [...archivos, ...nuevosArchivos];

    const archivosSinDuplicados = archivosCombinados.filter(
      (archivo, indice, lista) => {
        const claveActual = obtenerClaveArchivo(archivo);

        return (
          lista.findIndex(
            (otroArchivo) => obtenerClaveArchivo(otroArchivo) === claveActual,
          ) === indice
        );
      },
    );

    if (archivosSinDuplicados.length > MAXIMO_IMAGENES) {
      Swal.fire({
        title: "Límite excedido",
        text:
          `Un anuncio puede tener como máximo ` +
          `${MAXIMO_IMAGENES} imágenes.`,
        icon: "warning",
        confirmButtonColor: "#2563eb",
      });

      e.target.value = "";
      return;
    }

    setArchivos(archivosSinDuplicados);

    e.target.value = "";
  };

  const handleEliminarImagen = (indice: number) => {
    setArchivos((prev) =>
      prev.filter((_, archivoIndice) => archivoIndice !== indice),
    );
  };

  const handleEliminarFotoGuardada = async (indice: number) => {
    // 1. Identificamos qué foto vamos a aniquilar
    const fotoAEliminar = fotosGuardadas[indice];

    // 2. Confirmación agresiva para evitar accidentes
    const confirmacion = await Swal.fire({
      title: "¿Estás seguro?",
      text: "Esta imagen se borrará permanentemente de tu inventario y de la nube.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#ef4444",
      cancelButtonColor: "#6b7280",
      confirmButtonText: "Sí, eliminar",
      cancelButtonText: "Cancelar",
    });

    if (confirmacion.isConfirmed) {
      try {
        setCargando(true);

        // 3. Disparamos al backend
        if (id) {
          await anuncioService.eliminarImagen(Number(id), fotoAEliminar);
        }

        // 4. Si el backend no explotó, la borramos de la pantalla
        setFotosGuardadas((prev) =>
          prev.filter((_, archivoIndice) => archivoIndice !== indice),
        );

        Swal.fire({
          title: "¡Eliminada!",
          text: "La imagen ha sido destruida.",
          icon: "success",
          confirmButtonColor: "#2563eb",
        });
      } catch (err: unknown) {
        console.error(err);
        Swal.fire({
          title: "Error",
          text: "No se pudo eliminar la imagen del servidor.",
          icon: "error",
          confirmButtonColor: "#ef4444",
        });
      } finally {
        setCargando(false);
      }
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const totalImagenes = archivos.length + fotosGuardadas.length;

    if (totalImagenes < MINIMO_IMAGENES) {
      await Swal.fire({
        title: "Faltan imágenes",
        text:
          `Debes agregar entre ${MINIMO_IMAGENES} y ` +
          `${MAXIMO_IMAGENES} imágenes para guardar el anuncio.`,
        icon: "warning",
        confirmButtonColor: "#2563eb",
      });

      return;
    }

    if (totalImagenes > MAXIMO_IMAGENES) {
      await Swal.fire({
        title: "Demasiadas imágenes",
        text: `Puedes agregar un máximo de ` + `${MAXIMO_IMAGENES} imágenes.`,
        icon: "warning",
        confirmButtonColor: "#2563eb",
      });

      return;
    }

    setCargando(true);

    try {
      const kilometrajeTexto = kilometraje.trim();

      if (kilometrajeTexto === "") {
        await Swal.fire({
          title: "Falta el kilometraje",
          text: "Debes indicar el kilometraje del vehículo.",
          icon: "warning",
          confirmButtonColor: "#2563eb",
        });

        return;
      }

      const kilometrajeNumero = Number(kilometrajeTexto);

      if (!Number.isInteger(kilometrajeNumero) || kilometrajeNumero < 0) {
        await Swal.fire({
          title: "Kilometraje inválido",
          text: "El kilometraje debe ser un número entero mayor o igual a cero.",
          icon: "warning",
          confirmButtonColor: "#2563eb",
        });

        return;
      }

      const arrayAccesorios = accesoriosTexto
        .split(",")
        .map((item) => item.trim())
        .filter((item) => item.length > 0);

      const transmisionFinal =
        formData.transmision === "Otra"
          ? transmisionPersonalizada.trim()
          : formData.transmision;

      if (!transmisionFinal) {
        await Swal.fire({
          title: "Falta la transmisión",
          text: "Debes seleccionar o escribir la transmisión.",
          icon: "warning",
          confirmButtonColor: "#2563eb",
        });

        return;
      }

      if (transmisionFinal.length > MAXIMO_TRANSMISION) {
        await Swal.fire({
          title: "Transmisión demasiado larga",
          text:
            `La transmisión no puede superar los ` +
            `${MAXIMO_TRANSMISION} caracteres.`,
          icon: "warning",
          confirmButtonColor: "#2563eb",
        });

        return;
      }

      const payload: AnuncioCreateRequestDto = {
        ...formData,
        kilometraje: kilometrajeNumero,
        transmision: transmisionFinal,
        accesorios: arrayAccesorios,
      };

      console.log("Payload del anuncio:", payload);

      // 1. Asegurarnos por seguridad de que el ID exista antes de enviar
      if (!id) {
        throw new Error("No se encontró el ID del vehículo a editar");
      }

      console.log("Payload del anuncio a actualizar:", payload);

      // 2. Usar el tubo correcto (PUT en lugar de POST)
      await anuncioService.actualizarAnuncio(id, payload);

      if (archivos.length > 0) {
        await anuncioService.subirImagenes(Number(id), archivos);
      }

      await Swal.fire({
        title: "¡Vehículo actualizado!",
        text: "Los cambios se han guardado correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });

      navigate("/dashboard/mis-anuncios");
    } catch (err: unknown) {
      console.error(err);

      let mensajeError = "Ocurrió un error al publicar el vehículo.";

      if (axios.isAxiosError(err)) {
        const data = err.response?.data;

        mensajeError =
          data?.mensaje ??
          data?.error ??
          data?.detail ??
          data?.title ??
          mensajeError;
      }

      await Swal.fire({
        title: "No se pudo publicar",
        text: mensajeError,
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    } finally {
      setCargando(false);
    }
  };

  const kilometrajeNumero = kilometraje.trim() === "" ? 0 : Number(kilometraje);

  const esNuevo = kilometrajeNumero <= 100;

  const totalImagenes = archivos.length + fotosGuardadas.length;

  return (
    <div className="mx-auto max-w-4xl rounded-lg border border-gray-200 bg-white p-8 shadow-sm">
      <h2 className="mb-6 text-2xl font-bold text-gray-900">
        Publicar Nuevo Vehículo
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
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
              value={formData.precio === 0 ? "" : formData.precio}
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
              onChange={(e) => setKilometraje(e.target.value)}
              placeholder="Ej: 50000"
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />

            <p className="mt-1 text-xs text-gray-500">
              {kilometraje.trim() === ""
                ? "Indica el kilometraje del vehículo."
                : esNuevo
                  ? "Se mostrará como vehículo nuevo."
                  : "Se mostrará como vehículo usado."}
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
              <option value="">Selecciona...</option>

              <option value="Delantera">Delantera</option>

              <option value="Trasera">Trasera</option>

              <option value="AWD">AWD</option>

              <option value="4x4">4x4</option>
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
                  onChange={(e) => setTransmisionPersonalizada(e.target.value)}
                  placeholder="Escribe la transmisión"
                  className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
                />

                <p className="mt-1 text-xs text-gray-500">
                  {transmisionPersonalizada.length}/{MAXIMO_TRANSMISION}{" "}
                  caracteres
                </p>

                {transmisionPersonalizada.length >= MAXIMO_TRANSMISION && (
                  <p className="mt-1 text-xs font-medium text-orange-600">
                    Has alcanzado el límite máximo de {MAXIMO_TRANSMISION}{" "}
                    caracteres.
                  </p>
                )}
              </div>
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
              <option value="">Selecciona...</option>

              <option value="Gasolina">Gasolina</option>

              <option value="Diesel">Diésel</option>

              <option value="Gas">GLP / Gas Natural</option>

              <option value="Electrico">Eléctrico</option>

              <option value="Hibrido">Híbrido</option>
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
              onChange={(e) => setAccesoriosTexto(e.target.value)}
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
              rows={6}
              required
              maxLength={5000}
              value={formData.descripcion}
              onChange={handleChange}
              placeholder="Detalles adicionales sobre el vehículo..."
              className="w-full rounded-md border border-gray-300 p-2.5 focus:border-blue-500 focus:ring-blue-500"
            />

            <p className="mt-1 text-xs text-gray-500">
              {formData.descripcion.length}/5000 caracteres
            </p>
          </div>
        </div>

        {/* GESTOR DE IMÁGENES */}
        <div className="border-t border-gray-100 pt-4">
          <div className="mb-2 flex items-center justify-between">
            <label className="block text-sm font-medium text-gray-700">
              Fotos del Vehículo
            </label>
            <span className="text-sm font-semibold text-gray-600">
              {archivos.length + fotosGuardadas.length}/{MAXIMO_IMAGENES}
            </span>
          </div>

          <input
            type="file"
            multiple
            accept="image/png, image/jpeg"
            onChange={handleImageChange}
            className="block w-full cursor-pointer rounded-md border border-gray-300 text-sm text-gray-500 file:mr-4 file:rounded-md file:border-0 file:bg-blue-50 file:px-4 file:py-2.5 file:text-sm file:font-semibold file:text-blue-700 hover:file:bg-blue-100"
          />

          <p className="mt-1 text-xs text-gray-500">
            Agrega imágenes una por una o varias a la vez. Debes tener entre 5 y
            10 para guardar el anuncio.
          </p>

          {(archivos.length > 0 || fotosGuardadas.length > 0) && (
            <div className="mt-4 grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
              {/* 1. DIBUJAR FOTOS VIEJAS */}
              {fotosGuardadas.map((foto, indice) => (
                <div
                  key={`old-${indice}`}
                  className="group relative overflow-hidden rounded-lg border border-gray-200 bg-gray-50"
                >
                  <img
                    src={foto}
                    alt={`Guardada ${indice + 1}`}
                    className="h-36 w-full object-cover"
                  />

                  {/* INYECTA ESTE BOTÓN AQUÍ */}
                  <button
                    type="button"
                    onClick={() => handleEliminarFotoGuardada(indice)}
                    title="Eliminar imagen"
                    className="absolute right-2 top-2 flex h-8 w-8 items-center justify-center rounded-full bg-red-600 text-lg font-bold text-white opacity-0 transition-opacity hover:bg-red-700 group-hover:opacity-100"
                  >
                    ×
                  </button>
                </div>
              ))}

              {/* 2. DIBUJAR FOTOS NUEVAS (Las que acaba de subir) */}
              {archivos.map((archivo, indice) => (
                <ImagenPreview
                  key={obtenerClaveArchivo(archivo)}
                  archivo={archivo}
                  indice={indice}
                  onEliminar={handleEliminarImagen}
                />
              ))}
            </div>
          )}

          {/* El texto naranja */}
          {totalImagenes > 0 && totalImagenes < MINIMO_IMAGENES && (
            <p className="mt-3 text-sm font-medium text-orange-600">
              Te faltan {MINIMO_IMAGENES - totalImagenes} imagen(es) para poder
              guardar el anuncio.
            </p>
          )}

          {/* El texto verde */}
          {totalImagenes >= MINIMO_IMAGENES && (
            <p className="mt-3 text-sm font-medium text-green-600">
              Tienes suficientes imágenes para guardar el anuncio.
            </p>
          )}
        </div>

        {/* BOTONES */}
        <div className="flex justify-end gap-4 border-t border-gray-100 pt-6">
          <button
            type="button"
            onClick={() => navigate("/dashboard/mis-anuncios")}
            className="rounded-md border border-gray-300 px-6 py-2.5 font-medium text-gray-700 transition-colors hover:bg-gray-50"
          >
            Cancelar
          </button>

          <button
            type="submit"
            disabled={cargando}
            className={`rounded-md bg-blue-600 px-6 py-2.5 font-medium text-white transition-colors ${
              cargando ? "cursor-not-allowed opacity-50" : "hover:bg-blue-700"
            }`}
          >
            {cargando ? "Actualizando datos..." : "Actualizar vehículo"}
          </button>
        </div>
      </form>
    </div>
  );
}
