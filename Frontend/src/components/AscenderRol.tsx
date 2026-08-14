import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import {
  FaLevelUpAlt,
  FaStore,
  FaUserEdit,
} from "react-icons/fa";
import { authService } from "../services/auth.service";
import { usuarioService, type AscenderRolDto } from "../services/usuario.service";
import { ROLES } from "../constants/roles";
import type { AuthResponse } from "../types/auth.types";

export default function AscenderRol() {
  const navigate = useNavigate();
  const usuario = authService.getCurrentUser();

  const [mostrarFormDealer, setMostrarFormDealer] = useState(false);
  const [cargando, setCargando] = useState(false);
  const [agencia, setAgencia] = useState({
    nombreAgencia: "",
    agenciaRNC: "",
    ubicacionAgencia: "",
    telefonoAgencia: "",
  });

  if (!usuario) return null;

  const aplicarSesionYRedirigir = (respuesta: AuthResponse) => {
    authService.guardarSesion(respuesta);

    const rol = respuesta.usuario?.rol;
    if (rol === ROLES.DEALER) {
      navigate("/suscripcion", { replace: true });
    } else if (rol === ROLES.VENDEDOR) {
      navigate("/vendedor", { replace: true });
    } else {
      navigate("/", { replace: true });
    }
  };

  const ascender = async (dto: AscenderRolDto, mensajeExito: string) => {
    setCargando(true);
    try {
      const respuesta = await usuarioService.ascenderRol(dto);
      await Swal.fire({
        icon: "success",
        title: "¡Cuenta actualizada!",
        text: respuesta.mensaje || mensajeExito,
        confirmButtonColor: "#3b82f6",
      });
      aplicarSesionYRedirigir(respuesta);
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "No se pudo actualizar tu cuenta",
        text: err instanceof Error ? err.message : "Error en el servidor",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setCargando(false);
    }
  };

  const confirmarAsVendedor = async () => {
    const confirmacion = await Swal.fire({
      icon: "question",
      title: "¿Convertirte en Vendedor?",
      text: "Podrás publicar tu vehículo de forma gratuita. Esta acción no requiere datos adicionales.",
      showCancelButton: true,
      confirmButtonText: "Sí, convertirme",
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#3b82f6",
    });

    if (!confirmacion.isConfirmed) return;
    await ascender({ nuevoRol: "Vendedor" }, "Tu cuenta ahora es de tipo Vendedor.");
  };

  const confirmarAsDealer = async (e: React.FormEvent) => {
    e.preventDefault();
    await ascender(
      {
        nuevoRol: "Dealer",
        nombreAgencia: agencia.nombreAgencia.trim(),
        agenciaRNC: agencia.agenciaRNC.trim(),
        ubicacionAgencia: agencia.ubicacionAgencia.trim(),
        telefonoAgencia: agencia.telefonoAgencia.trim(),
      },
      "Tu cuenta ahora es de tipo Dealer. Se te asignó el plan Gratis."
    );
  };

  const esComprador = usuario.rol === ROLES.COMPRADOR;
  const esVendedor = usuario.rol === ROLES.VENDEDOR;

  return (
    <>
      <h2 className="flex items-center gap-2 text-base font-bold text-white">
        <FaLevelUpAlt className="text-blue-400" />
        Empieza a vender
      </h2>

      {esComprador && (
        <p>
          Tu cuenta es de <strong className="text-gray-200">Comprador</strong>.
          Puedes escalarla cuando lo desees:
        </p>
      )}

      {esVendedor && (
        <p>
          Tu cuenta es de <strong className="text-gray-200">Vendedor</strong>.
          Puedes escalarla a <strong className="text-gray-200">Dealer</strong>{" "}
          para publicar más anuncios y acceder a suscripciones de pago:
        </p>
      )}

      <div className="mt-2 grid grid-cols-1 gap-3 sm:grid-cols-2">
        {esComprador && (
          <button
            type="button"
            onClick={confirmarAsVendedor}
            disabled={cargando}
            className="flex items-center justify-center gap-2 rounded-lg bg-white/5 px-5 py-3 font-semibold text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <FaUserEdit className="text-green-400" />
            Convertirme en Vendedor
          </button>
        )}
        <button
          type="button"
          onClick={() => {
            setMostrarFormDealer((v) => !v);
            setAgencia({
              nombreAgencia: "",
              agenciaRNC: "",
              ubicacionAgencia: "",
              telefonoAgencia: "",
            });
          }}
          disabled={cargando}
          className="flex items-center justify-center gap-2 rounded-lg bg-white/5 px-5 py-3 font-semibold text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <FaStore className="text-yellow-400" />
          Convertirme en Dealer
        </button>
      </div>

      {mostrarFormDealer && (
        <form onSubmit={confirmarAsDealer} className="mt-4 space-y-4">
          <p className="rounded-lg border border-blue-500/30 bg-blue-500/10 px-3 py-2 text-xs text-blue-300">
            Al convertirte en Dealer tu cuenta quedará con el plan{" "}
            <strong>Gratis</strong> (1 anuncio). Después podrás elegir una
            suscripción de pago y pagarla con PayPal.
          </p>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label
                htmlFor="nombreAgencia"
                className="mb-1 block text-xs font-medium text-[#9aa1b1]"
              >
                Nombre de la Agencia *
              </label>
              <input
                id="nombreAgencia"
                type="text"
                required
                value={agencia.nombreAgencia}
                onChange={(e) =>
                  setAgencia({ ...agencia, nombreAgencia: e.target.value })
                }
                placeholder="AutoVentas RD"
                className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>
            <div>
              <label
                htmlFor="agenciaRNC"
                className="mb-1 block text-xs font-medium text-[#9aa1b1]"
              >
                RNC de la Agencia *
              </label>
              <input
                id="agenciaRNC"
                type="text"
                required
                value={agencia.agenciaRNC}
                onChange={(e) =>
                  setAgencia({ ...agencia, agenciaRNC: e.target.value })
                }
                placeholder="1-30-12345-6"
                className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>
            <div>
              <label
                htmlFor="ubicacionAgencia"
                className="mb-1 block text-xs font-medium text-[#9aa1b1]"
              >
                Ubicación de la Agencia
              </label>
              <input
                id="ubicacionAgencia"
                type="text"
                value={agencia.ubicacionAgencia}
                onChange={(e) =>
                  setAgencia({
                    ...agencia,
                    ubicacionAgencia: e.target.value,
                  })
                }
                placeholder="Santo Domingo"
                className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>
            <div>
              <label
                htmlFor="telefonoAgencia"
                className="mb-1 block text-xs font-medium text-[#9aa1b1]"
              >
                Teléfono de la Agencia
              </label>
              <input
                id="telefonoAgencia"
                type="tel"
                value={agencia.telefonoAgencia}
                onChange={(e) =>
                  setAgencia({
                    ...agencia,
                    telefonoAgencia: e.target.value,
                  })
                }
                placeholder="809-555-5555"
                className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={cargando}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-yellow-500 px-5 py-3 font-semibold text-[#0c101b] transition-colors hover:bg-yellow-400 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <FaStore />
            {cargando ? "Procesando..." : "Confirmar cuenta Dealer"}
          </button>
        </form>
      )}
    </>
  );
}