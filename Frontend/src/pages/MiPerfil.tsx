import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import {
  FaStore,
  FaWhatsapp,
  FaImage,
  FaEdit,
  FaTimes,
  FaSave,
} from "react-icons/fa";
import {
  perfilDealerService,
  type PerfilDealer,
} from "../services/perfilDealer.service";
import { getUserIdFromToken } from "../utils/jwt.util";
import { useLoading } from "../context/LoadingContext";
import Spinner from "../components/Spinner";

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

export default function MiPerfil() {
  const usuarioId = getUserIdFromToken();
  const { setLoading } = useLoading();

  const [cargando, setCargando] = useState(true);
  const [perfilExiste, setPerfilExiste] = useState(false);
  const [modoEdicion, setModoEdicion] = useState(false);

  const [camposGuardados, setCamposGuardados] = useState(CAMPOS_VACIOS);
  const [form, setForm] = useState(CAMPOS_VACIOS);

  const [logoUrlGuardado, setLogoUrlGuardado] = useState("");
  const [logo, setLogo] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState("");

  useEffect(() => {
    if (usuarioId === null) return;

    const cargarPerfil = async () => {
      setCargando(true);
      setLoading(true);
      try {
        const data = await perfilDealerService.obtenerPerfil(usuarioId);

        const valores = mapearCampos(data);
        setForm(valores);
        setCamposGuardados(valores);
        setLogoUrlGuardado(data.logoUrl ?? "");
        setLogoPreview(data.logoUrl ?? "");
        setPerfilExiste(true);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (error: any) {
        if (error?.response?.status === 404) {
          // Primera vez: aún no hay perfil creado en la base de datos
          setPerfilExiste(false);
          setForm(CAMPOS_VACIOS);
          setCamposGuardados(CAMPOS_VACIOS);
          setLogoUrlGuardado("");
          setLogoPreview("");
        } else {
          console.error(error);
          Swal.fire({
            title: "Error",
            text: error.message || "No se pudo cargar tu perfil.",
            icon: "error",
            confirmButtonColor: "#ef4444",
          });
        }
      } finally {
        setCargando(false);
        setLoading(false);
      }
    };

    cargarPerfil();
  }, [usuarioId, setLoading]);

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
    setLogoPreview(URL.createObjectURL(archivo));
  };

  const comenzarEnEdicion = () => setModoEdicion(true);

  const cancelarEdicion = () => {
    setForm(camposGuardados);
    setLogo(null);
    setLogoPreview(logoUrlGuardado);
    setModoEdicion(false);
  };

  const handleGuardar = async (event: React.FormEvent) => {
    event.preventDefault();

    if (usuarioId === null) return;
    if (!modoEdicion || !hayCambios) return;

    setLoading(true);
    try {
      const data = await perfilDealerService.actualizarPerfil({
        ...form,
        logo: logo ?? undefined,
      });

      const valores = mapearCampos(data);
      setCamposGuardados(valores);
      setForm(valores);
      setLogoUrlGuardado(data.logoUrl ?? "");
      setLogoPreview(data.logoUrl ?? "");
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

      <form
        onSubmit={handleGuardar}
        className="space-y-6 rounded-2xl border border-gray-200 bg-white p-6 shadow-sm"
      >
        {/* LOGO */}
        <div>
          <label className="mb-2 block text-sm font-medium text-gray-700">
            Logo de la agencia
          </label>

          <div className="flex items-center gap-4">
            <div className="h-24 w-24 overflow-hidden rounded-xl border border-gray-200 bg-gray-100">
              {logoPreview ? (
                <img
                  src={logoPreview}
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
              <label className="cursor-pointer rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-50">
                {logo ? "Cambiar logo seleccionado" : "Subir nuevo logo"}
                <input
                  type="file"
                  accept="image/png, image/jpeg, image/webp"
                  onChange={handleLogo}
                  className="hidden"
                />
              </label>
            )}

            {logo && (
              <button
                type="button"
                onClick={() => {
                  setLogo(null);
                  setLogoPreview(logoUrlGuardado);
                }}
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
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Nombre de la agencia
            </label>
            <input
              type="text"
              name="nombreAgencia"
              value={form.nombreAgencia}
              onChange={handleChange}
              disabled={!modoEdicion}
              required
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Ubicación
            </label>
            <input
              type="text"
              name="ubicacion"
              value={form.ubicacion}
              onChange={handleChange}
              disabled={!modoEdicion}
              required
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Teléfono
            </label>
            <input
              type="tel"
              name="telefonoAgencia"
              value={form.telefonoAgencia}
              onChange={handleChange}
              disabled={!modoEdicion}
              required
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
            />
          </div>

          <div>
            <label className="mb-1 flex items-center gap-1.5 text-sm font-medium text-gray-700">
              <FaWhatsapp className="text-green-600" />
              WhatsApp
            </label>
            <input
              type="text"
              name="whatsApp"
              value={form.whatsApp}
              onChange={handleChange}
              disabled={!modoEdicion}
              placeholder="+1 809 000 0000"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
            />
          </div>

          <div className="md:col-span-2">
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Horarios de atención
            </label>
            <input
              type="text"
              name="horarios"
              value={form.horarios}
              onChange={handleChange}
              disabled={!modoEdicion}
              placeholder="Lunes a Sábado, 9:00 AM - 6:00 PM"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200 disabled:bg-gray-50 disabled:text-gray-600"
            />
          </div>

          <div className="md:col-span-2">
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Descripción
            </label>
            <textarea
              name="descripcion"
              value={form.descripcion}
              onChange={handleChange}
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
              onClick={cancelarEdicion}
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
    </div>
  );
}
