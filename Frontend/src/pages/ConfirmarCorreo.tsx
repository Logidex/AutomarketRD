import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { FaCheckCircle, FaExclamationTriangle, FaEnvelope } from "react-icons/fa";
import { authService } from "../services/auth.service";
import logo from "../assets/AutoMarketRD_Logo.svg";
import Spinner from "../components/ui/Spinner";

export default function ConfirmarCorreo() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";

  const [estado, setEstado] = useState<"cargando" | "exito" | "error">(() =>
    token ? "cargando" : "error",
  );
  const [mensaje, setMensaje] = useState(() =>
    token ? "" : "El enlace de confirmación es inválido o incompleto.",
  );

  useEffect(() => {
    if (!token) return;

    let activo = true;

    authService
      .confirmarCorreo(token)
      .then(() => {
        if (!activo) return;
        setEstado("exito");
        setMensaje("Tu correo fue confirmado exitosamente.");
      })
      .catch((err) => {
        if (!activo) return;
        setEstado("error");
        setMensaje(
          err instanceof Error ? err.message : "No se pudo confirmar tu correo.",
        );
      });

    return () => {
      activo = false;
    };
  }, [token]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-page p-4">
      <div className="w-full max-w-[520px] overflow-hidden rounded-2xl bg-white shadow-2xl">
        <div className="bg-surface p-8 text-center">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="mx-auto w-40 object-contain"
          />
        </div>

        <div className="p-10 text-center">
          {estado === "cargando" && (
            <div className="py-10">
              <Spinner />
              <p className="mt-4 text-sm text-gray-500">
                Confirmando tu correo...
              </p>
            </div>
          )}

          {estado === "exito" && (
            <>
              <FaCheckCircle className="mx-auto text-6xl text-green-500" />
              <h1 className="mt-5 text-2xl font-semibold text-gray-800">
                ¡Correo confirmado!
              </h1>
              <p className="mt-3 text-sm text-gray-500">
                {mensaje} Ahora tu cuenta Dealer puede obtener la insignia de{" "}
                <strong className="text-gray-700">Dealer Verificado</strong> al
                activar un plan de pago.
              </p>
              <div className="mt-8 flex flex-col gap-3">
                <Link
                  to="/login"
                  className="w-full rounded-lg bg-blue-500 py-3 font-semibold text-white transition-colors hover:bg-blue-600"
                >
                  Iniciar sesión
                </Link>
                <Link
                  to="/"
                  className="w-full rounded-lg border border-[#e1e7f0] py-3 font-medium text-gray-600 transition-colors hover:bg-[#f7f9fc]"
                >
                  Ir al inicio
                </Link>
              </div>
            </>
          )}

          {estado === "error" && (
            <>
              <FaExclamationTriangle className="mx-auto text-6xl text-amber-500" />
              <h1 className="mt-5 text-2xl font-semibold text-gray-800">
                No pudimos confirmar tu correo
              </h1>
              <p className="mt-3 text-sm text-gray-500">{mensaje}</p>
              <p className="mt-3 flex items-center justify-center gap-2 text-sm text-gray-500">
                <FaEnvelope className="text-blue-500" />
                Revisa tu bandeja de entrada o solicita un nuevo enlace desde tu
                panel de dealer.
              </p>
              <div className="mt-8 flex flex-col gap-3">
                <Link
                  to="/login"
                  className="w-full rounded-lg bg-blue-500 py-3 font-semibold text-white transition-colors hover:bg-blue-600"
                >
                  Iniciar sesión
                </Link>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}