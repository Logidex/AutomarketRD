import { useEffect, useReducer, useState } from "react";
import Swal from "sweetalert2";
import { FaEnvelopeOpenText, FaLock, FaCopyright } from "react-icons/fa";
import { authService } from "../services/auth.service";
import { usuarioService } from "../services/usuario.service";
import Spinner from "./ui/Spinner";

const emailValido = (email: string) =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

interface CambioCorreoState {
  pasoCodigo: boolean;
  password: string;
  nuevoEmail: string;
  codigo: string;
}

const CAMBIO_CORREO_INICIAL: CambioCorreoState = {
  pasoCodigo: false,
  password: "",
  nuevoEmail: "",
  codigo: "",
};

type CambioCorreoAction =
  | { type: "password"; valor: string }
  | { type: "nuevoEmail"; valor: string }
  | { type: "codigo"; valor: string }
  | { type: "pasoCodigo" }
  | { type: "volver" }
  | { type: "reiniciar" };

function reducerCambioCorreo(
  estado: CambioCorreoState,
  accion: CambioCorreoAction,
): CambioCorreoState {
  switch (accion.type) {
    case "password":
      return { ...estado, password: accion.valor };
    case "nuevoEmail":
      return { ...estado, nuevoEmail: accion.valor };
    case "codigo":
      return { ...estado, codigo: accion.valor };
    case "pasoCodigo":
      return { ...estado, pasoCodigo: true };
    case "volver":
      return { ...estado, pasoCodigo: false, codigo: "" };
    case "reiniciar":
      return CAMBIO_CORREO_INICIAL;
  }
}

export default function SeccionCambiarCorreo() {
  const [cargandoCuenta, setCargandoCuenta] = useState(true);
  const [emailActual, setEmailActual] = useState("");
  const [emailConfirmado, setEmailConfirmado] = useState(false);
  const [enviando, setEnviando] = useState(false);

  const [cambioCorreo, dispatchCambioCorreo] = useReducer(
    reducerCambioCorreo,
    CAMBIO_CORREO_INICIAL,
  );
  const { pasoCodigo, password, nuevoEmail, codigo } = cambioCorreo;

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
      dispatchCambioCorreo({ type: "pasoCodigo" });
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
      dispatchCambioCorreo({ type: "reiniciar" });
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
    "w-full rounded-lg border border-line px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200";

  return (
    <div className="rounded-2xl border border-line bg-surface p-6 shadow-sm">
      <h2 className="flex items-center gap-2 text-lg font-bold text-ink">
        <FaCopyright className="text-green-600" />
        Cambiar correo electrónico
      </h2>
      <p className="mt-1 text-sm text-ink-3">
        {pasoCodigo
          ? "Paso 2 de 2: ingresa el código que enviamos al nuevo correo."
          : "Paso 1 de 2: confirma con tu contraseña y recibe un código en el nuevo correo."}
      </p>
      <p className="mt-2 text-sm text-ink-3">
        Correo actual:{" "}
        <strong className="text-ink">{emailActual}</strong>
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
              <label
                htmlFor="passwordActual"
                className="mb-1 block text-sm font-medium text-ink-2"
              >
                Contraseña actual *
              </label>
              <input
                id="passwordActual"
                type="password"
                value={password}
                onChange={(e) =>
                  dispatchCambioCorreo({ type: "password", valor: e.target.value })
                }
                className={inputClase}
              />
            </div>
            <div>
              <label
                htmlFor="nuevoEmail"
                className="mb-1 block text-sm font-medium text-ink-2"
              >
                Nuevo correo *
              </label>
              <input
                id="nuevoEmail"
                type="email"
                maxLength={150}
                value={nuevoEmail}
                onChange={(e) =>
                  dispatchCambioCorreo({ type: "nuevoEmail", valor: e.target.value })
                }
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
                onChange={(e) =>
                  dispatchCambioCorreo({
                    type: "codigo",
                    valor: e.target.value.replace(/\D/g, ""),
                  })
                }
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
                onClick={() => dispatchCambioCorreo({ type: "volver" })}
                disabled={enviando}
                className="rounded-lg border border-line px-5 py-2.5 text-sm text-ink-2 transition-colors hover:bg-surface-2"
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