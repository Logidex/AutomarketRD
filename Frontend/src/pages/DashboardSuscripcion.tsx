import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { FaCreditCard } from "react-icons/fa";
import { planesService, type PlanCatalogo } from "../services/planes.service";
import { pagosService } from "../services/pagos.service";
import { suscripcionService, type SuscripcionDealer } from "../services/suscripcion.service";

type Ciclo = "Mensual" | "Trimestral" | "Anual";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

export default function DashboardSuscripcion() {
  const [suscripcion, setSuscripcion] = useState<SuscripcionDealer | null>(null);
  const [planes, setPlanes] = useState<PlanCatalogo[]>([]);
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const [cargando, setCargando] = useState(true);
  const [procesando, setProcesando] = useState<string | null>(null);

  const cargarSuscripcion = async () => {
    try {
      const data = await suscripcionService.obtenerSuscripcion();
      setSuscripcion(data);
    } catch {
      setSuscripcion(null);
    }
  };

  useEffect(() => {
    async function cargarDatos() {
      await cargarSuscripcion();

      try {
        setPlanes(await planesService.obtenerCatalogo());
      } catch {
        setPlanes([]);
      } finally {
        setCargando(false);
      }
    }

    cargarDatos();
  }, []);

  const precioCiclo = (plan: PlanCatalogo) => {
    if (ciclo === "Trimestral") return plan.precioTrimestral;
    if (ciclo === "Anual") return plan.precioAnual;
    return plan.precioMensual;
  };

  const estadoEscogido = (plan: PlanCatalogo) => {
    const esActual =
      suscripcion &&
      suscripcion.nivel === plan.nivel &&
      suscripcion.ciclo === ciclo;

    if (esActual) return "renovar";
    if (suscripcion?.estado === "Cancelada") return "reactivar";
    return "cambiar";
  };

  const etiquetaBoton = (plan: PlanCatalogo) => {
    const accion = precioCiclo(plan) <= 0 ? "tiene" : estadoEscogido(plan);
    if (accion === "renovar") return "Renovar";
    if (accion === "reactivar") return "Reactivar";
    if (accion === "tiene") return "Comprar";
    return "Cambiar Plan";
  };

  const handlePago = async (plan: PlanCatalogo) => {
    if (precioCiclo(plan) <= 0) {
      await Swal.fire({
        icon: "info",
        title: "Plan Gratis",
        text: "El plan Gratis se asigna al registrarte, no requiere pago.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    setProcesando(plan.nivel);

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
      setProcesando(null);
    }
  };

  const handleCancelar = async () => {
    const resultado = await Swal.fire({
      icon: "warning",
      title: "¿Cancelar tu suscripción?",
      text: "Perderás el acceso a los beneficios del plan hasta que la reactives.",
      showCancelButton: true,
      confirmButtonColor: "#dc2626",
      cancelButtonColor: "#9ca3af",
      confirmButtonText: "Sí, cancelar",
      cancelButtonText: "Volver",
    });

    if (!resultado.isConfirmed) return;

    try {
      await suscripcionService.cancelarSuscripcion();
      await Swal.fire({
        icon: "success",
        title: "Suscripción cancelada",
        confirmButtonColor: "#3b82f6",
      });
      await cargarSuscripcion();
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: err instanceof Error ? err.message : "No se pudo cancelar la suscripción.",
        confirmButtonColor: "#3b82f6",
      });
    }
  };

  const planesPago = planes.filter((p) => p.nivel !== "Gratis");

  if (cargando) {
    return (
      <div className="p-6">
        <div className="text-gray-500">Cargando tu suscripción...</div>
      </div>
    );
  }

  const esActiva =
    suscripcion !== null && suscripcion.estado === "Activa" && suscripcion.activa;

  const esCanceladaConVigencia =
    suscripcion !== null &&
    suscripcion.estado === "Cancelada" &&
    suscripcion.activa;

  return (
    <div className="space-y-6 p-6">
      {/* Estado actual */}
      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-full bg-blue-100 text-blue-600">
              <FaCreditCard className="text-xl" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Plan actual</p>
              <p className="text-xl font-bold text-gray-900">
                {suscripcion ? `${nombrePlan(suscripcion.nivel)} · ${suscripcion.ciclo}` : "Sin suscripción"}
              </p>
              {suscripcion && (
                <p className="text-sm text-gray-500">
                  {suscripcion.limiteAnuncios} anuncios ·{" "}
                  {suscripcion.estado === "Activa"
                    ? `${suscripcion.diasRestantes} días restantes`
                    : suscripcion.estado === "Cancelada"
                      ? esCanceladaConVigencia
                        ? `cancelada · beneficios vigentes hasta la fecha (${suscripcion.diasRestantes} días restantes)`
                        : "suscripción cancelada"
                      : "vencida"}
                </p>
              )}
            </div>
          </div>

          {suscripcion && suscripcion.estado !== "Cancelada" && (
            <button
              type="button"
              onClick={handleCancelar}
              className="rounded-lg border border-red-200 px-4 py-2 text-sm font-semibold text-red-600 transition-colors hover:bg-red-50"
            >
              Cancelar suscripción
            </button>
          )}
        </div>

        {suscripcion && suscripcion.estado === "Cancelada" && (
          <div className="mt-4 space-y-3">
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              {esCanceladaConVigencia ? (
                <>
                  Cancelaste tu suscripción, pero <strong>conservas el plan hasta su
                  vencimiento</strong> ({suscripcion.diasRestantes} días restantes,{" "}
                  {new Date(suscripcion.fechaVencimientoUtc).toLocaleDateString("es-DO")}).
                  Podrás seguir publicando hasta entonces.
                </>
              ) : (
                <>
                  Tu suscripción está cancelada. Elige un plan para{" "}
                  <strong>reactivarla</strong> y seguir publicando anuncios.
                </>
              )}
            </div>

            <SoporteCard />
          </div>
        )}
      </div>

      {!esActiva && !(suscripcion && suscripcion.estado === "Cancelada") && (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
          Tu suscripción está vencida. Renuevala para seguir publicando tus
          anuncios sin interrupciones.
        </div>
      )}

      {/* Selector de ciclo */}
      <div className="flex items-center gap-2">
        <span className="text-sm font-semibold text-gray-700">Ciclo de facturación:</span>
        <div className="inline-flex rounded-lg border border-gray-200 bg-white p-1">
          {CICLOS.map((c) => (
            <button
              key={c}
              type="button"
              onClick={() => setCiclo(c)}
              className={`rounded-lg px-5 py-2 text-sm font-semibold transition-colors ${
                ciclo === c
                  ? "bg-blue-500 text-white"
                  : "text-gray-500 hover:text-gray-800"
              }`}
            >
              {c === "Mensual" ? "Mensual" : c === "Trimestral" ? "Trimestral" : "Anual"}
            </button>
          ))}
        </div>
      </div>

      {/* Planes de pago */}
      {planesPago.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
          {planesPago.map((plan) => (
            <div
              key={plan.nivel}
              className="flex flex-col rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-colors hover:border-blue-300"
            >
              <h3 className="text-lg font-semibold text-gray-900">{plan.nombre}</h3>
              <p className="mt-1 mb-4 text-sm text-gray-500">{plan.descripcion}</p>
              <div className="text-3xl font-bold text-gray-900 mb-1">
                {precioCiclo(plan) === 0 ? "Gratis" : `RD$ ${precioCiclo(plan).toLocaleString("es-DO")}`}
              </div>
              <p className="text-xs text-gray-500 mb-6">
                {plan.limiteAnuncios} anuncios
                {ciclo === "Mensual" && plan.descuentoAnualPorcentaje > 0 && (
                  <> · hasta {plan.descuentoAnualPorcentaje}% en Anual</>
                )}
              </p>
              <div className="mt-auto">
                <button
                  type="button"
                  onClick={() => handlePago(plan)}
                  disabled={procesando === plan.nivel}
                  className="w-full rounded-lg bg-blue-500 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:bg-blue-300"
                >
                  {procesando === plan.nivel ? "Redirigiendo..." : etiquetaBoton(plan)}
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-center text-gray-500">Los planes están disponibles próximamente.</p>
      )}
    </div>
  );
}

function nombrePlan(nivel: string): string {
  const mapa: Record<string, string> = {
    Gratis: "Gratis",
    Basico: "Básico",
    Pro: "Pro",
    Elite: "Elite",
  };
  return mapa[nivel] ?? nivel;
}

function SoporteCard() {
  return (
    <div className="flex items-center gap-3 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
      <span className="text-lg">?</span>
      <div>
        <p className="font-semibold">¿Cancelaste por error?</p>
        <p>
          Escríbenos a{" "}
          <a
            href="mailto:noreply.automarketrd@gmail.com?subject=Cancelación por error - AutoMarket RD"
            className="font-semibold underline"
          >
            noreply.automarketrd@gmail.com
          </a>{" "}
          y te ayudaremos a recuperar tu suscripción.
        </p>
      </div>
    </div>
  );
}