import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import Swal from "sweetalert2";
import {
  FaArrowLeft,
  FaEnvelope,
  FaKey,
  FaLock,
  FaPaperPlane,
} from "react-icons/fa";
import { MdVisibility, MdVisibilityOff } from "react-icons/md";
import { authService } from "../services/auth.service";
import logo from "../assets/AutoMarketRD_Logo.svg";

const emailSchema = z.object({
  email: z.string().min(1, "El email es requerido").email("Email inválido"),
});

const resetSchema = z.object({
  codigo: z.string().min(6, "El código debe tener 6 dígitos").max(6),
  nuevaPassword: z.string().min(6, "La contraseña debe tener al menos 6 caracteres"),
  repetirPassword: z.string(),
}).refine((data) => data.nuevaPassword === data.repetirPassword, {
  message: "Las contraseñas no coinciden",
  path: ["repetirPassword"],
});

type EmailFormData = z.infer<typeof emailSchema>;
type ResetFormData = z.infer<typeof resetSchema>;

const inputClase =
  "w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors";

export default function RecuperarPassword() {
  const navigate = useNavigate();

  const [paso, setPaso] = useState<"email" | "codigo">("email");
  const [emailGuardado, setEmailGuardado] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [mostrarNueva, setMostrarNueva] = useState(false);
  const [mostrarRepetir, setMostrarRepetir] = useState(false);

  const emailForm = useForm<EmailFormData>({
    resolver: zodResolver(emailSchema),
  });

  const resetForm = useForm<ResetFormData>({
    resolver: zodResolver(resetSchema),
  });

  const enviarCodigo = async (data: EmailFormData) => {
    setEnviando(true);
    try {
      const resultado = await authService.solicitarRecuperacion(data.email);
      Swal.fire("Revisa tu correo", resultado.mensaje, "success");
      setEmailGuardado(data.email);
      setPaso("codigo");
    } catch (err) {
      Swal.fire(
        "Error",
        err instanceof Error ? err.message : "No se pudo solicitar la recuperación.",
        "error",
      );
    } finally {
      setEnviando(false);
    }
  };

  const restablecer = async (data: ResetFormData) => {
    setEnviando(true);
    try {
      const resultado = await authService.restablecerPassword({
        email: emailGuardado,
        codigo: data.codigo,
        nuevaPassword: data.nuevaPassword,
      });
      await Swal.fire("¡Listo!", resultado.mensaje, "success");
      navigate("/login", { replace: true });
    } catch (err) {
      Swal.fire(
        "Error",
        err instanceof Error ? err.message : "No se pudo restablecer la contraseña.",
        "error",
      );
    } finally {
      setEnviando(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-page p-4">
      <div className="w-full max-w-[950px] overflow-hidden rounded-2xl bg-white shadow-2xl flex flex-col md:flex-row">
        {/* Columna Izquierda - Visual */}
        <div className="bg-surface p-12 text-white flex flex-col md:flex-1">
          <div className="mb-10 flex justify-center">
            <img
              src={logo}
              alt="AutoMarket RD"
              className="w-48 md:w-56 object-contain"
            />
          </div>

          <h1 className="mb-4 text-3xl font-bold">Recupera tu acceso</h1>
          <p className="mb-10 text-ink-2">
            {paso === "email"
              ? "Ingresa tu correo y te enviaremos un código para restablecer tu contraseña."
              : "Ingresa el código que recibiste y define tu nueva contraseña."}
          </p>

          <div className="flex flex-1 items-center justify-center">
            <div className="flex h-40 w-40 items-center justify-center rounded-full bg-gradient-to-br from-blue-500/20 to-transparent">
              <FaLock className="text-5xl text-blue-400" />
            </div>
          </div>
        </div>

        {/* Columna Derecha - Formulario */}
        <div className="bg-white p-12 md:flex-[1.2] flex flex-col justify-center">
          <Link
            to="/login"
            className="mb-4 inline-flex items-center gap-2 text-sm font-medium text-gray-500 transition-colors hover:text-blue-500"
          >
            <FaArrowLeft />
            Volver a iniciar sesión
          </Link>

          {paso === "email" ? (
            <>
              <h2 className="mb-2 text-2xl font-semibold text-gray-800">
                ¿Olvidaste tu contraseña?
              </h2>
              <p className="mb-6 text-sm text-gray-500">
                Te enviaremos un código de 6 dígitos a tu correo.
              </p>

              <form onSubmit={emailForm.handleSubmit(enviarCodigo)} className="space-y-5">
                <div>
                  <label
                    htmlFor="emailRecuperacion"
                    className="mb-2 block text-sm font-medium text-gray-600"
                  >
                    Email
                  </label>
                  <div className="relative">
                    <FaEnvelope className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
                    <input
                      id="emailRecuperacion"
                      type="email"
                      {...emailForm.register("email")}
                      placeholder="tu@email.com"
                      className={`${inputClase} pl-11 ${
                        emailForm.formState.errors.email ? "border-red-500" : ""
                      }`}
                    />
                  </div>
                  {emailForm.formState.errors.email && (
                    <p className="mt-1 text-sm text-red-500">
                      {emailForm.formState.errors.email.message}
                    </p>
                  )}
                </div>

                <button
                  type="submit"
                  disabled={enviando}
                  className="w-full flex items-center justify-center gap-2 rounded-lg bg-blue-500 py-3.5 font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:bg-blue-300"
                >
                  <FaPaperPlane />
                  {enviando ? "Enviando..." : "Enviar código"}
                </button>
              </form>
            </>
          ) : (
            <>
              <h2 className="mb-2 text-2xl font-semibold text-gray-800">
                Ingresa el código y tu nueva contraseña
              </h2>
              <p className="mb-6 text-sm text-gray-500">
                Código enviado a <strong className="text-gray-700">{emailGuardado}</strong>. Vigencia: 15 minutos.
              </p>

              <form onSubmit={resetForm.handleSubmit(restablecer)} className="space-y-5">
                <div>
                  <label
                    htmlFor="codigoRecuperacion"
                    className="mb-2 block text-sm font-medium text-gray-600"
                  >
                    Código de confirmación (6 dígitos)
                  </label>
                  <div className="relative">
                    <FaKey className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
                    <input
                      id="codigoRecuperacion"
                      type="text"
                      inputMode="numeric"
                      {...resetForm.register("codigo")}
                      placeholder="123456"
                      className={`${inputClase} pl-11 tracking-[0.5em] ${
                        resetForm.formState.errors.codigo ? "border-red-500" : ""
                      }`}
                    />
                  </div>
                  {resetForm.formState.errors.codigo && (
                    <p className="mt-1 text-sm text-red-500">
                      {resetForm.formState.errors.codigo.message}
                    </p>
                  )}
                </div>

                <div>
                  <label
                    htmlFor="nuevaPassword"
                    className="mb-2 block text-sm font-medium text-gray-600"
                  >
                    Nueva contraseña
                  </label>
                  <div className="relative">
                    <FaLock className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
                    <input
                      id="nuevaPassword"
                      type={mostrarNueva ? "text" : "password"}
                      {...resetForm.register("nuevaPassword")}
                      placeholder="••••••••"
                      className={`${inputClase} pl-11 pr-12 ${
                        resetForm.formState.errors.nuevaPassword ? "border-red-500" : ""
                      }`}
                    />
                    <button
                      type="button"
                      onClick={() => setMostrarNueva((v) => !v)}
                      aria-label={mostrarNueva ? "Ocultar contraseña" : "Mostrar contraseña"}
                      className={`absolute right-4 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarNueva ? "text-blue-500" : "text-gray-400"}`}
                    >
                      {mostrarNueva ? <MdVisibilityOff /> : <MdVisibility />}
                    </button>
                  </div>
                  {resetForm.formState.errors.nuevaPassword && (
                    <p className="mt-1 text-sm text-red-500">
                      {resetForm.formState.errors.nuevaPassword.message}
                    </p>
                  )}
                </div>

                <div>
                  <label
                    htmlFor="repetirPassword"
                    className="mb-2 block text-sm font-medium text-gray-600"
                  >
                    Repetir nueva contraseña
                  </label>
                  <div className="relative">
                    <input
                      id="repetirPassword"
                      type={mostrarRepetir ? "text" : "password"}
                      {...resetForm.register("repetirPassword")}
                      placeholder="••••••••"
                      className={`${inputClase} pr-12 ${
                        resetForm.formState.errors.repetirPassword ? "border-red-500" : ""
                      }`}
                    />
                    <button
                      type="button"
                      onClick={() => setMostrarRepetir((v) => !v)}
                      aria-label={mostrarRepetir ? "Ocultar contraseña" : "Mostrar contraseña"}
                      className={`absolute right-4 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarRepetir ? "text-blue-500" : "text-gray-400"}`}
                    >
                      {mostrarRepetir ? <MdVisibilityOff /> : <MdVisibility />}
                    </button>
                  </div>
                  {resetForm.formState.errors.repetirPassword && (
                    <p className="mt-1 text-sm text-red-500">
                      {resetForm.formState.errors.repetirPassword.message}
                    </p>
                  )}
                </div>

                <button
                  type="submit"
                  disabled={enviando}
                  className="w-full flex items-center justify-center gap-2 rounded-lg bg-blue-500 py-3.5 font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:bg-blue-300"
                >
                  <FaLock />
                  {enviando ? "Restableciendo..." : "Restablecer contraseña"}
                </button>

                <button
                  type="button"
                  onClick={() => setPaso("email")}
                  className="w-full text-center text-sm text-gray-500 transition-colors hover:text-blue-500"
                >
                  ¿No recibiste el código? Reintentar
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
