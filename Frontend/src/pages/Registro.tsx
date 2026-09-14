import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { AnimatePresence, motion } from "motion/react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import Swal from "sweetalert2";
import { MdVisibility, MdVisibilityOff } from "react-icons/md";
import { authService } from "../services/auth.service";
import SectionBackground from "../components/SectionBackground";
import logo from "../assets/AutoMarketRD_Logo.svg";

const registroSchema = z.object({
  nombre: z.string().min(1, "El nombre es requerido"),
  apellido: z.string().min(1, "El apellido es requerido"),
  email: z.string().min(1, "El email es requerido").email("Email inválido"),
  password: z.string().min(6, "La contraseña debe tener al menos 6 caracteres"),
  confirmarPassword: z.string(),
  rol: z.enum(["Comprador", "Vendedor", "Dealer"]),
  telefonoPersonal: z.string().optional(),
  nombreAgencia: z.string().optional(),
  agenciaRNC: z.string().optional(),
  ubicacionAgencia: z.string().optional(),
  telefonoAgencia: z.string().optional(),
  aceptaTerminos: z.boolean().optional(),
}).refine((data) => data.password === data.confirmarPassword, {
  message: "Las contraseñas no coinciden",
  path: ["confirmarPassword"],
}).refine(
  (data) => {
    if (data.rol === "Dealer") {
      return data.nombreAgencia && data.agenciaRNC && data.ubicacionAgencia && data.telefonoAgencia;
    }
    return true;
  },
  {
    message: "Los campos de agencia son requeridos para Dealers",
    path: ["nombreAgencia"],
  }
);

type RegistroFormData = z.infer<typeof registroSchema>;

interface CamposDealerProps {
  errors: ReturnType<typeof useForm<RegistroFormData>>["formState"]["errors"];
  register: ReturnType<typeof useForm<RegistroFormData>>["register"];
}

function CamposDealer({ errors, register }: CamposDealerProps) {
  return (
    <div className="border-t border-line pt-5 mt-3">
      <h3 className="text-lg font-semibold text-blue-500 mb-4">
        Información de la Agencia
      </h3>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label
            htmlFor="nombreAgencia"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            Nombre de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="nombreAgencia"
            type="text"
            {...register("nombreAgencia")}
            placeholder="AutoVentas RD"
            className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
              errors.nombreAgencia ? "border-red-500" : "border-line"
            }`}
          />
          {errors.nombreAgencia && (
            <p className="mt-1 text-sm text-red-500">{errors.nombreAgencia.message}</p>
          )}
        </div>
        <div>
          <label
            htmlFor="agenciaRNC"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            RNC de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="agenciaRNC"
            type="text"
            {...register("agenciaRNC")}
            placeholder="1-30-12345-6"
            className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
              errors.agenciaRNC ? "border-red-500" : "border-line"
            }`}
          />
          {errors.agenciaRNC && (
            <p className="mt-1 text-sm text-red-500">{errors.agenciaRNC.message}</p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <div>
          <label
            htmlFor="ubicacionAgencia"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            Ubicación de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="ubicacionAgencia"
            type="text"
            {...register("ubicacionAgencia")}
            placeholder="Santo Domingo"
            className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
              errors.ubicacionAgencia ? "border-red-500" : "border-line"
            }`}
          />
          {errors.ubicacionAgencia && (
            <p className="mt-1 text-sm text-red-500">{errors.ubicacionAgencia.message}</p>
          )}
        </div>
        <div>
          <label
            htmlFor="telefonoAgencia"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            Teléfono de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="telefonoAgencia"
            type="tel"
            {...register("telefonoAgencia")}
            placeholder="809-555-5555"
            className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
              errors.telefonoAgencia ? "border-red-500" : "border-line"
            }`}
          />
          {errors.telefonoAgencia && (
            <p className="mt-1 text-sm text-red-500">{errors.telefonoAgencia.message}</p>
          )}
        </div>
      </div>
    </div>
  );
}

export default function Registro() {
  const [loading, setLoading] = useState(false);
  const [mostrarPassword, setMostrarPassword] = useState(false);
  const [mostrarConfirmacion, setMostrarConfirmacion] = useState(false);
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<RegistroFormData>({
    resolver: zodResolver(registroSchema),
    defaultValues: {
      rol: "Comprador",
      aceptaTerminos: undefined,
    },
  });

  const rolSeleccionado = watch("rol");
  const esDealer = rolSeleccionado === "Dealer";

  const onSubmit = async (data: RegistroFormData) => {
    if (!data.aceptaTerminos) {
      await Swal.fire({
        icon: "warning",
        title: "Falta aceptar los términos",
        text: "Debes aceptar los Términos y Condiciones y la Política de Privacidad para crear tu cuenta.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    setLoading(true);

    try {
      const response = await authService.register({
        ...data,
        aceptaTerminos: true,
        ubicacionAgencia: data.ubicacionAgencia ?? "",
        telefonoAgencia: data.telefonoAgencia ?? "",
      });

      if (response.exito) {
        const esDealerRegistrado = data.rol === "Dealer";

        if (esDealerRegistrado) {
          try {
            await authService.login({
              email: data.email,
              password: data.password,
            });
          } catch {
            // Si el auto-login falla, la página de suscripción lo redirigirá a /login
          }

          await Swal.fire({
            icon: "success",
            title: "¡Registro exitoso!",
            html: `Tu cuenta Dealer fue creada con el plan <strong>Gratis</strong> (1 anuncio).<br/><br/>Te enviamos un correo de confirmación a <strong>${data.email}</strong>. Confírmalo para poder obtener la insignia de <strong>Dealer Verificado</strong>.<br/><br/>⭐ <strong>No te pierdas este paso:</strong> entra a tu panel y completa <strong>Mi Perfil</strong> con el logo, horarios y descripción de tu agencia — los compradores confían más en perfiles completos.<br/><br/>Ahora elige la suscripción que mejor se adapte a tu agencia.`,
            confirmButtonColor: "#3b82f6",
          });
          navigate("/suscripcion");
        } else {
          await Swal.fire({
            icon: "success",
            title: "¡Registro exitoso!",
            text: response.mensaje,
            confirmButtonColor: "#3b82f6",
          });
          navigate("/login");
        }
      } else {
        await Swal.fire({
          icon: "error",
          title: "Error al registrarse",
          text: response.mensaje,
          confirmButtonColor: "#3b82f6",
        });
      }
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error al registrarse",
        text: err instanceof Error ? err.message : "Error en el servidor",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative min-h-screen overflow-hidden bg-page flex items-center justify-center p-4">
      <SectionBackground variant="cta" className="w-full max-w-[950px]">
      <div className="w-full max-w-[950px] bg-surface rounded-2xl shadow-2xl overflow-hidden flex flex-col md:flex-row">

        {/* Columna Izquierda - Visual */}
        <div className="md:flex-1 bg-[#11141a] p-12 text-white flex flex-col">
          <div className="mb-10">
            <div className="flex items-center justify-center w-full">
              <img
                src={logo}
                alt="AutoMarket RD"
                className="w-48 md:w-56 object-contain"
              />
            </div>
          </div>

          <h1 className="text-3xl font-bold mb-4">
            Únete al mercado
          </h1>
          <p className="text-white/70 mb-10">
            Crea tu cuenta y comienza a explorar o publicar vehículos en
            AutoMarket RD.
          </p>

          <div className="flex-1 flex items-center justify-center">
            <div className="w-40 h-40 rounded-full bg-gradient-to-br from-blue-500/20 to-transparent flex items-center justify-center relative">
              <div className="w-36 h-36 border-2 border-white/5 border-t-blue-500 rounded-full animate-spin"></div>
            </div>
          </div>
        </div>

        {/* Columna Derecha - Formulario */}
        <div className="md:flex-[1.2] bg-surface p-12 flex flex-col justify-center">
          <h2 className="text-2xl font-semibold mb-6 text-ink">
            Crear Cuenta
          </h2>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            {/* Nombre y Apellido */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="nombre"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Nombre
                </label>
                <input
                  id="nombre"
                  type="text"
                  {...register("nombre")}
                  placeholder="Juan"
                  className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
                    errors.nombre ? "border-red-500" : "border-line"
                  }`}
                />
                {errors.nombre && (
                  <p className="mt-1 text-sm text-red-500">{errors.nombre.message}</p>
                )}
              </div>
              <div>
                <label
                  htmlFor="apellido"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Apellido
                </label>
                <input
                  id="apellido"
                  type="text"
                  {...register("apellido")}
                  placeholder="Pérez"
                  className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
                    errors.apellido ? "border-red-500" : "border-line"
                  }`}
                />
                {errors.apellido && (
                  <p className="mt-1 text-sm text-red-500">{errors.apellido.message}</p>
                )}
              </div>
            </div>

            {/* Email y Teléfono */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="email"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Email
                </label>
                <input
                  id="email"
                  type="email"
                  {...register("email")}
                  placeholder="tu@email.com"
                  className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
                    errors.email ? "border-red-500" : "border-line"
                  }`}
                />
                {errors.email && (
                  <p className="mt-1 text-sm text-red-500">{errors.email.message}</p>
                )}
              </div>
              <div>
                <label
                  htmlFor="telefonoPersonal"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Teléfono Personal
                </label>
                <input
                  id="telefonoPersonal"
                  type="tel"
                  {...register("telefonoPersonal")}
                  placeholder="809-555-5555"
                  className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                />
              </div>
            </div>

            {/* Contraseña y Confirmación */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="password"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Contraseña
                </label>
                <div className="relative">
                  <input
                    id="password"
                    type={mostrarPassword ? "text" : "password"}
                    {...register("password")}
                    placeholder="••••••••"
                    className={`w-full px-4 py-3 pr-12 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
                      errors.password ? "border-red-500" : "border-line"
                    }`}
                  />
                  <button
                    type="button"
                    onClick={() => setMostrarPassword((v) => !v)}
                    aria-label={mostrarPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
                    className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarPassword ? "text-blue-500" : "text-ink-3"}`}
                  >
                    {mostrarPassword ? <MdVisibilityOff /> : <MdVisibility />}
                  </button>
                </div>
                {errors.password && (
                  <p className="mt-1 text-sm text-red-500">{errors.password.message}</p>
                )}
              </div>
              <div>
                <label
                  htmlFor="confirmarPassword"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Confirmar Contraseña
                </label>
                <div className="relative">
                  <input
                    id="confirmarPassword"
                    type={mostrarConfirmacion ? "text" : "password"}
                    {...register("confirmarPassword")}
                    placeholder="••••••••"
                    className={`w-full px-4 py-3 pr-12 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
                      errors.confirmarPassword ? "border-red-500" : "border-line"
                    }`}
                  />
                  <button
                    type="button"
                    onClick={() => setMostrarConfirmacion((v) => !v)}
                    aria-label={mostrarConfirmacion ? "Ocultar contraseña" : "Mostrar contraseña"}
                    className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarConfirmacion ? "text-blue-500" : "text-ink-3"}`}
                  >
                    {mostrarConfirmacion ? <MdVisibilityOff /> : <MdVisibility />}
                  </button>
                </div>
                {errors.confirmarPassword && (
                  <p className="mt-1 text-sm text-red-500">{errors.confirmarPassword.message}</p>
                )}
              </div>
            </div>

            {/* Tipo de Cuenta */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="rol"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Tipo de Cuenta
                </label>
                <select
                  id="rol"
                  {...register("rol")}
                  className={`w-full px-4 py-3 bg-input text-ink border rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3 ${
                    errors.rol ? "border-red-500" : "border-line"
                  }`}
                >
                  <option value="Comprador">Comprador</option>
                  <option value="Vendedor">Vendedor</option>
                  <option value="Dealer">Dealer</option>
                </select>
              </div>
            </div>

            {/* Campos exclusivos para Dealer */}
            <AnimatePresence initial={false}>
              {esDealer && (
                <motion.div
                  key="campos-dealer"
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: "auto" }}
                  exit={{ opacity: 0, height: 0 }}
                  transition={{ duration: 0.25, ease: "easeInOut" }}
                  className="overflow-hidden"
                >
                  <CamposDealer errors={errors} register={register} />
                </motion.div>
              )}
            </AnimatePresence>

            {/* Aceptación de términos */}
            <div className="flex items-start gap-3">
              <input
                id="aceptaTerminos"
                type="checkbox"
                {...register("aceptaTerminos")}
                className="mt-0.5 h-4 w-4 shrink-0 cursor-pointer rounded border-line accent-blue-600"
              />
              <label
                htmlFor="aceptaTerminos"
                className="cursor-pointer text-sm leading-6 text-ink-2"
              >
                Acepto los{" "}
                <Link
                  to="/terminos"
                  target="_blank"
                  className="font-semibold text-blue-500 hover:underline"
                >
                  Términos y Condiciones
                </Link>{" "}
                y la{" "}
                <Link
                  to="/privacidad"
                  target="_blank"
                  className="font-semibold text-blue-500 hover:underline"
                >
                  Política de Privacidad
                </Link>
              </label>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-blue-500 text-white rounded-lg font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors mt-4"
            >
              {loading ? "Registrando..." : "Registrarse"}
            </button>
          </form>

          <p className="text-center text-sm text-ink-3 mt-6">
            ¿Ya tienes cuenta?{" "}
            <Link to="/login" className="text-blue-500 font-semibold hover:underline">
              Inicia sesión aquí
            </Link>
          </p>
        </div>
      </div>
      </SectionBackground>
    </div>
  );
}
