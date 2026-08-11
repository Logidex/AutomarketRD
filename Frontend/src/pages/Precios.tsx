import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import { planesService, type PlanCatalogo } from "../services/planes.service";
import { pagosService } from "../services/pagos.service";
import { authService } from "../services/auth.service";
import { ROLES } from "../constants/roles";
import { formatearRD$, precioCicloDe } from "../utils/formato";
import logo from "../assets/AutoMarketRD_Logo.svg";
import MenuPublico from "../components/layout/MenuPublico";

type Ciclo = "Mensual" | "Trimestral" | "Anual";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

export default function Precios() {
  const [planes, setPlanes] = useState<PlanCatalogo[]>([]);
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const [comprandoPlan, setComprandoPlan] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    planesService
      .obtenerCatalogo()
      .then(setPlanes)
      .catch(() => setPlanes([]));
  }, []);

  const precioCiclo = (plan: PlanCatalogo) => precioCicloDe(plan, ciclo);

  const precioEtiqueta = (plan: PlanCatalogo) => formatearRD$(precioCiclo(plan));

  // Reglas visuales: solo el Gratis se resalta, los demás son opciones.
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
      const { url } = await pagosService.generarLinkPago(plan.nivel, ciclo);
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
    <div className="min-h-screen bg-[#0c101b] text-white">
      {/* Header */}
      <header className="flex items-center justify-between border-b border-white/10 px-8 py-5">
        <div className="flex items-center gap-4">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-12 w-auto object-contain"
          />
          <span className="text-xl font-bold">Precios</span>
        </div>

        <MenuPublico />
      </header>

      <main className="mx-auto max-w-5xl px-8 py-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold mb-3">
            Planes para tu agencia
          </h1>
          <p className="text-[#9aa1b1]">
            Publica, gestiona y vende más con los planes de AutoMarket RD.
          </p>
        </div>

        {/* Selector de ciclo */}
        <div className="mb-10 flex justify-center">
          <div className="inline-flex rounded-lg border border-white/10 bg-[#11141a] p-1">
            {CICLOS.map((c) => (
              <button
                key={c}
                type="button"
                onClick={() => setCiclo(c)}
                className={`px-6 py-2 rounded-lg text-sm font-semibold transition-colors ${
                  ciclo === c
                    ? "bg-blue-500 text-white"
                    : "text-[#9aa1b1] hover:text-white"
                }`}
              >
                {c}
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          {/* Tarjeta del plan Gratis */}
          <div className="rounded-2xl border border-white/10 bg-[#17141a] p-6 flex flex-col">
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-lg font-semibold">
                {planGratis?.nombre ?? "Plan Gratis"}
              </h3>
              <span className="rounded-full bg-green-500/10 px-2 py-0.5 text-xs font-bold text-green-400">
                Gratis
              </span>
            </div>
            <p className="text-sm text-[#9aa1b1] mb-4">
              {planGratis?.descripcion ?? "Para probar la plataforma."}
            </p>
            <div className="text-3xl font-bold mb-1">
              {planGratis ? precioEtiqueta(planGratis) : "Gratis"}
            </div>
            <p className="text-xs text-[#9aa1b1] mb-6">
              {planGratis
                ? `${planGratis.limiteAnuncios} ${planGratis.limiteAnuncios === 1 ? "anuncio" : "anuncios"}`
                : "1 anuncio"}
            </p>
            <div className="mt-auto">
              <Link
                to="/registro"
                className="block w-full rounded-lg border border-white/20 py-2.5 text-center text-sm font-semibold hover:border-white transition-colors"
              >
                Registrarme
              </Link>
            </div>
          </div>

          {/* Planes de pago */}
          {planesPago.length > 0 ? (
            planesPago.map((plan) => (
              <div
                key={plan.nivel}
                className="rounded-2xl border border-white/10 bg-[#13161d] p-6 flex flex-col transition-colors hover:border-blue-500/40"
              >
                <h3 className="text-lg font-semibold mb-2">{plan.nombre}</h3>
                <p className="text-sm text-[#9aa1b1] mb-4">
                  {plan.descripcion}
                </p>
                <div className="text-3xl font-bold mb-1">
                  {precioEtiqueta(plan)}
                </div>
                <p className="text-xs text-[#9aa1b1] mb-6">
                  {plan.limiteAnuncios} anuncios
                  {ciclo === "Mensual" && plan.descuentoTrimestralPorcentaje > 0 && (
                    <> · hasta {plan.descuentoAnualPorcentaje}% en Anual</>
                  )}
                </p>
                <div className="mt-auto">
                  <button
                    type="button"
                    onClick={() => handleComprar(plan)}
                    disabled={comprandoPlan === plan.nivel}
                    className="w-full rounded-lg bg-blue-500 py-2.5 text-sm font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors"
                  >
                    {comprandoPlan === plan.nivel ? "Redirigiendo..." : "Comprar Plan"}
                  </button>
                </div>
              </div>
            ))
          ) : (
            <p className="col-span-3 text-center text-[#9aa1b1]">
              Los planes están disponibles próximamente.
            </p>
          )}
        </div>
      </main>
    </div>
  );
}