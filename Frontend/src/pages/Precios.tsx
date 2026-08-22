import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import { type PlanCatalogo } from "../services/planes.service";
import { authService } from "../services/auth.service";
import { ROLES } from "../constants/roles";
import { formatearRD$, precioCicloDe, type Ciclo } from "../utils/formato";
import { FaPaypal, FaBolt } from "react-icons/fa";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";
import PlanCard from "../components/PlanCard";
import { usePlanesCatalogo, useGenerarLinkPago } from "../hooks/useSuscripcion";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

export default function Precios() {
  const { data: planes = [] } = usePlanesCatalogo();
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const [comprandoPlan, setComprandoPlan] = useState<string | null>(null);
  const navigate = useNavigate();
  const generarLinkPago = useGenerarLinkPago();

  const precioCiclo = (plan: PlanCatalogo) => precioCicloDe(plan, ciclo);
  const precioEtiqueta = (plan: PlanCatalogo) => formatearRD$(precioCiclo(plan));

  const handleComprar = async (plan: PlanCatalogo) => {
    const usuario = authService.getCurrentUser();

    if (!usuario || usuario.rol !== ROLES.DEALER) {
      await Swal.fire({
        icon: "info",
        title: "Inicia sesión como Dealer",
        text: "Para comprar un plan debes iniciar sesión con una cuenta de Dealer.",
        confirmButtonColor: "#3b82f6",
      });
      navigate("/login");
      return;
    }

    if (precioCiclo(plan) <= 0) {
      await Swal.fire({
        icon: "info",
        title: "Plan Gratis",
        text: "El plan Gratis se asigna directamente al registrarte.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    setComprandoPlan(plan.nivel);

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
      setComprandoPlan(null);
    }
  };

  const planesPago = planes.filter((p) => p.nivel !== "Gratis");
  const planGratis = planes.find((p) => p.nivel === "Gratis");

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
          <span className="text-xl font-bold">Precios</span>
        </div>

        <MenuPublico />
      </header>

      <main className="mx-auto max-w-5xl px-8 py-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold mb-3">
            Planes para tu agencia
          </h1>
          <p className="mx-auto max-w-2xl text-ink-2">
            Mientras mejor es tu plan, <strong className="text-ink">más rápido vendes</strong>:
            tus vehículos aparecen primero en la vitrina y destacan en la portada.
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
              etiqueta="Para probar"
              boton={
                <Link
                  to="/registro"
                  className="block w-full rounded-lg border border-white/20 py-2.5 text-center text-sm font-semibold hover:border-white transition-colors"
                >
                  Registrarme
                </Link>
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
                  precio={precioEtiqueta(plan)}
                  ciclo={ciclo}
                  etiqueta={esPopular ? "Más popular" : esPremium ? "Máximo rendimiento" : undefined}
                  destacado={esPopular}
                  premium={esPremium}
                  boton={
                    <button
                      type="button"
                      onClick={() => handleComprar(plan)}
                      disabled={comprandoPlan === plan.nivel}
                      className="w-full rounded-lg bg-blue-500 py-2.5 text-sm font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors"
                    >
                      {comprandoPlan === plan.nivel ? "Redirigiendo..." : "Comprar Plan"}
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

        {/* Por qué un mejor plan vende más rápido */}
        <section className="mt-16 rounded-2xl border border-line bg-surface-2 p-8">
          <div className="flex items-center gap-3 mb-6">
            <FaBolt className="text-2xl text-amber-400" />
            <h2 className="text-xl font-bold">
              ¿Por qué un mejor plan vende más rápido?
            </h2>
          </div>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            <div>
              <h3 className="font-semibold mb-1 text-blue-400">
                Prioridad en la vitrina
              </h3>
              <p className="text-sm text-ink-2">
                La vitrina y las búsquedas ordenan los vehículos por plan:
                los anuncios de Elite, Pro y Básico aparecen antes que los del
                plan Gratis.
              </p>
            </div>
            <div>
              <h3 className="font-semibold mb-1 text-violet-400">
                Destacados en la portada
              </h3>
              <p className="text-sm text-ink-2">
                Pro y Elite muestran sus vehículos en la sección
                "Destacados Premium" de la página principal, la zona de mayor
                visibilidad.
              </p>
            </div>
            <div>
              <h3 className="font-semibold mb-1 text-amber-400">
                Confianza y reconocimiento
              </h3>
              <p className="text-sm text-ink-2">
                Tu vehículo lleva el badge de tu plan, lo que transmite seriedad
                y atrae más contactos de compradores.
              </p>
            </div>
          </div>
        </section>

        {/* Método de pago */}
        <div className="mt-10 flex items-center justify-center gap-3 rounded-xl border border-[#3b2f2f] bg-[#1a1515] p-4">
          <FaPaypal className="text-3xl text-[#0070ba]" />
          <p className="text-sm text-ink-2">
            El pago se procesa de forma segura con <strong className="text-white">PayPal</strong>. Por
            ahora es el único método de pago disponible.
          </p>
        </div>
      </main>
    </div>
  );
}
