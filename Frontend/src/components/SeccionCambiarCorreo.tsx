import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { FaEnvelopeOpenText, FaLock, FaCopyright } from "react-icons/fa";
import { authService } from "../services/auth.service";
import { usuarioService } from "../services/usuario.service";
import Spinner from "./Spinner";

const emailValido = (email: string) =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

export default function SeccionCambiarCorreo() {
  const [cargandoCuenta, setCargandoCuenta] = useState(true);
  const [emailActual, setEmailActual] = useState("");
  const [emailConfirmado, setEmailConfirmado] = useState(false);

  const [pasoCodigo, setPasoCodigo] = useState(false);
  const [password, setPassword] = useState("");
  const [nuevoEmail, setNuevoEmail] = useState("");
  const [codigo, setCodigo] = useState("");
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    let activo = true;

    usuarioService
      .obtenerCuenta()
      .then((cuenta) => {
        if (!activo) return;
        setEmailActual(cuenta.email);
        setEmailConfirmado(cuenta.emailConfirmado);
      })
      .catch(() => {})
      .finally(() => {
        if (activo) setCargandoCuenta(false);
      });

    return () => {
      activo = false;
    };
  }, []);

  const solicitarCodigo = async () => {
    if (!password || !nuevoEmail) {
      Swal.fire(
        "Campos incompletos",
        "Ingresa tu contraseña actual y el nuevo correo.",
        "warning",
      );
      return;
    }

    if (!emailValido(nuevoEmail)) {
      Swal.fire("Correo inválido", "Ingresa un correo electrónico válido.", "warning");
      return;
    }

    setEnviando(true);
    try {
      const resultado = await usuarioService.solicitarCambioEmail(
        password,
        nuevoEmail.trim(),
      );
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
      const resultado = await usuarioService.confirmarCambioEmail(codigo.trim());
      const emailNuevo = nuevoEmail.trim();

      authService.actualizarUsuario({ email: emailNuevo });
      setEmailActual(emailNuevo);
      setEmailConfirmado(true);

      Swal.fire("Correo actualizado", resultado.mensaje, "success");
      setPasoCodigo(false);
      setCodigo("");
      setPassword("");
      setNuevoEmail("");
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

  if (cargandoCuenta) {
    return (
      <div className="py-6">
        <Spinner />
      </div>
    );
  }

  const inputClase =
    "w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200";

  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
      <h2 className="flex items-center gap-2 text-lg font-bold text-gray-900">
        <FaCopyright className="text-green-600" />
        Cambiar correo electrónico
      </h2>
      <p className="mt-1 text-sm text-gray-500">
        {pasoCodigo
          ? "Paso 2 de 2: ingresa el código que enviamos al nuevo correo."
          : "Paso 1 de 2: confirma con tu contraseña y recibe un código en el nuevo correo."}
      </p>
      <p className="mt-2 text-sm text-gray-500">
        Correo actual:{" "}
        <strong className="text-gray-800">{emailActual}</strong>
        {emailConfirmado ? (
          <span className="ml-2 rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-700">
            confirmado
          </span>
        ) : (
          <span className="ml-2 rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-700">
            sin confirmar
          </span>
        )}
      </p>

      <div className="mt-5 space-y-4">
        {!pasoCodigo ? (
          <>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">
                Contraseña actual *
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={inputClase}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">
                Nuevo correo *
              </label>
              <input
                type="email"
                maxLength={150}
                value={nuevoEmail}
                onChange={(e) => setNuevoEmail(e.target.value)}
                className={inputClase}
              />
            </div>
            <button
              type="button"
              onClick={solicitarCodigo}
              disabled={enviando}
              className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
            >
              <FaEnvelopeOpenText />
              Enviar código al nuevo correo
            </button>
          </>
        ) : (
          <>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">
                Código de confirmación (6 dígitos) *
              </label>
              <input
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
                className="rounded-lg border border-gray-300 px-5 py-2.5 text-sm text-gray-600 transition-colors hover:bg-gray-50"
              >
                Volver
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}