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
  useDestacarAnuncio,
  useQuitarDestacadoAnuncio,
} from './useAnuncios';

export const useFormularioVehiculo = (
  isEditMode: boolean = false,
  destino: string = "/dashboard/mis-anuncios",
  maxImagenes: number = 10,
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
  const destacarAnuncio = useDestacarAnuncio();
  const quitarDestacadoAnuncio = useQuitarDestacadoAnuncio();

  const MINIMO_IMAGENES = 5;

  const enviandoRef = useRef(false);
  const fotosInicialesRef = useRef<string[]>([]);
  const esDestacadoInicialRef = useRef(false);
  const [submitting, setSubmitting] = useState(false);
  const [publicarAlGuardar, setPublicarAlGuardar] = useState(false);
  const [destacarAlPublicar, setDestacarAlPublicar] = useState(false);

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
      let activo = true;
      const cargarAnuncio = async () => {
        setLoading(true);
        try {
          if (!activo) return;
          const datos = await anuncioService.obtenerPorId(id);
          if (!activo) return;
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
          esDestacadoInicialRef.current = datos.esDestacado ?? false;
          setDestacarAlPublicar(datos.esDestacado ?? false);

if (!["Automatica", "Manual", "Secuencial", "CVT", "DobleEmbrague"].includes(datos.transmision)) {
            setFormData((prev) => ({ ...prev, transmision: "Otra" }));
            setTransmisionPersonalizada(datos.transmision);
            setMostrarTransmisionPersonalizada(true);
          }
        } catch {
          if (!activo) return;
          Swal.fire("Error", "No se cargaron los datos", "error");
          navigate(destino);
        } finally {
          if (activo) setLoading(false);
        }
      };
      cargarAnuncio();
      return () => {
        activo = false;
      };
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

    const cupo = Math.max(0, maxImagenes - (archivos.length + fotosGuardadas.length));
    if (cupo === 0) {
      Swal.fire({
        title: "Límite alcanzado",
        text: `Ya puedes agregar hasta ${maxImagenes} imágenes por anuncio.`,
        icon: "warning",
        confirmButtonColor: "#ef4444",
      });
      return;
    }

    const nuevosArchivos = Array.from(e.target.files).slice(0, cupo);
    setArchivos((prev) => [...prev, ...nuevosArchivos]);
  };

  const handleEliminarArchivo = (indice: number) => {
    const eliminado = archivos[indice];
    if (fotoPrincipal === eliminado) setFotoPrincipal(null);
    setArchivos((prev) => prev.filter((_, i) => i !== indice));
  };

  const handleEliminarFotoGuardada = (indice: number) => {
    const eliminado = fotosGuardadas[indice];
    if (fotoPrincipal === eliminado) setFotoPrincipal(null);
    setFotosGuardadas((prev) => prev.filter((_, i) => i !== indice));
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
    if (totalImagenes < MINIMO_IMAGENES || totalImagenes > maxImagenes) {
      enviandoRef.current = false;
      setSubmitting(false);
      setLoading(false);
      Swal.fire({
        title: "Imágenes requeridas",
        text: `Debes agregar entre ${MINIMO_IMAGENES} y ${maxImagenes} fotos para guardar el anuncio.`,
        icon: "warning",
        confirmButtonColor: "#ef4444",
      });
      return;
    }

    try {
      if (isEditMode && id) {
        // Marcar anuncio como actualizándose primero
        await actualizarAnuncio.mutateAsync({ id, dto: payload });

        // Solo si la actualización fue exitosa, proceder con fotos
        const fotosGuardadasSet = new Set(fotosGuardadas);
        const fotosEliminadas = fotosInicialesRef.current.filter(
          (foto) => !fotosGuardadasSet.has(foto),
        );
        await Promise.all(
          fotosEliminadas.map((url) =>
            eliminarImagen.mutateAsync({ id: Number(id), urlImagen: url }),
          ),
        );

        let rutasSubidas: string[] = [];
        if (archivos.length > 0) {
          rutasSubidas = await subirImagenes.mutateAsync({ id: Number(id), imagenes: archivos });
        }
        await aplicarFotoPrincipal(Number(id), rutasSubidas);
        if (publicarAlGuardar) await publicarAnuncio.mutateAsync(Number(id));
        await aplicarDestacado(Number(id), esDestacadoInicialRef.current);
      } else {
        const response = await crearAnuncio.mutateAsync(payload);
        let rutasSubidas: string[] = [];
        if (archivos.length > 0) {
          rutasSubidas = await subirImagenes.mutateAsync({ id: response.id, imagenes: archivos });
        }
        await aplicarFotoPrincipal(response.id, rutasSubidas);
        if (publicarAlGuardar) await publicarAnuncio.mutateAsync(response.id);
        await aplicarDestacado(response.id, false);
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

  const aplicarDestacado = async (idAnuncio: number, fueDestacadoInicial: boolean) => {
    try {
      if (destacarAlPublicar) {
        // En creación solo aplica si se publicó; en edición aplica siempre
        // (el anuncio ya pudo estar publicado antes).
        if (!isEditMode && !publicarAlGuardar) return;
        await destacarAnuncio.mutateAsync(idAnuncio);
      } else if (isEditMode && fueDestacadoInicial) {
        await quitarDestacadoAnuncio.mutateAsync(idAnuncio);
      }
    } catch {
      Swal.fire({
        title: "Aviso de destacado",
        text: "El anuncio se guardó correctamente, pero no fue posible gestionar su destacado. Verifica que tengas cupo disponible en tu plan.",
        icon: "warning",
        confirmButtonColor: "#f59e0b",
      });
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
    destacarAlPublicar, setDestacarAlPublicar,
    fotoPrincipal, handleEstablecerPrincipal
  };
};
