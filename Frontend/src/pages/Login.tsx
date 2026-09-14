import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { motion } from "motion/react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import Swal from "sweetalert2";
import { FaArrowLeft } from "react-icons/fa";
import { MdVisibility, MdVisibilityOff } from "react-icons/md";
import { authService } from "../services/auth.service";
import SectionBackground from "../components/SectionBackground";
import logo from "../assets/AutoMarketRD_Logo.svg";

const loginSchema = z.object({
  email: z.string().min(1, "El email es requerido").email("Email inválido"),
  password: z.string().min(1, "La contraseña es requerida"),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function Login() {
  const [mostrarPassword, setMostrarPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setLoading(true);

    try {
      const response = await authService.login(data);

      if (!response.usuario) {
        await Swal.fire({
          icon: "error",
          title: "Respuesta incompleta",
          text: "El servidor no devolvió la información del usuario.",
          confirmButtonColor: "#3b82f6",
        });
        return;
      }

      await Swal.fire({
        icon: "success",
        title: "¡Bienvenido!",
        text: response.mensaje,
        timer: 1500,
        showConfirmButton: false,
      });

      const rol = response.usuario.rol?.trim().toLowerCase();

      if (rol === "dealer") {
        navigate("/dashboard", { replace: true });
        return;
      }
      if (rol === "vendedor") {
        navigate("/vendedor", { replace: true });
        return;
      }
      if (rol === "comprador") {
        navigate("/", { replace: true });
        return;
      }
      if (rol === "admin") {
        navigate("/admin", { replace: true });
        return;
      }

      authService.logout();

      await Swal.fire({
        icon: "error",
        title: "Rol no reconocido",
        text: "Tu cuenta tiene un rol no válido.",
        confirmButtonColor: "#3b82f6",
      });

      navigate("/login", { replace: true });
    } catch (err) {
      const status = (err as { response?: { status?: number } })?.response?.status;

      let mensaje = "Correo electrónico o contraseña incorrectos.";

      if (status === 429) {
        mensaje =
          "Has hecho demasiados intentos de inicio de sesión. " +
          "Por seguridad fuiste bloqueado temporalmente. " +
          "Espera unos 15 minutos y vuelve a intentarlo.";
      } else {
        const msg = (err as { message?: unknown })?.message;
        if (typeof msg === "string" && msg && msg !== "Network Error") {
          mensaje = msg;
        }
      }

      await Swal.fire({
        icon: "error",
        title: "Error al iniciar sesión",
        text: mensaje,
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative min-h-screen overflow-hidden bg-page flex items-center justify-center p-4">
      <SectionBackground variant="cta" className="w-full max-w-[950px]">
      <motion.div
        initial={{ opacity: 0, scale: 0.96 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.3, ease: "easeOut" }}
        className="w-full max-w-[950px] bg-surface rounded-2xl shadow-2xl overflow-hidden flex flex-col md:flex-row"
      >
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

          <h1 className="text-3xl font-bold mb-4">Bienvenido de nuevo</h1>
          <p className="text-white/70 mb-10">
            Accede a tu cuenta para explorar los mejores vehículos del mercado.
          </p>

          <div className="flex-1 flex items-center justify-center">
            <div className="w-40 h-40 rounded-full bg-gradient-to-br from-blue-500/20 to-transparent flex items-center justify-center relative">
              <div className="w-36 h-36 border-2 border-white/5 border-t-blue-500 rounded-full animate-spin"></div>
            </div>
          </div>
        </div>

        {/* Columna Derecha - Formulario */}
        <div className="md:flex-[1.2] bg-surface p-12 flex flex-col justify-center">
          <Link
            to="/"
            className="mb-4 inline-flex items-center gap-2 text-sm font-medium text-ink-3 transition-colors hover:text-blue-500"
          >
            <FaArrowLeft />
            Volver al inicio
          </Link>

          <h2 className="text-2xl font-semibold mb-6 text-ink">
            Iniciar Sesión
          </h2>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <div>
              <label
                htmlFor="loginEmail"
                className="block text-sm font-medium text-ink-2 mb-2"
              >
                Email
              </label>
              <input
                id="loginEmail"
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
                htmlFor="loginPassword"
                className="block text-sm font-medium text-ink-2 mb-2"
              >
                Contraseña
              </label>
              <div className="relative">
                <input
                  id="loginPassword"
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

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-blue-500 text-white rounded-lg font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors"
            >
              {loading ? "Iniciando..." : "Iniciar Sesión"}
            </button>
          </form>

          <div className="mt-4 text-center">
            <Link
              to="/recuperar-password"
              className="text-sm text-ink-3 transition-colors hover:text-blue-500"
            >
              ¿Olvidaste tu contraseña?
            </Link>
          </div>

          <p className="text-center text-sm text-ink-3 mt-6">
            ¿No tienes cuenta?{" "}
            <Link
              to="/registro"
              className="text-blue-500 font-semibold hover:underline"
            >
              Regístrate aquí
            </Link>
          </p>
        </div>
      </motion.div>
      </SectionBackground>
    </div>
  );
}
