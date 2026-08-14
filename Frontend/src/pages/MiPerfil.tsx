import { useEffect, useRef, useState } from "react";
import Swal from "sweetalert2";
import {
  FaStore,
  FaWhatsapp,
  FaImage,
  FaEdit,
  FaTimes,
  FaSave,
} from "react-icons/fa";
import type { PerfilDealer } from "../services/perfilDealer.service";
import { getUserIdFromToken } from "../utils/jwt.util";
import { urlImagen } from "../utils/imagen";
import { useLoading } from "../context/LoadingContext";
import Spinner from "../components/Spinner";
import SeccionCambiarCorreo from "../components/SeccionCambiarCorreo";
import SeccionCambiarPassword from "../components/SeccionCambiarPassword";
import { usePerfilDealer, useActualizarPerfilDealer } from "../hooks/usePerfilDealer";

const CAMPOS_VACIOS = {
  nombreAgencia: "",
  ubicacion: "",
  telefonoAgencia: "",
  horarios: "",
  descripcion: "",
  whatsApp: "",
};

function mapearCampos(data: PerfilDealer) {
  return {
    nombreAgencia: data.nombreAgencia ?? "",
    ubicacion: data.ubicacion ?? "",
    telefonoAgencia: data.telefonoAgencia ?? "",
    horarios: data.horarios ?? "",
    descripcion: data.descripcion ?? "",
    whatsApp: data.whatsApp ?? "",
  };
}

function useObjectUrl(archivo: File | null): string {
  const [url, setUrl] = useState("");
  useEffect(() => {
    if (!archivo) return;
    const nueva = URL.createObjectURL(archivo);
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setUrl(nueva);
    return () => URL.revokeObjectURL(nueva);
  }, [archivo]);
  return url;
}

function usePerfil() {
  const usuarioId = getUserIdFromToken();
  const { setLoading } = useLoading();

  const [perfilExiste, setPerfilExiste] = useState(false);
  const [modoEdicion, setModoEdicion] = useState(false);

  const [camposGuardados, setCamposGuardados] = useState(CAMPOS_VACIOS);
  const [form, setForm] = useState(CAMPOS_VACIOS);

  const logoUrlGuardadoRef = useRef("");
  const [logo, setLogo] = useState<File | null>(null);
  const objectUrl = useObjectUrl(logo);

  const { data: perfil, isLoading: cargando, isError, error } = usePerfilDealer(usuarioId);
  const actualizarPerfil = useActualizarPerfilDealer();

  const esNoEncontrado =
    isError && error instanceof Error && "response" in error &&
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (error as any).response?.status === 404;

  useEffect(() => {
    if (!perfil) return;
    const valores = mapearCampos(perfil);
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setForm(valores);
    setCamposGuardados(valores);
    logoUrlGuardadoRef.current = perfil.logoUrl ?? "";
    setPerfilExiste(true);
  }, [perfil]);

  useEffect(() => {
    if (isError && !esNoEncontrado) {
      Swal.fire({
        title: "Error",
        text: error instanceof Error ? error.message : "No se pudo cargar tu perfil.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isError]);

  const hayCambios =
    logo !== null ||
    JSON.stringify(form) !== JSON.stringify(camposGuardados);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleLogo = (e: React.ChangeEvent<HTMLInputElement>) => {
    const archivo = e.target.files?.[0];
    if (!archivo) return;
    setLogo(archivo);
  };

  const comenzarEnEdicion = () => setModoEdicion(true);

  const cancelarEdicion = () => {
    setForm(camposGuardados);
    setLogo(null);
    setModoEdicion(false);
  };

  const quitarLogoNuevo = () => setLogo(null);

  const handleGuardar = async (event: React.FormEvent) => {
    event.preventDefault();

    if (actualizarPerfil.isPending) return;

    if (usuarioId === null) return;
    if (!modoEdicion || !hayCambios) return;

    setLoading(true);
    try {
      const data = await actualizarPerfil.mutateAsync({
        ...form,
        logo: logo ?? undefined,
      });

      const valores = mapearCampos(data);
      setCamposGuardados(valores);
      setForm(valores);
      logoUrlGuardadoRef.current = data.logoUrl ?? "";
      setLogo(null);
      setPerfilExiste(true);
      setModoEdicion(false);

      Swal.fire({
        title: "Guardado",
        text: "Tu perfil se actualizó correctamente.",
        icon: "success",
        confirmButtonColor: "#2563eb",
      });
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      console.error(error);
      Swal.fire({
        title: "Error",
        text: error.message || "No se pudo actualizar el perfil.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    } finally {
      setLoading(false);
    }
  };

  return {
    usuarioId,
    cargando,
    perfilExiste,
    modoEdicion,
    form,
    logo,
    logoPreview: logo ? objectUrl : logoUrlGuardadoRef.current,
    hayCambios,
    handleChange,
    handleLogo,
    handleGuardar,
    comenzarEnEdicion,
    cancelarEdicion,
    quitarLogoNuevo,
  };
}

interface PropsFormulario {
  form: typeof CAMPOS_VACIOS;
  logo: File | null;
  logoPreview: string;
  modoEdicion: boolean;
  hayCambios: boolean;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
  onLogo: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onGuardar: (e: React.FormEvent) => void;
  onCancelar: () => void;
  onQuitarLogo: () => void;
}

function FormularioPerfil({
  form,
  logo,
  logoPreview,
  modoEdicion,
  hayCambios,
  onChange,
  onLogo,
  onGuardar,
  onCancelar,
  onQuitarLogo,
}: PropsFormulario) {
  return (
    <form
      onSubmit={onGuardar}
      className="space-y-6 rounded-2xl border border-gray-200 bg-white p-6 shadow-sm"
    >
      {/* LOGO */}
      <div>
        <label
          htmlFor="logoAgencia"
          className="mb-2 block text-sm font-medium text-gray-700"
        >
          Logo de la agencia
        </label>

        <div className="flex items-center gap-4">
          <div className="h-24 w-24 overflow-hidden rounded-xl border border-gray-200 bg-gray-100">
            {logoPreview ? (
              <img
                src={logo ? logoPreview : urlImagen(logoPreview)}
                alt="Logo"
                className="h-full w-full object-cover"
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center text-3xl text-gray-300">
                <FaImage />
              </div>
            )}
          </div>

          {modoEdicion && (
            <label
              htmlFor="logoAgencia"
              className="cursor-pointer rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50"
            >
              {logo ? "Cambiar logo seleccionado" : "Subir nuevo logo"}
              <input
                id="logoAgencia"
                type="file"
                accept="image/png, image/jpeg, image/webp"
                onChange={onLogo}
                className="hidden"
              />
            </label>
          )}

          {logo && (
            <button
              type="button"
              onClick={onQuitarLogo}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50"
            >
              Quitar logo nuevo
            </button>
          )}
        </div>
      </div>

      {/* CAMPOS */}
      <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
        <div>
          <label
            htmlFor="nombreAgencia"
            className="mb-1 block text-sm font-medium text-gray-700"
          >
            Nombre de la agencia
          </label>
          <input
            id="nombreAgencia"
            type="text"
            name="nombreAgencia"
            value={form.nombreAgencia}
            onChange={onChange}
            disabled={!modoEdicion}
            required
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
          />
        </div>

        <div>
          <label
            htmlFor="ubicacion"
            className="mb-1 block text-sm font-medium text-gray-700"
          >
            Ubicación
          </label>
          <input
            id="ubicacion"
            type="text"
            name="ubicacion"
            value={form.ubicacion}
            onChange={onChange}
            disabled={!modoEdicion}
            required
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
          />
        </div>

        <div>
          <label
            htmlFor="telefonoAgencia"
            className="mb-1 block text-sm font-medium text-gray-700"
          >
            Teléfono
          </label>
          <input
            id="telefonoAgencia"
            type="tel"
            name="telefonoAgencia"
            value={form.telefonoAgencia}
            onChange={onChange}
            disabled={!modoEdicion}
            required
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
          />
        </div>

        <div>
          <label
            htmlFor="whatsApp"
            className="mb-1 flex items-center gap-1.5 text-sm font-medium text-gray-700"
          >
            <FaWhatsapp className="text-green-600" />
            WhatsApp
          </label>
          <input
            id="whatsApp"
            type="text"
            name="whatsApp"
            value={form.whatsApp}
            onChange={onChange}
            disabled={!modoEdicion}
            placeholder="+1 809 000 0000"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
          />
        </div>

        <div className="md:col-span-2">
          <label
            htmlFor="horarios"
            className="mb-1 block text-sm font-medium text-gray-700"
          >
            Horarios de atención
          </label>
          <input
            id="horarios"
            type="text"
            name="horarios"
            value={form.horarios}
            onChange={onChange}
            disabled={!modoEdicion}
            placeholder="Lunes a Sábado, 9:00 AM - 6:00 PM"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
          />
        </div>

        <div className="md:col-span-2">
          <label
            htmlFor="descripcion"
            className="mb-1 block text-sm font-medium text-gray-700"
          >
            Descripción
          </label>
          <textarea
            id="descripcion"
            name="descripcion"
            value={form.descripcion}
            onChange={onChange}
            disabled={!modoEdicion}
            rows={4}
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
          />
        </div>
      </div>

      {modoEdicion && (
        <div className="flex flex-col gap-3 border-t border-gray-100 pt-4 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onCancelar}
            className="flex items-center justify-center gap-2 rounded-lg border border-gray-300 bg-white px-6 py-2.5 font-semibold text-gray-700 transition-colors hover:bg-gray-50"
          >
            <FaTimes />
            Cancelar
          </button>

          <button
            type="submit"
            className="flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-2.5 font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
            disabled={!hayCambios}
          >
            <FaSave />
            {hayCambios ? "Guardar cambios" : "Sin cambios"}
          </button>
        </div>
      )}
    </form>
  );
}

export default function MiPerfil() {
  const {
    usuarioId,
    cargando,
    perfilExiste,
    modoEdicion,
    form,
    logo,
    logoPreview,
    hayCambios,
    handleChange,
    handleLogo,
    handleGuardar,
    comenzarEnEdicion,
    cancelarEdicion,
    quitarLogoNuevo,
  } = usePerfil();

  if (usuarioId === null) {
    return (
      <div className="p-6 text-gray-500">
        No se pudo identificar al usuario.
      </div>
    );
  }

  if (cargando) {
    return <Spinner />;
  }

  if (!perfilExiste && !modoEdicion) {
    return (
      <div className="mx-auto max-w-3xl p-6">
        <div className="mb-6 flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-full bg-blue-100">
            <FaStore className="text-lg text-blue-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Mi Perfil</h1>
            <p className="text-sm text-gray-500">
              Información pública de tu agencia en AutoMarket RD
            </p>
          </div>
        </div>

        <div className="flex min-h-[280px] flex-col items-center justify-center rounded-2xl border border-gray-200 bg-white px-6 text-center shadow-sm">
          <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-blue-50">
            <FaStore className="text-3xl text-blue-600" />
          </div>
          <h2 className="mb-2 text-xl font-bold text-gray-800">
            Aún no has configurado tu agencia
          </h2>
          <p className="mb-6 max-w-md text-gray-500">
            Completa tu información para que los compradores conozcan tu
            agencia cuando vean tus anuncios.
          </p>
          <button
            type="button"
            onClick={comenzarEnEdicion}
            className="rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            Completar mi perfil
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl p-6">
      <div className="mb-6 flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-full bg-blue-100">
            <FaStore className="text-lg text-blue-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Mi Perfil</h1>
            <p className="text-sm text-gray-500">
              Información pública de tu agencia en AutoMarket RD
            </p>
          </div>
        </div>

        {!modoEdicion ? (
          <button
            type="button"
            onClick={comenzarEnEdicion}
            className="flex items-center gap-2 rounded-lg border border-blue-200 bg-blue-50 px-4 py-2 font-semibold text-blue-700 transition-colors hover:bg-blue-100"
          >
            <FaEdit />
            Editar perfil
          </button>
        ) : (
          <span className="flex items-center gap-2 rounded-lg bg-amber-100 px-4 py-2 text-sm font-semibold text-amber-800">
            <FaSave />
            Modo edición
          </span>
        )}
      </div>

      <FormularioPerfil
        form={form}
        logo={logo}
        logoPreview={logoPreview}
        modoEdicion={modoEdicion}
        hayCambios={hayCambios}
        onChange={handleChange}
        onLogo={handleLogo}
        onGuardar={handleGuardar}
        onCancelar={cancelarEdicion}
        onQuitarLogo={quitarLogoNuevo}
      />

      {/* SECCIÓN DE CUENTA: CAMBIO DE CORREO */}
      <div className="mt-6">
        <SeccionCambiarCorreo />
      </div>

      {/* SECCIÓN DE CUENTA: CAMBIO DE CONTRASEÑA */}
      <div className="mt-6">
        <SeccionCambiarPassword />
      </div>
    </div>
  );
}