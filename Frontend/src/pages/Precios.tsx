import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import { type PlanCatalogo } from "../services/planes.service";
import { authService } from "../services/auth.service";
import { ROLES } from "../constants/roles";
import { PAGOS_HABILITADOS } from "../constants/config";
import { formatearRD$, precioCicloDe, type Ciclo } from "../utils/formato";
import { FaPaypal, FaBolt } from "react-icons/fa";
import HeaderPublico from "../components/layout/HeaderPublico";
import SectionBackground from "../components/SectionBackground";
import ShinyText from "../components/ShinyText";
import PlanCard from "../components/PlanCard";
import SeoHead from "../components/SeoHead";
import {
  usePlanesCatalogo,
} from "../hooks/useSuscripcion";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

export default function Precios() {
  const { data: planes = [] } = usePlanesCatalogo();
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const [comprandoPlan, setComprandoPlan] = useState<string | null>(null);
  const navigate = useNavigate();

  const precioCiclo = (plan: PlanCatalogo) =>
    precioCicloDe(plan, ciclo);

  const precioEtiqueta = (plan: PlanCatalogo) =>
    formatearRD$(precioCiclo(plan));

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

    if (!PAGOS_HABILITADOS) {
      await Swal.fire({
        icon: "warning",
        title: "Sistema de pagos en mantenimiento",
        html:
          "El sistema de pagos se encuentra temporalmente no disponible." +
          "<br/><br/>" +
          "Si deseas adquirir un plan, puedes solicitarlo contactando a nuestro equipo de soporte.",
        confirmButtonColor: "#3b82f6",
        confirmButtonText: "Ir a Contacto",
      });

      navigate("/contacto?asunto=Pagos+y+suscripciones");
      return;
    }

    setComprandoPlan(plan.nivel);
    navigate(`/checkout?plan=${encodeURIComponent(plan.nivel)}&ciclo=${encodeURIComponent(ciclo)}`);
  };

  const planesPago = planes.filter(
    (plan) => plan.nivel !== "Gratis",
  );

  const planGratis = planes.find(
    (plan) => plan.nivel === "Gratis",
  );

  return (
    <div className="min-h-screen bg-page text-ink">
      <SeoHead
        titulo="Planes y precios"
        descripcion="Elige el plan perfecto para vender tu vehículo en AutoMarket RD. Planes desde gratis hasta Elite."
        tipo="website"
      />
      <HeaderPublico />

      <SectionBackground
        variant="cta"
        className="mx-auto max-w-5xl px-8 py-16"
      >
        <main>
          <div className="mb-12 text-center">
            <h1 className="mb-3 text-4xl font-bold leading-tight">
              <ShinyText>Planes para tu agencia</ShinyText>
            </h1>

            <p className="mx-auto max-w-2xl text-ink-2">
              Mientras mejor es tu plan,{" "}
              <strong className="text-ink">
                más rápido vendes
              </strong>
              : tus vehículos aparecen primero en la vitrina y
              destacan en la portada.
            </p>
          </div>

          <div className="mb-10 flex justify-center">
            <div className="inline-flex rounded-lg border border-line bg-surface-2 p-1">
              {CICLOS.map((cicloOpcion) => (
                <button
                  key={cicloOpcion}
                  type="button"
                  onClick={() => setCiclo(cicloOpcion)}
                  className={`rounded-lg px-6 py-2 text-sm font-semibold transition-colors ${
                    ciclo === cicloOpcion
                      ? "bg-blue-500 text-white"
                      : "text-ink-2 hover:text-ink"
                  }`}
                >
                  {cicloOpcion}
                </button>
              ))}
            </div>
          </div>

          <div className="grid grid-cols-1 items-stretch gap-6 md:grid-cols-2 lg:grid-cols-4">
            {planGratis && (
              <div>
                <PlanCard
                  plan={planGratis}
                  precio="Gratis"
                  ciclo={ciclo}
                  etiqueta="Para probar"
                  boton={
                    <Link
                      to="/registro"
                      className="block w-full rounded-lg border border-line py-2.5 text-center text-sm font-semibold transition-colors hover:border-blue-500"
                    >
                      Registrarme
                    </Link>
                  }
                />
              </div>
            )}

            {planesPago.length > 0 ? (
              planesPago.map((plan) => {
                const esPopular = plan.nivel === "Pro";
                const esPremium = plan.nivel === "Elite";

                return (
                  <div key={plan.nivel}>
                    <PlanCard
                      plan={plan}
                      precio={precioEtiqueta(plan)}
                      ciclo={ciclo}
                      etiqueta={
                        esPopular
                          ? "Más popular"
                          : esPremium
                            ? "Máximo rendimiento"
                            : undefined
                      }
                      destacado={esPopular}
                      premium={esPremium}
                      boton={
                        <button
                          type="button"
                          onClick={() => handleComprar(plan)}
                          disabled={comprandoPlan === plan.nivel}
                          className="w-full rounded-lg bg-blue-500 py-2.5 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:bg-blue-300"
                        >
                          {comprandoPlan === plan.nivel
                            ? "Redirigiendo..."
                            : "Comprar Plan"}
                        </button>
                      }
                    />
                  </div>
                );
              })
            ) : (
              <p className="col-span-3 text-center text-ink-2">
                Los planes están disponibles próximamente.
              </p>
            )}
          </div>

          <section className="mt-16 rounded-2xl border border-line bg-surface-2 p-8">
            <div className="mb-6 flex items-center gap-3">
              <FaBolt className="text-2xl text-amber-400" />

              <h2 className="text-xl font-bold">
                ¿Por qué un mejor plan vende más rápido?
              </h2>
            </div>

            <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
              <div>
                <h3 className="mb-1 font-semibold text-blue-400">
                  Prioridad en la vitrina
                </h3>

                <p className="text-sm text-ink-2">
                  La vitrina y las búsquedas ordenan los vehículos
                  por plan: los anuncios de Elite, Pro y Básico
                  aparecen antes que los del plan Gratis.
                </p>
              </div>

              <div>
                <h3 className="mb-1 font-semibold text-violet-400">
                  Destacados en la portada
                </h3>

                <p className="text-sm text-ink-2">
                  Pro y Elite muestran sus vehículos en la sección
                  &quot;Destacados Premium&quot; de la página principal,
                  la zona de mayor visibilidad.
                </p>
              </div>

              <div>
                <h3 className="mb-1 font-semibold text-amber-400">
                  Confianza y reconocimiento
                </h3>

                <p className="text-sm text-ink-2">
                  Tu vehículo lleva el badge de tu plan, lo que
                  transmite seriedad y atrae más contactos de
                  compradores.
                </p>
              </div>
            </div>
          </section>

          <div className="mt-10 flex items-center justify-center gap-3 rounded-xl border border-[#3b2f2f] bg-[#1a1515] p-4">
            <FaPaypal className="text-3xl text-[#0070ba]" />

            <p className="text-sm text-ink-2">
              Aceptamos{" "}
              <strong className="text-white">PayPal</strong> y{" "}
              <strong className="text-white">transferencia bancaria</strong>.
            </p>
          </div>
        </main>
      </SectionBackground>
    </div>
  );
}
