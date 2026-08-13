import { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Swal from 'sweetalert2';
import { anuncioService } from '../services/anuncio.service';
import type { AnuncioCreateRequestDto } from '../types/anuncio.types';
import { useLoading } from '../context/LoadingContext';
import {
  useCrearAnuncio,
  useActualizarAnuncio,
  useSubirImagenesAnuncio,
  useEliminarImagenAnuncio,
  usePublicarAnuncio,
  useEstablecerFotoPrincipal,
} from './useAnuncios';

export const useFormularioVehiculo = (
  isEditMode: boolean = false,
  destino: string = "/dashboard/mis-anuncios",
) => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { setLoading } = useLoading();

  const crearAnuncio = useCrearAnuncio();
  const actualizarAnuncio = useActualizarAnuncio();
  const subirImagenes = useSubirImagenesAnuncio();
  const eliminarImagen = useEliminarImagenAnuncio();
  const publicarAnuncio = usePublicarAnuncio();
  const establecerFotoPrincipal = useEstablecerFotoPrincipal();

  const MINIMO_IMAGENES = 5;
  const MAXIMO_IMAGENES = 10;

  const enviandoRef = useRef(false);
  const fotosInicialesRef = useRef<string[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [publicarAlGuardar, setPublicarAlGuardar] = useState(false);

  const [archivos, setArchivos] = useState<File[]>([]);
  const [fotosGuardadas, setFotosGuardadas] = useState<string[]>([]);
  const [fotoPrincipal, setFotoPrincipal] = useState<File | string | null>(null);
  const [accesoriosTexto, setAccesoriosTexto] = useState("");
  const [mostrarTransmisionPersonalizada, setMostrarTransmisionPersonalizada] = useState(false);
  const [transmisionPersonalizada, setTransmisionPersonalizada] = useState("");
  const [kilometraje, setKilometraje] = useState<string>("");

  const [formData, setFormData] = useState({
    marca: "", modelo: "", version: "", tipoVehiculo: "", motor: "",
    traccion: "", colorExterior: "", colorInterior: "",
    anio: new Date().getFullYear(), precio: 0, moneda: "DOP", precioAnterior: 0, kilometraje: 0,
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
            anio: datos.anio, precio: datos.precio,
            moneda: datos.moneda ?? "DOP",
            precioAnterior: datos.precioAnterior ?? 0,
            kilometraje: datos.kilometraje,
            transmision: datos.transmision, combustible: datos.combustible,
            ubicacion: datos.ubicacion, descripcion: datos.descripcion,
          });
          setKilometraje(datos.kilometraje.toString());
          setAccesoriosTexto(datos.accesorios.join(", "));
          setFotosGuardadas(datos.fotos || []);
          fotosInicialesRef.current = datos.fotos || [];

          if (!["Automatica", "Manual", "Secuencial", "CVT", "DobleEmbrague"].includes(datos.transmision)) {
            setFormData((prev) => ({ ...prev, transmision: "Otra" }));
            setTransmisionPersonalizada(datos.transmision);
            setMostrarTransmisionPersonalizada(true);
          }
        } catch {
          Swal.fire("Error", "No se cargaron los datos", "error");
          navigate(destino);
        } finally {
          setLoading(false);
        }
      };
      cargarAnuncio();
    }
  }, [id, isEditMode, navigate, setLoading, destino]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    if (name === "transmision") {
      setMostrarTransmisionPersonalizada(value === "Otra");
      if (value !== "Otra") setTransmisionPersonalizada("");
    }
    setFormData((prev) => ({ ...prev, [name]: (name === "anio" || name === "precio" || name === "precioAnterior") ? Number(value) : value }));
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;

    const cupo = Math.max(0, MAXIMO_IMAGENES - (archivos.length + fotosGuardadas.length));
    if (cupo === 0) {
      Swal.fire({
        title: "Límite alcanzado",
        text: `Ya puedes agregar hasta ${MAXIMO_IMAGENES} imágenes por anuncio.`,
        icon: "warning",
        confirmButtonColor: "#ef4444",
      });
      return;
    }

    const nuevosArchivos = Array.from(e.target.files).slice(0, cupo);
    setArchivos((prev) => [...prev, ...nuevosArchivos]);
  };

  const handleEliminarArchivo = (indice: number) => {
    setArchivos((prev) => {
      const eliminado = prev[indice];
      if (fotoPrincipal === eliminado) setFotoPrincipal(null);
      return prev.filter((_, i) => i !== indice);
    });
  };

  const handleEliminarFotoGuardada = (indice: number) => {
    setFotosGuardadas((prev) => {
      const eliminado = prev[indice];
      if (fotoPrincipal === eliminado) setFotoPrincipal(null);
      return prev.filter((_, i) => i !== indice);
    });
  };

  const handleEstablecerPrincipal = (tipo: "archivo" | "guardada", indice: number) => {
    if (tipo === "archivo") {
      const archivo = archivos[indice];
      if (archivo) setFotoPrincipal(archivo);
    } else {
      const url = fotosGuardadas[indice];
      if (url) setFotoPrincipal(url);
    }
  };

  const guardar = async (payload: AnuncioCreateRequestDto) => {
    if (enviandoRef.current) return;
    // Usar AtomicFlag pattern para prevenir races conditions más robusto
    enviandoRef.current = true;
    setSubmitting(true);
    setLoading(true);

    const totalImagenes = archivos.length + fotosGuardadas.length;
    if (totalImagenes < MINIMO_IMAGENES || totalImagenes > MAXIMO_IMAGENES) {
      enviandoRef.current = false;
      setSubmitting(false);
      setLoading(false);
      Swal.fire({
        title: "Imágenes requeridas",
        text: `Debes agregar entre ${MINIMO_IMAGENES} y ${MAXIMO_IMAGENES} fotos para guardar el anuncio.`,
        icon: "warning",
        confirmButtonColor: "#ef4444",
      });
      return;
    }

    try {
      if (isEditMode && id) {
        // Marcar anuncio como actualizándose primero
        const anulacionActualizacion = actualizarAnuncio.mutateAsync({ id, dto: payload });

        // Esperar a que la actualización termine (éxito o error)
        try {
          await anulacionActualizacion;

          // Solo si la actualización fue exitosa, proceder con fotos
          const fotosEliminadas = fotosInicialesRef.current.filter(
            (foto) => !fotosGuardadas.includes(foto),
          );
          for (const url of fotosEliminadas) {
            await eliminarImagen.mutateAsync({ id: Number(id), urlImagen: url });
          }

          let rutasSubidas: string[] = [];
          if (archivos.length > 0) {
            rutasSubidas = await subirImagenes.mutateAsync({ id: Number(id), imagenes: archivos });
          }
          await aplicarFotoPrincipal(Number(id), rutasSubidas);
          if (publicarAlGuardar) await publicarAnuncio.mutateAsync(Number(id));
        } catch (mutateError) {
          // Si la mutación falló, lanzar error para el finally
          throw mutateError;
        }
      } else {
        const response = await crearAnuncio.mutateAsync(payload);
        let rutasSubidas: string[] = [];
        if (archivos.length > 0) {
          rutasSubidas = await subirImagenes.mutateAsync({ id: response.id, imagenes: archivos });
        }
        await aplicarFotoPrincipal(response.id, rutasSubidas);
        if (publicarAlGuardar) await publicarAnuncio.mutateAsync(response.id);
        Swal.fire("Éxito", publicarAlGuardar ? "Publicado correctamente" : "Creado correctamente", "success");
      }
      navigate(destino);
    } catch (err) {
      console.error(err);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const msg = (err as any)?.response?.data?.mensaje || (err as Error)?.message;
      Swal.fire({
        title: "Error",
        text: msg || "No se pudo guardar el anuncio.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
      // No reseteamos enviandoRef aquí - se hará en finally
      // para que el usuario pueda reintentar sin recargar estado
      throw err; // Re-lanzar para que el finally se ejecute
    } finally {
      // Pequeño delay para asegurar que el ref esté actualizado
      setTimeout(() => {
        enviandoRef.current = false;
        setSubmitting(false);
        setLoading(false);
      }, 100);
    }
  };

  const aplicarFotoPrincipal = async (idAnuncio: number, rutasSubidas: string[]) => {
    if (!fotoPrincipal) return;

    let urlPrincipal: string | undefined;
    if (typeof fotoPrincipal === "string") {
      urlPrincipal = fotoPrincipal;
    } else {
      const indice = archivos.indexOf(fotoPrincipal);
      urlPrincipal = indice >= 0 ? rutasSubidas[indice] : undefined;
    }

    if (urlPrincipal) {
      await establecerFotoPrincipal.mutateAsync({ id: idAnuncio, urlImagen: urlPrincipal });
    }
  };

  return {
    formData, setFormData,
    kilometraje, setKilometraje,
    accesoriosTexto, setAccesoriosTexto,
    mostrarTransmisionPersonalizada,
    transmisionPersonalizada, setTransmisionPersonalizada,
    archivos, fotosGuardadas, handleChange, handleImageChange, 
    handleEliminarArchivo, handleEliminarFotoGuardada, guardar, submitting,
    publicarAlGuardar, setPublicarAlGuardar,
    fotoPrincipal, handleEstablecerPrincipal
  };
};
