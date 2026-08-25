import { useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import Swal from "sweetalert2";
import { FaPaypal, FaSpinner, FaStore } from "react-icons/fa";
import { authService } from "../services/auth.service";
import { usePlanesCatalogo, useGenerarLinkPago } from "../hooks/useSuscripcion";
import { ROLES } from "../constants/roles";
import { formatearRD$, precioCicloDe, type Ciclo } from "../utils/formato";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";

export default function Checkout() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const planNivel = searchParams.get("plan") ?? "";
  const cicloParam = searchParams.get("ciclo");
  const ciclo: Ciclo =
    cicloParam === "Trimestral" || cicloParam === "Anual" ? cicloParam : "Mensual";

  const { data: planes = [] } = usePlanesCatalogo();
  const generarLinkPago = useGenerarLinkPago();
  const [procesando, setProcesando] = useState(false);

  const plan = planes.find((p) => p.nivel === planNivel) ?? null;
  const precio = plan ? precioCicloDe(plan, ciclo) : 0;

  const handlePagar = async () => {
    if (generarLinkPago.isPending) return;

    const usuario = authService.getCurrentUser();

    if (!usuario || usuario.rol !== ROLES.DEALER) {
      await Swal.fire({
        icon: "info",
        title: "Inicia sesión como Dealer",
        text: "Para completar el pago debes iniciar sesión con una cuenta de Dealer.",
        confirmButtonColor: "#3b82f6",
      });
      navigate("/login");
      return;
    }

    if (!plan || precio <= 0) return;

    setProcesando(true);

    try {
      const { url } = await generarLinkPago.mutateAsync({ plan: plan.nivel, ciclo });
      window.location.assign(url);
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error al iniciar el pago",
        text: err instanceof Error ? err.message : "Inténtalo nuevamente.",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setProcesando(false);
    }
  };

  return (
    <div className="min-h-screen bg-page text-ink">
      {/* Header */}
      <header className="sticky top-0 z-40 flex items-center justify-between border-b border-line bg-page/80 px-4 py-2 backdrop-blur sm:px-8">
        <div className="flex items-center gap-4">
          <Link to="/" className="flex items-center">
            <img
              src={logo}
              alt="AutoMarket RD"
              className="h-12 w-auto object-contain sm:h-16"
            />
          </Link>
          <span className="text-xl font-bold">Finalizar compra</span>
        </div>

        <MenuPublico />
      </header>

      <main className="mx-auto max-w-xl px-8 py-16">
        {!plan ? (
          <div className="rounded-2xl border border-line bg-surface-2 p-10 text-center">
            <p className="text-ink-2 mb-6">
              No encontramos el plan seleccionado. Elige uno desde la página de Precios.
            </p>
            <Link
              to="/precios"
              className="inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-3 font-semibold hover:bg-blue-600 transition-colors"
            >
              <FaStore />
              Ver Precios
            </Link>
          </div>
        ) : (
          <div className="rounded-2xl border border-line bg-surface-2 overflow-hidden">
            {/* Resumen del plan */}
            <div className="p-8">
              <h1 className="text-2xl font-bold mb-1">{plan.nombre}</h1>
              <p className="text-ink-2 mb-6">{plan.descripcion}</p>

              <div className="space-y-3 mb-6">
                <div className="flex items-center justify-between border-b border-line pb-3">
                  <span className="text-ink-2">Plan</span>
                  <span className="font-semibold">{plan.nombre}</span>
                </div>
                <div className="flex items-center justify-between border-b border-line pb-3">
                  <span className="text-ink-2">Ciclo de facturación</span>
                  <span className="font-semibold">{ciclo}</span>
                </div>
                <div className="flex items-center justify-between border-b border-line pb-3">
                  <span className="text-ink-2">Anuncios incluidos</span>
                  <span className="font-semibold">{plan.limiteAnuncios}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-ink-2">Total a pagar</span>
                  <span className="text-3xl font-bold text-green-400">
                    {formatearRD$(precio)}
                  </span>
                </div>
              </div>

              {/* Método de pago: solo PayPal */}
              <div className="rounded-xl border border-line bg-surface-2 p-4 mb-6 dark:border-[#3b2f2f] dark:bg-[#1a1515]">
                <p className="text-sm text-ink-2 mb-2">Método de pago</p>
                <div className="flex items-center gap-3">
                  <FaPaypal className="text-3xl text-[#0070ba]" />
                  <div>
                    <p className="font-semibold text-ink dark:text-white">PayPal</p>
                    <p className="text-xs text-ink-2 dark:text-gray-300">
                      Por ahora es el único método de pago disponible.
                    </p>
                  </div>
                </div>
              </div>

              <button
                type="button"
                onClick={handlePagar}
                disabled={procesando}
                className="w-full rounded-lg bg-[#0070ba] py-3.5 font-semibold hover:bg-[#005ea3] disabled:bg-[#3a6580] disabled:cursor-not-allowed flex items-center justify-center gap-2 transition-colors"
              >
                {procesando ? (
                  <>
                    <FaSpinner className="animate-spin" />
                    Redirigiendo a PayPal...
                  </>
                ) : (
                  <>
                    <FaPaypal className="text-xl" />
                    Pagar con PayPal
                  </>
                )}
              </button>

              <p className="text-xs text-ink-2 text-center mt-4">
                Al continuar serás redirigido a PayPal para completar el pago de forma
                segura. Al volver, tu suscripción se activará automáticamente.
              </p>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
