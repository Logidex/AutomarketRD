import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { FaBan, FaCheckCircle, FaCoins, FaRedoAlt } from "react-icons/fa";
import {
  adminService,
  type UsuarioAdmin,
} from "../../services/admin.service";
import Spinner from "../../components/Spinner";
import { formatearFecha } from "../../utils/fecha";

const COLOR_ROL: Record<string, string> = {
  Admin: "bg-red-100 text-red-700",
  Dealer: "bg-blue-100 text-blue-700",
  Vendedor: "bg-violet-100 text-violet-700",
  Comprador: "bg-emerald-100 text-emerald-700",
};

export default function AdminUsuarios() {
  const [usuarios, setUsuarios] = useState<UsuarioAdmin[]>([]);
  const [loading, setLoading] = useState(true);
  const [procesando, setProcesando] = useState<number | null>(null);

  const cargar = async () => {
    try {
      const data = await adminService.listarUsuarios();
      setUsuarios(data);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: error.message || "No se pudieron cargar los usuarios.",
        confirmButtonColor: "#7c3aed",
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const inicializar = async () => {
      await cargar();
    };

    inicializar();
  }, []);

  const toggleEstado = async (usuario: UsuarioAdmin) => {
    if (procesando !== null) return;

    const reactivar = !usuario.isActivo;
    const resultado = await Swal.fire({
      icon: "warning",
      title: reactivar ? "Reactivar usuario" : "Suspender usuario",
      text: reactivar
        ? `¿Reactivar a ${usuario.nombre} ${usuario.apellido}?`
        : `¿Suspender a ${usuario.nombre} ${usuario.apellido}? No podrá iniciar sesión hasta ser reactivado.`,
      showCancelButton: true,
      confirmButtonColor: reactivar ? "#059669" : "#dc2626",
      confirmButtonText: reactivar ? "Sí, reactivar" : "Sí, suspender",
      cancelButtonText: "Cancelar",
    });

    if (!resultado.isConfirmed) return;

    setProcesando(usuario.usuarioId);
    try {
      const respuesta = reactivar
        ? await adminService.reactivarUsuario(usuario.usuarioId)
        : await adminService.suspenderUsuario(usuario.usuarioId);

      await Swal.fire({
        icon: "success",
        title: "Listo",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
      await cargar();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: error.message || "No se pudo actualizar el usuario.",
        confirmButtonColor: "#7c3aed",
      });
    } finally {
      setProcesando(null);
    }
  };

  const cambiarPlan = async (dealer: UsuarioAdmin) => {
    if (procesando !== null) return;

    const resultado = await Swal.fire({
      icon: "question",
      title: "Cambiar plan del dealer",
      text: `${dealer.nombre} ${dealer.apellido} (${dealer.email})`,
      input: "select",
      inputOptions: {
        Gratis: "Gratis",
        Basico: "Básico",
        Pro: "Pro",
        Elite: "Elite",
      },
      inputPlaceholder: "Selecciona el nuevo nivel",
      showCancelButton: true,
      confirmButtonText: "Cambiar",
      cancelButtonText: "Cancelar",
      inputValidator: (value) =>
        value ? undefined : "Debes seleccionar un nivel.",
    });

    if (!resultado.isConfirmed || !resultado.value) return;

    setProcesando(dealer.usuarioId);
    try {
      const respuesta = await adminService.cambiarPlan(
        dealer.usuarioId,
        resultado.value as string,
      );

      await Swal.fire({
        icon: "success",
        title: "Listo",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: error.message || "No se pudo cambiar el plan.",
        confirmButtonColor: "#7c3aed",
      });
    } finally {
      setProcesando(null);
    }
  };

  const renovarSuscripcion = async (dealer: UsuarioAdmin) => {
    if (procesando !== null) return;

    const hoy = new Date().toISOString().split("T")[0];
    const resultado = await Swal.fire({
      icon: "question",
      title: "Renovar suscripción",
      text: `${dealer.nombre} ${dealer.apellido} (${dealer.email})`,
      input: "date",
      inputValue: hoy,
      showCancelButton: true,
      confirmButtonText: "Renovar",
      cancelButtonText: "Cancelar",
      inputValidator: (value) =>
        value ? undefined : "Debes seleccionar la nueva fecha de vencimiento.",
    });

    if (!resultado.isConfirmed || !resultado.value) return;

    setProcesando(dealer.usuarioId);
    try {
      const respuesta = await adminService.renovarSuscripcion(
        dealer.usuarioId,
        `${resultado.value}T00:00:00Z`,
      );

      await Swal.fire({
        icon: "success",
        title: "Listo",
        text: respuesta.mensaje,
        confirmButtonColor: "#7c3aed",
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: error.message || "No se pudo renovar la suscripción.",
        confirmButtonColor: "#7c3aed",
      });
    } finally {
      setProcesando(null);
    }
  };

  if (loading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900">
          Gestión de usuarios
        </h2>
      </div>

      <div className="overflow-x-auto rounded-lg border border-gray-200 bg-white shadow-sm">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">Usuario</th>
              <th className="px-4 py-3">Correo</th>
              <th className="px-4 py-3">Rol</th>
              <th className="px-4 py-3">Registro</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {usuarios.map((usuario) => (
              <tr key={usuario.usuarioId} className="hover:bg-gray-50">
                <td className="px-4 py-3 font-medium text-gray-900">
                  {usuario.nombre} {usuario.apellido}
                </td>
                <td className="px-4 py-3 text-gray-600">{usuario.email}</td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                      COLOR_ROL[usuario.rol] ?? "bg-gray-100 text-gray-700"
                    }`}
                  >
                    {usuario.rol}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-600">
                  {formatearFecha(usuario.fechaRegistro)}
                </td>
                <td className="px-4 py-3">
                  {usuario.isActivo ? (
                    <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-semibold text-green-700">
                      <FaCheckCircle /> Activo
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-700">
                      <FaBan /> Suspendido
                    </span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center justify-end gap-2">
                    {usuario.rol === "Dealer" && (
                      <>
                        <button
                          type="button"
                          onClick={() => cambiarPlan(usuario)}
                          disabled={procesando !== null}
                          className="inline-flex items-center gap-1.5 rounded-lg border border-violet-200 bg-white px-3 py-1.5 text-xs font-semibold text-violet-700 transition-colors hover:bg-violet-50 disabled:opacity-50"
                        >
                          <FaCoins />
                          Plan
                        </button>
                        <button
                          type="button"
                          onClick={() => renovarSuscripcion(usuario)}
                          disabled={procesando !== null}
                          className="inline-flex items-center gap-1.5 rounded-lg border border-blue-200 bg-white px-3 py-1.5 text-xs font-semibold text-blue-700 transition-colors hover:bg-blue-50 disabled:opacity-50"
                        >
                          <FaRedoAlt />
                          Renovar
                        </button>
                      </>
                    )}

                    {usuario.rol !== "Admin" && (
                      <button
                        type="button"
                        onClick={() => toggleEstado(usuario)}
                        disabled={procesando !== null}
                        className={`inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-xs font-semibold transition-colors disabled:opacity-50 ${
                          usuario.isActivo
                            ? "border-red-200 bg-white text-red-700 hover:bg-red-50"
                            : "border-green-200 bg-white text-green-700 hover:bg-green-50"
                        }`}
                      >
                        {usuario.isActivo ? <FaBan /> : <FaCheckCircle />}
                        {usuario.isActivo ? "Suspender" : "Reactivar"}
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}