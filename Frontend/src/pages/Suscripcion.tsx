import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import { FaPaypal, FaTicketAlt } from "react-icons/fa";
import { type PlanCatalogo } from "../services/planes.service";
import { authService } from "../services/auth.service";
import { ROLES } from "../constants/roles";
import { formatearRD$, precioCicloDe, type Ciclo } from "../utils/formato";
import HeaderPublico from "../components/layout/HeaderPublico";
import SectionBackground from "../components/SectionBackground";
import PlanCard from "../components/PlanCard";
import { usePlanesCatalogo, useAplicarCupon } from "../hooks/useSuscripcion";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

export default function Suscripcion() {
  const { data: planes = [] } = usePlanesCatalogo();
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const [codigoCupon, setCodigoCupon] = useState("");
  const navigate = useNavigate();
  const aplicarCupon = useAplicarCupon();

  const planesPago = planes.filter((p) => p.nivel !== "Gratis");
  const planGratis = planes.find((p) => p.nivel === "Gratis");

  const precioCiclo = (plan: PlanCatalogo) => precioCicloDe(plan, ciclo);

  const handleElegir = (plan: PlanCatalogo) => {
    const usuario = authService.getCurrentUser();

    if (!usuario || usuario.rol !== ROLES.DEALER) {
      Swal.fire({
        icon: "info",
        title: "Inicia sesión como Dealer",
        text: "Para elegir un plan debes iniciar sesión con una cuenta de Dealer.",
        confirmButtonColor: "#3b82f6",
      }).then(() => navigate("/login"));
      return;
    }

    if (precioCiclo(plan) <= 0) {
      Swal.fire({
        icon: "info",
        title: "Plan Gratis",
        text: "Ya tienes el plan Gratis asignado a tu cuenta.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    navigate(
      `/checkout?plan=${encodeURIComponent(plan.nivel)}&ciclo=${encodeURIComponent(ciclo)}`
    );
  };

  const handleAplicarCupon = async () => {
    if (!codigoCupon.trim() || aplicarCupon.isPending) return;

    try {
      const resultado = await aplicarCupon.mutateAsync(codigoCupon.trim());
      setCodigoCupon("");
      await Swal.fire({
        icon: "success",
        title: "¡Cupón aplicado!",
        text: resultado.mensaje,
        confirmButtonColor: "#3b82f6",
      });
      navigate("/dashboard");
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "No se pudo aplicar el cupón",
        text: err instanceof Error ? err.message : "Inténtalo nuevamente.",
        confirmButtonColor: "#3b82f6",
      });
    }
  };

  return (
    <div className="relative min-h-screen overflow-hidden bg-page text-ink">
      <HeaderPublico titulo="Suscripción" />

      <SectionBackground variant="cta" className="mx-auto max-w-5xl px-8 py-16">
      <main className="mx-auto max-w-5xl px-8 py-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold mb-3">
            Elige tu suscripción
          </h1>
          <p className="text-ink-2">
            Tu cuenta ya está activa con el plan Gratis. Elige el plan que mejor
            se adapte a tu agencia y empieza a vender más.
          </p>
        </div>

        {/* Cupón promocional: canje único por dealer */}
        <div className="mx-auto mb-10 max-w-xl rounded-xl border border-line bg-surface-2 p-5">
          <label
            htmlFor="codigoCupon"
            className="mb-3 flex items-center gap-2 text-sm font-semibold text-ink"
          >
            <FaTicketAlt className="text-green-500" />
            ¿Tienes un cupón?
          </label>
          <div className="flex flex-col gap-3 sm:flex-row">
            <input
              id="codigoCupon"
              type="text"
              value={codigoCupon}
              onChange={(e) => setCodigoCupon(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleAplicarCupon()}
              placeholder="Escribe tu código aquí"
              maxLength={50}
              className="flex-1 rounded-lg border border-line bg-input px-4 py-2.5 text-sm uppercase text-ink placeholder:normal-case placeholder:text-ink-3 focus:border-blue-500 focus:outline-none"
            />
            <button
              type="button"
              onClick={handleAplicarCupon}
              disabled={aplicarCupon.isPending || !codigoCupon.trim()}
              className="rounded-lg bg-green-600 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {aplicarCupon.isPending ? "Aplicando..." : "Aplicar cupón"}
            </button>
          </div>
        </div>

        {/* Selector de ciclo */}
        <div className="mb-10 flex justify-center">
          <div className="inline-flex rounded-lg border border-line bg-surface-2 p-1">
            {CICLOS.map((c) => (
              <button
                key={c}
                type="button"
                onClick={() => setCiclo(c)}
                className={`px-6 py-2 rounded-lg text-sm font-semibold transition-colors ${
                  ciclo === c
                    ? "bg-blue-500 text-white"
                    : "text-ink-2 hover:text-ink"
                }`}
              >
                {c}
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 items-stretch">
          {/* Tarjeta del plan Gratis */}
          {planGratis && (
            <PlanCard
              plan={planGratis}
              precio="Gratis"
              ciclo={ciclo}
              etiqueta="Tu plan actual"
              boton={
                <button
                  type="button"
                  onClick={() => navigate("/dashboard")}
                  className="block w-full rounded-lg border border-black/10 py-2.5 text-center text-sm font-semibold text-ink-2 hover:border-black/20 dark:border-white/20 dark:text-white dark:hover:border-white transition-colors"
                >
                  Ir a mi panel
                </button>
              }
            />
          )}

          {/* Planes de pago */}
          {planesPago.length > 0 ? (
            planesPago.map((plan) => {
              const esPopular = plan.nivel === "Pro";
              const esPremium = plan.nivel === "Elite";
              return (
                <PlanCard
                  key={plan.nivel}
                  plan={plan}
                  precio={formatearRD$(precioCiclo(plan))}
                  ciclo={ciclo}
                  etiqueta={esPopular ? "Más popular" : esPremium ? "Máximo rendimiento" : undefined}
                  destacado={esPopular}
                  premium={esPremium}
                  boton={
                    <button
                      type="button"
                      onClick={() => handleElegir(plan)}
                      className="w-full rounded-lg bg-blue-500 py-2.5 text-sm font-semibold hover:bg-blue-600 transition-colors"
                    >
                      Elegir este plan
                    </button>
                  }
                />
              );
            })
          ) : (
            <p className="col-span-3 text-center text-ink-2">
              Los planes están disponibles próximamente.
            </p>
          )}
        </div>

        {/* Método de pago */}
        <div className="mt-10 flex items-center justify-center gap-3 rounded-xl border border-line bg-surface-2 p-4 dark:border-[#3b2f2f] dark:bg-[#1a1515]">
          <FaPaypal className="text-3xl text-[#0070ba]" />
          <p className="text-sm text-ink-2 dark:text-gray-300">
            Aceptamos{" "}
            <strong className="text-ink dark:text-white">PayPal</strong> y{" "}
            <strong className="text-ink dark:text-white">transferencia bancaria</strong>.
          </p>
        </div>
      </main>
      </SectionBackground>
    </div>
  );
}
