import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import {
  FaArrowLeft,
  FaEnvelope,
  FaEye,
  FaEyeSlash,
  FaKey,
  FaLock,
  FaPaperPlane,
} from "react-icons/fa";
import { authService } from "../services/auth.service";
import logo from "../assets/AutoMarketRD_Logo.svg";

const PASSWORD_MIN = 6;

export default function RecuperarPassword() {
  const navigate = useNavigate();

  const [paso, setPaso] = useState<"email" | "codigo">("email");
  const [email, setEmail] = useState("");
  const [codigo, setCodigo] = useState("");
  const [nuevaPassword, setNuevaPassword] = useState("");
  const [repetirPassword, setRepetirPassword] = useState("");
  const [mostrarNueva, setMostrarNueva] = useState(false);
  const [mostrarRepetir, setMostrarRepetir] = useState(false);
  const [enviando, setEnviando] = useState(false);

  const enviarCodigo = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      Swal.fire("Correo inválido", "Ingresa un correo electrónico válido.", "warning");
      return;
    }

    setEnviando(true);
    try {
      const resultado = await authService.solicitarRecuperacion(email);
      Swal.fire("Revisa tu correo", resultado.mensaje, "success");
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

  const restablecer = async (e: React.FormEvent) => {
    e.preventDefault();

    if (codigo.trim().length < 6) {
      Swal.fire("Código incompleto", "Ingresa el código de 6 dígitos que recibiste.", "warning");
      return;
    }

    if (nuevaPassword.length < PASSWORD_MIN) {
      Swal.fire(
        "Contraseña corta",
        `La nueva contraseña debe tener al menos ${PASSWORD_MIN} caracteres.`,
        "warning",
      );
      return;
    }

    if (nuevaPassword !== repetirPassword) {
      Swal.fire(
        "No coinciden",
        "La nueva contraseña y su confirmación no coinciden.",
        "warning",
      );
      return;
    }

    setEnviando(true);
    try {
      const resultado = await authService.restablecerPassword({
        email,
        codigo,
        nuevaPassword,
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

  const inputClase =
    "w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors";

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#0c101b] p-4">
      <div className="w-full max-w-[950px] overflow-hidden rounded-2xl bg-white shadow-2xl flex flex-col md:flex-row">
        {/* Columna Izquierda - Visual */}
        <div className="bg-[#0e1422] p-12 text-white flex flex-col md:flex-1">
          <div className="mb-10 flex justify-center">
            <img
              src={logo}
              alt="AutoMarket RD"
              className="w-48 md:w-56 object-contain"
            />
          </div>

          <h1 className="mb-4 text-3xl font-bold">Recupera tu acceso</h1>
          <p className="mb-10 text-[#9aa1b1]">
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

              <form onSubmit={enviarCodigo} className="space-y-5">
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
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="tu@email.com"
                      className={`${inputClase} pl-11`}
                      required
                    />
                  </div>
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
                Código enviado a <strong className="text-gray-700">{email}</strong>. Vigencia: 15 minutos.
              </p>

              <form onSubmit={restablecer} className="space-y-5">
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
                      maxLength={6}
                      value={codigo}
                      onChange={(e) => setCodigo(e.target.value.replace(/\D/g, ""))}
                      placeholder="123456"
                      className={`${inputClase} pl-11 tracking-[0.5em]`}
                      required
                    />
                  </div>
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
                      value={nuevaPassword}
                      onChange={(e) => setNuevaPassword(e.target.value)}
                      placeholder="••••••••"
                      className={`${inputClase} pl-11 pr-12`}
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setMostrarNueva((v) => !v)}
                      aria-label={mostrarNueva ? "Ocultar contraseña" : "Mostrar contraseña"}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 transition-colors hover:text-blue-500"
                    >
                      {mostrarNueva ? <FaEyeSlash /> : <FaEye />}
                    </button>
                  </div>
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
                      value={repetirPassword}
                      onChange={(e) => setRepetirPassword(e.target.value)}
                      placeholder="••••••••"
                      className={`${inputClase} pr-12`}
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setMostrarRepetir((v) => !v)}
                      aria-label={mostrarRepetir ? "Ocultar contraseña" : "Mostrar contraseña"}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 transition-colors hover:text-blue-500"
                    >
                      {mostrarRepetir ? <FaEyeSlash /> : <FaEye />}
                    </button>
                  </div>
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