import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import { FaPaypal } from "react-icons/fa";
import { type PlanCatalogo } from "../services/planes.service";
import { authService } from "../services/auth.service";
import { ROLES } from "../constants/roles";
import { formatearRD$, precioCicloDe, type Ciclo } from "../utils/formato";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";
import PlanCard from "../components/PlanCard";
import { usePlanesCatalogo } from "../hooks/useSuscripcion";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

export default function Suscripcion() {
  const { data: planes = [] } = usePlanesCatalogo();
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const navigate = useNavigate();

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

  return (
    <div className="min-h-screen bg-page text-ink">
      {/* Header */}
      <header className="relative flex items-center justify-between border-b border-line bg-page/80 px-4 py-2 backdrop-blur sm:px-8">
        <div className="flex items-center gap-4">
          <Link to="/" className="flex items-center">
            <img
              src={logo}
              alt="AutoMarket RD"
              className="h-12 w-auto object-contain sm:h-16"
            />
          </Link>
          <span className="text-xl font-bold">Suscripción</span>
        </div>

        <MenuPublico />
      </header>

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
            El pago se procesa de forma segura con{" "}
            <strong className="text-ink dark:text-white">PayPal</strong>. Por ahora es el único
            método de pago disponible.
          </p>
        </div>
      </main>
    </div>
  );
}
