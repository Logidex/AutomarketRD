import { FaUserCog } from "react-icons/fa";
import SeccionCambiarCorreo from "../../components/SeccionCambiarCorreo";
import SeccionCambiarPassword from "../../components/SeccionCambiarPassword";

export default function CuentaVendedor() {
  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="flex items-center gap-3 text-lg font-semibold text-gray-900">
          <FaUserCog className="text-blue-600" />
          Mi cuenta
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          Cambia el correo y la contraseña con los que accedes a AutoMarket RD.
        </p>
      </div>

      <SeccionCambiarCorreo />

      <SeccionCambiarPassword />
    </div>
  );
}