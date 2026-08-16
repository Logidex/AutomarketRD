import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { FaEnvelope, FaPaperPlane } from "react-icons/fa";
import { authService } from "../services/auth.service";
import { usuarioService } from "../services/usuario.service";

export default function BannerConfirmarCorreo() {
  const [emailConfirmado, setEmailConfirmado] = useState<boolean | null>(null);
  const [email, setEmail] = useState("");
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    let activo = true;

    usuarioService
      .obtenerCuenta()
      .then((cuenta) => {
        if (!activo) return;
        setEmailConfirmado(cuenta.emailConfirmado);
        setEmail(cuenta.email);
      })
      .catch(() => {
        if (activo) setEmailConfirmado(true);
      });

    return () => {
      activo = false;
    };
  }, []);

  const reenviar = async () => {
    if (!email) return;

    setEnviando(true);
    try {
      const resultado = await authService.reenviarConfirmacion(email);
      await Swal.fire(
        "Correo enviado",
        resultado.mensaje,
        "success",
      );
    } catch (err) {
      await Swal.fire(
        "Error",
        err instanceof Error ? err.message : "No se pudo reenviar el correo.",
        "error",
      );
    } finally {
      setEnviando(false);
    }
  };

  if (emailConfirmado !== false) return null;

  return (
    <div className="mb-6 flex flex-col gap-4 rounded-2xl border border-amber-300 bg-amber-50 p-5 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-start gap-4">
        <div className="mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-amber-100">
          <FaEnvelope className="text-xl text-amber-600" />
        </div>
        <div>
          <p className="font-semibold text-amber-800">
            Tu correo aún no está confirmado
          </p>
          <p className="mt-1 text-sm text-amber-700">
            Confirma tu correo <strong className="text-amber-800">{email}</strong>{" "}
            para poder obtener la insignia de{" "}
            <strong className="text-amber-800">Dealer Verificado</strong> al
            activar un plan de pago. Revisa tu bandeja de entrada (y la carpeta
            de spam).
          </p>
        </div>
      </div>

      <button
        type="button"
        onClick={reenviar}
        disabled={enviando}
        className="inline-flex shrink-0 items-center justify-center gap-2 rounded-lg bg-amber-500 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-amber-600 disabled:cursor-not-allowed disabled:bg-amber-300"
      >
        <FaPaperPlane />
        {enviando ? "Enviando..." : "Reenviar correo"}
      </button>
    </div>
  );
}