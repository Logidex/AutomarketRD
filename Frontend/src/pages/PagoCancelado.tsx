import { Link } from "react-router-dom";
import { FaTimesCircle, FaStore } from "react-icons/fa";
import logo from "../assets/AutoMarketRD_Logo.svg";

export default function PagoCancelado() {
  return (
    <div className="min-h-screen bg-page flex items-center justify-center p-4 text-ink">
      <div className="w-full max-w-md bg-surface-2 rounded-2xl border border-line p-10 text-center">
        <div className="mb-6 flex justify-center">
          <img src={logo} alt="AutoMarket RD" className="h-14 w-auto object-contain" />
        </div>

        <FaTimesCircle className="mx-auto mb-4 text-5xl text-red-500" />

        <h1 className="text-2xl font-bold mb-2">Pago cancelado</h1>
        <p className="text-ink-2 mb-8">
          No se realizó ningún cargo. Si lo deseas, puedes volver a intentarlo
          desde los planes.
        </p>

        <Link
          to="/precios"
          className="flex items-center justify-center gap-2 w-full rounded-lg bg-blue-500 py-3 font-semibold hover:bg-blue-600 transition-colors"
        >
          <FaStore />
          Volver a Precios
        </Link>
      </div>
    </div>
  );
}