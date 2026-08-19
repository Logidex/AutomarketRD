import { useState } from "react";
import Swal from "sweetalert2";
import { FaEnvelopeOpenText, FaKey, FaLock } from "react-icons/fa";
import { MdVisibility, MdVisibilityOff } from "react-icons/md";
import { usuarioService } from "../services/usuario.service";

const PASSWORD_MIN = 6;

export default function SeccionCambiarPassword() {
  const [pasoCodigo, setPasoCodigo] = useState(false);

  const [password, setPassword] = useState("");
  const [nuevaPassword, setNuevaPassword] = useState("");
  const [repetirPassword, setRepetirPassword] = useState("");
  const [mostrarActual, setMostrarActual] = useState(false);
  const [mostrarNueva, setMostrarNueva] = useState(false);
  const [mostrarRepetir, setMostrarRepetir] = useState(false);
  const [codigo, setCodigo] = useState("");
  const [enviando, setEnviando] = useState(false);

  const solicitarCodigo = async () => {
    if (!password || !nuevaPassword) {
      Swal.fire(
        "Campos incompletos",
        "Completa la contraseña actual y la nueva.",
        "warning",
      );
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
      const resultado = await usuarioService.cambiarPassword(password, nuevaPassword);
      Swal.fire("Código enviado", resultado.mensaje, "success");
      setPasoCodigo(true);
    } catch (err) {
      Swal.fire(
        "Error",
        err instanceof Error ? err.message : "No se pudo solicitar el cambio.",
        "error",
      );
    } finally {
      setEnviando(false);
    }
  };

  const confirmarCodigo = async () => {
    if (codigo.trim().length < 6) {
      Swal.fire(
        "Código incompleto",
        "Ingresa el código de 6 dígitos que recibiste.",
        "warning",
      );
      return;
    }

    setEnviando(true);
    try {
      const resultado = await usuarioService.confirmarCambioPassword(codigo.trim());
      Swal.fire("Contraseña actualizada", resultado.mensaje, "success");
      setPasoCodigo(false);
      setCodigo("");
      setPassword("");
      setNuevaPassword("");
      setRepetirPassword("");
    } catch (err) {
      Swal.fire(
        "Error",
        err instanceof Error ? err.message : "El código no pudo confirmarse.",
        "error",
      );
    } finally {
      setEnviando(false);
    }
  };

  const inputClase =
    "w-full rounded-lg border border-line px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200";

  return (
    <div className="rounded-2xl border border-line bg-surface p-6 shadow-sm">
      <h2 className="flex items-center gap-2 text-lg font-bold text-ink">
        <FaKey className="text-yellow-600" />
        Cambiar contraseña
      </h2>
      <p className="mt-1 text-sm text-ink-3">
        {pasoCodigo
          ? "Paso 2 de 2: ingresa el código que enviamos a tu correo."
          : "Paso 1 de 2: confirma con tu contraseña actual. Recibirás un código en tu correo."}
      </p>

      {!pasoCodigo ? (
        <div className="mt-5 space-y-4">
          <div>
            <label
              htmlFor="passwordActual"
              className="mb-1 block text-sm font-medium text-ink-2"
            >
              Contraseña actual *
            </label>
            <div className="relative">
              <input
                id="passwordActual"
                type={mostrarActual ? "text" : "password"}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={`${inputClase} pr-10`}
              />
              <button
                type="button"
                onClick={() => setMostrarActual((v) => !v)}
                aria-label={mostrarActual ? "Ocultar contraseña" : "Mostrar contraseña"}
                className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarActual ? "text-blue-500" : "text-ink-3"}`}
              >
                {mostrarActual ? <MdVisibilityOff /> : <MdVisibility />}
              </button>
            </div>
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label
                htmlFor="nuevaPassword"
                className="mb-1 block text-sm font-medium text-ink-2"
              >
                Nueva contraseña *
              </label>
              <div className="relative">
                <input
                  id="nuevaPassword"
                  type={mostrarNueva ? "text" : "password"}
                  value={nuevaPassword}
                  onChange={(e) => setNuevaPassword(e.target.value)}
                  className={`${inputClase} pr-10`}
                />
                <button
                  type="button"
                  onClick={() => setMostrarNueva((v) => !v)}
                  aria-label={mostrarNueva ? "Ocultar contraseña" : "Mostrar contraseña"}
                  className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarNueva ? "text-blue-500" : "text-ink-3"}`}
                >
                  {mostrarNueva ? <MdVisibilityOff /> : <MdVisibility />}
                </button>
              </div>
            </div>
            <div>
              <label
                htmlFor="repetirPassword"
                className="mb-1 block text-sm font-medium text-ink-2"
              >
                Repetir nueva contraseña *
              </label>
              <div className="relative">
                <input
                  id="repetirPassword"
                  type={mostrarRepetir ? "text" : "password"}
                  value={repetirPassword}
                  onChange={(e) => setRepetirPassword(e.target.value)}
                  className={`${inputClase} pr-10`}
                />
                <button
                  type="button"
                  onClick={() => setMostrarRepetir((v) => !v)}
                  aria-label={mostrarRepetir ? "Ocultar contraseña" : "Mostrar contraseña"}
                  className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarRepetir ? "text-blue-500" : "text-ink-3"}`}
                >
                  {mostrarRepetir ? <MdVisibilityOff /> : <MdVisibility />}
                </button>
              </div>
            </div>
          </div>

          <button
            type="button"
            onClick={solicitarCodigo}
            disabled={enviando}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
          >
            <FaEnvelopeOpenText />
            Enviar código a mi correo
          </button>
        </div>
      ) : (
        <div className="mt-5 space-y-4">
          <div>
            <label
              htmlFor="codigoConfirmacion"
              className="mb-1 block text-sm font-medium text-ink-2"
            >
              Código de confirmación (6 dígitos) *
            </label>
            <input
              id="codigoConfirmacion"
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.replace(/\D/g, ""))}
              className={`${inputClase} tracking-[0.5em]`}
            />
          </div>
          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={confirmarCodigo}
              disabled={enviando}
              className="inline-flex items-center gap-2 rounded-lg bg-green-600 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-green-700 disabled:cursor-not-allowed disabled:bg-green-300"
            >
              <FaLock />
              Confirmar cambio
            </button>
            <button
              type="button"
              onClick={() => {
                setPasoCodigo(false);
                setCodigo("");
              }}
              disabled={enviando}
              className="rounded-lg border border-line px-5 py-2.5 text-sm text-ink-2 transition-colors hover:bg-surface-2"
            >
              Volver
            </button>
          </div>
        </div>
      )}
    </div>
  );
}