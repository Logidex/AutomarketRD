import { Link } from "react-router-dom";
import { FaCheckCircle, FaTachometerAlt } from "react-icons/fa";
import logo from "../assets/AutoMarketRD_Logo.svg";

export default function PagoExitoso() {
  return (
    <div className="min-h-screen bg-[#0c101b] flex items-center justify-center p-4 text-white">
      <div className="w-full max-w-md bg-[#11141a] rounded-2xl border border-white/10 p-10 text-center">
        <div className="mb-6 flex justify-center">
          <img src={logo} alt="AutoMarket RD" className="h-14 w-auto object-contain" />
        </div>

        <FaCheckCircle className="mx-auto mb-4 text-5xl text-green-500" />

        <h1 className="text-2xl font-bold mb-2">¡Pago exitoso!</h1>
        <p className="text-[#9aa1b1] mb-8">
          Tu suscripción se está activando. Puedes verificar el estado en tu
          panel de control.
        </p>

        <Link
          to="/dashboard"
          className="flex items-center justify-center gap-2 w-full rounded-lg bg-blue-500 py-3 font-semibold hover:bg-blue-600 transition-colors"
        >
          <FaTachometerAlt />
          Ir a mi Panel
        </Link>
      </div>
    </div>
  );
}