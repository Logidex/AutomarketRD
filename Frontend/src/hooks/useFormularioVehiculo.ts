import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Swal from 'sweetalert2';
import { anuncioService } from '../services/anuncio.service';
import type { AnuncioCreateRequestDto } from '../types/anuncio.types';
import { useLoading } from '../context/LoadingContext';

export const useFormularioVehiculo = (isEditMode: boolean = false) => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { setLoading } = useLoading();

  const [archivos, setArchivos] = useState<File[]>([]);
  const [fotosGuardadas, setFotosGuardadas] = useState<string[]>([]);
  const [accesoriosTexto, setAccesoriosTexto] = useState("");
  const [mostrarTransmisionPersonalizada, setMostrarTransmisionPersonalizada] = useState(false);
  const [transmisionPersonalizada, setTransmisionPersonalizada] = useState("");
  const [kilometraje, setKilometraje] = useState<string>("");

  const [formData, setFormData] = useState({
    marca: "", modelo: "", version: "", tipoVehiculo: "", motor: "",
    traccion: "", colorExterior: "", colorInterior: "",
    anio: new Date().getFullYear(), precio: 0, kilometraje: 0,
    transmision: "", combustible: "", ubicacion: "", descripcion: "",
  });

  useEffect(() => {
    if (isEditMode && id) {
      const cargarAnuncio = async () => {
        setLoading(true);
        try {
          const datos = await anuncioService.obtenerPorId(id);
          setFormData({
            marca: datos.marca, modelo: datos.modelo, version: datos.version,
            tipoVehiculo: datos.tipoVehiculo, motor: datos.motor, traccion: datos.traccion,
            colorExterior: datos.colorExterior, colorInterior: datos.colorInterior,
            anio: datos.anio, precio: datos.precio, kilometraje: datos.kilometraje,
            transmision: datos.transmision, combustible: datos.combustible,
            ubicacion: datos.ubicacion, descripcion: datos.descripcion,
          });
          setKilometraje(datos.kilometraje.toString());
          setAccesoriosTexto(datos.accesorios.join(", "));
          setFotosGuardadas(datos.fotos || []);

          if (!["Automatica", "Manual", "Secuencial", "CVT", "DobleEmbrague"].includes(datos.transmision)) {
            setFormData((prev) => ({ ...prev, transmision: "Otra" }));
            setTransmisionPersonalizada(datos.transmision);
            setMostrarTransmisionPersonalizada(true);
          }
        } catch {
          Swal.fire("Error", "No se cargaron los datos", "error");
          navigate("/dashboard/mis-anuncios");
        } finally {
          setLoading(false);
        }
      };
      cargarAnuncio();
    }
  }, [id, isEditMode, navigate, setLoading]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    if (name === "transmision") {
      setMostrarTransmisionPersonalizada(value === "Otra");
      if (value !== "Otra") setTransmisionPersonalizada("");
    }
    setFormData((prev) => ({ ...prev, [name]: (name === "anio" || name === "precio") ? Number(value) : value }));
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;
    const nuevosArchivos = Array.from(e.target.files);
    setArchivos((prev) => [...prev, ...nuevosArchivos]);
  };

  const handleEliminarArchivo = (indice: number) => setArchivos((prev) => prev.filter((_, i) => i !== indice));
  
  const handleEliminarFotoGuardada = (indice: number) => {
    setFotosGuardadas((prev) => prev.filter((_, i) => i !== indice));
  };

  const guardar = async (payload: AnuncioCreateRequestDto) => {
    setLoading(true);
    try {
      if (isEditMode && id) {
        await anuncioService.actualizarAnuncio(id, payload);
        if (archivos.length > 0) await anuncioService.subirImagenes(Number(id), archivos);
        Swal.fire("Éxito", "Actualizado correctamente", "success");
      } else {
        const response = await anuncioService.crearAnuncio(payload);
        if (archivos.length > 0) await anuncioService.subirImagenes(response.id, archivos);
        Swal.fire("Éxito", "Creado correctamente", "success");
      }
      navigate("/dashboard/mis-anuncios");
    } catch (err) {
      console.error(err);
      Swal.fire("Error", "No se pudo guardar", "error");
    } finally {
      setLoading(false);
    }
  };

  return {
    formData, setFormData,
    kilometraje, setKilometraje,
    accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada,
    transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange, 
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar
  };
};
