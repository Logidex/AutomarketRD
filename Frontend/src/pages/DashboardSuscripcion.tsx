import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Swal from "sweetalert2";
import { FaCreditCard, FaTicketAlt } from "react-icons/fa";
import { type PlanCatalogo } from "../services/planes.service";
import type { SuscripcionDealer, PagoSuscripcion } from "../services/suscripcion.service";
import type { CuponAplicado } from "../services/cupones.service";
import { dashboardService } from "../services/dashboard.service";
import Spinner from "../components/Spinner";
import { formatearRD$, precioCicloDe } from "../utils/formato";
import { nombrePlan } from "../constants/planes";
import { formatearFecha } from "../utils/fecha";
import {
  useSuscripcion,
  usePlanesCatalogo,
  useHistorialPagos,
  useCancelarSuscripcion,
  useGenerarLinkPago,
  useAplicarCupon,
} from "../hooks/useSuscripcion";

type Ciclo = "Mensual" | "Trimestral" | "Anual";

const CICLOS: Ciclo[] = ["Mensual", "Trimestral", "Anual"];

function useSuscripcionPage() {
  const [ciclo, setCiclo] = useState<Ciclo>("Mensual");
  const [procesando, setProcesando] = useState<string | null>(null);

  const suscQuery = useSuscripcion();
  const planesQuery = usePlanesCatalogo();
  const pagosQuery = useHistorialPagos();
  const resumenQuery = useQuery({
    queryKey: ['dashboard-resumen'],
    queryFn: () => dashboardService.obtenerResumen(),
    staleTime: 1000 * 60 * 2,
    retry: false,
  });

  const cancelar = useCancelarSuscripcion();
  const generarLinkPago = useGenerarLinkPago();
  const aplicarCupon = useAplicarCupon();

  const suscripcion = suscQuery.data ?? null;
  const planes = planesQuery.data ?? [];
  const pagos = pagosQuery.data ?? [];
  const anunciosActivos = resumenQuery.data?.anunciosActivos ?? null;

  const cargando =
    suscQuery.isLoading ||
    planesQuery.isLoading ||
    pagosQuery.isLoading ||
    resumenQuery.isLoading;

  const precioCiclo = (plan: PlanCatalogo) => precioCicloDe(plan, ciclo);

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
    if (procesando !== null) return;

    if (precioCiclo(plan) <= 0) {
      await Swal.fire({
        icon: "info",
        title: "Plan Gratis",
        text: "El plan Gratis se asigna al registrarte, no requiere pago.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    // Renovación anticipada: avisar que el nuevo período se suma al vencimiento actual
    if (
      estadoEscogido(plan) === "renovar" &&
      suscripcion !== null &&
      suscripcion.estado === "Activa" &&
      suscripcion.activa
    ) {
      const confirmacion = await Swal.fire({
        icon: "info",
        title: "Renovar tu suscripción",
        html: `Tu suscripción está activa hasta el <strong>${formatearFecha(
          suscripcion.fechaVencimientoUtc,
        )}</strong>. Si renuevas ahora, el próximo período (${ciclo.toLowerCase()}) se sumará al vencimiento actual y <strong>no perderás los días restantes</strong>.`,
        showCancelButton: true,
        confirmButtonColor: "#3b82f6",
        confirmButtonText: "Sí, ir al pago",
        cancelButtonText: "Cancelar",
      });

      if (!confirmacion.isConfirmed) return;
    }

    // Cambio de plan con suscripción activa: avisar que se pierden los días restantes
    if (
      estadoEscogido(plan) === "cambiar" &&
      suscripcion !== null &&
      suscripcion.estado === "Activa" &&
      suscripcion.activa
    ) {
      const confirmacion = await Swal.fire({
        icon: "warning",
        title: "Cambiar de plan",
        html: `Tu suscripción actual vence el <strong>${formatearFecha(
          suscripcion.fechaVencimientoUtc,
        )}</strong>. Al cambiar el plan o el ciclo, la nueva vigencia empieza desde hoy y <strong>los días restantes del plan actual no se conservan</strong>.`,
        showCancelButton: true,
        confirmButtonColor: "#3b82f6",
        confirmButtonText: "Sí, cambiar ahora",
        cancelButtonText: "Cancelar",
      });

      if (!confirmacion.isConfirmed) return;
    }

    setProcesando(plan.nivel);

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
      await cancelar.mutateAsync();
      await Swal.fire({
        icon: "success",
        title: "Suscripción cancelada",
        confirmButtonColor: "#3b82f6",
      });
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

  const esActiva =
    suscripcion !== null && suscripcion.estado === "Activa" && suscripcion.activa;

  const esCanceladaConVigencia =
    suscripcion !== null &&
    suscripcion.estado === "Cancelada" &&
    suscripcion.activa;

  // Canje de cupón promocional desde el dashboard (para quien no lo usó al iniciar)
  const aplicarCuponDealer = async (codigo: string): Promise<CuponAplicado | null> => {
    if (!codigo.trim() || procesando !== null) return null;

    setProcesando("cupon");
    try {
      return await aplicarCupon.mutateAsync(codigo.trim());
    } finally {
      setProcesando(null);
    }
  };

  return {
    cargando,
    suscripcion,
    anunciosActivos,
    ciclo,
    procesando,
    pagos,
    planesPago,
    esActiva,
    esCanceladaConVigencia,
    etiquetaBoton,
    setCiclo,
    handlePago,
    handleCancelar,
    aplicarCuponDealer,
  };
}

interface PropsEstado {
  suscripcion: SuscripcionDealer | null;
  anunciosActivos: number | null;
  esCanceladaConVigencia: boolean;
  onCancelar: () => void;
}

function TarjetaEstadoSuscripcion({
  suscripcion,
  anunciosActivos,
  esCanceladaConVigencia,
  onCancelar,
}: PropsEstado) {
  return (
    <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-blue-100 text-blue-600">
            <FaCreditCard className="text-xl" />
          </div>
          <div>
            <p className="text-sm text-ink-3">Plan actual</p>
            <p className="text-xl font-bold text-ink">
              {suscripcion ? `${nombrePlan(suscripcion.nivel)} · ${suscripcion.ciclo}` : "Sin suscripción"}
            </p>
            {suscripcion && (
              <p className="text-sm text-ink-3">
                {anunciosActivos !== null
                  ? `${anunciosActivos} de ${suscripcion.limiteAnuncios} anuncios en uso`
                  : `${suscripcion.limiteAnuncios} anuncios en tu plan`}{" "}
                ·{" "}
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
            onClick={onCancelar}
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
                {formatearFecha(suscripcion.fechaVencimientoUtc)}).
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
  );
}

interface PropsCiclo {
  ciclo: Ciclo;
  procesando: string | null;
  onChange: (ciclo: Ciclo) => void;
}

function SelectorCiclo({ ciclo, procesando, onChange }: PropsCiclo) {
  return (
    <div className="flex items-center gap-2">
      <span className="text-sm font-semibold text-ink-2">Ciclo de facturación:</span>
      <div className="inline-flex rounded-lg border border-line bg-surface p-1">
        {CICLOS.map((c) => (
          <button
            key={c}
            type="button"
            onClick={() => onChange(c)}
            disabled={procesando !== null}
            className={`rounded-lg px-5 py-2 text-sm font-semibold transition-colors ${
              ciclo === c
                ? "bg-blue-500 text-white"
                : "text-ink-3 hover:text-ink"
            } disabled:cursor-not-allowed`}
          >
            {c === "Mensual" ? "Mensual" : c === "Trimestral" ? "Trimestral" : "Anual"}
          </button>
        ))}
      </div>
    </div>
  );
}

interface PropsGrilla {
  planes: PlanCatalogo[];
  ciclo: Ciclo;
  procesando: string | null;
  etiquetaBoton: (plan: PlanCatalogo) => string;
  onPagar: (plan: PlanCatalogo) => void;
}

function GrillaPlanes({ planes, ciclo, procesando, etiquetaBoton, onPagar }: PropsGrilla) {
  const precioCiclo = (plan: PlanCatalogo) => precioCicloDe(plan, ciclo);

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
      {planes.map((plan) => (
        <div
          key={plan.nivel}
          className="flex flex-col rounded-xl border border-line bg-surface p-6 shadow-sm transition-colors hover:border-blue-300"
        >
          <h3 className="text-lg font-semibold text-ink">{plan.nombre}</h3>
          <p className="mt-1 mb-4 text-sm text-ink-3">{plan.descripcion}</p>
          <div className="text-3xl font-bold text-ink mb-1">
            {formatearRD$(precioCiclo(plan))}
          </div>
          <p className="text-xs text-ink-3 mb-6">
            {plan.limiteAnuncios} anuncios
            {ciclo === "Mensual" && plan.descuentoAnualPorcentaje > 0 && (
              <> · hasta {plan.descuentoAnualPorcentaje}% en Anual</>
            )}
          </p>
          <div className="mt-auto">
            <button
              type="button"
              onClick={() => onPagar(plan)}
              disabled={procesando !== null}
              className="w-full rounded-lg bg-blue-500 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:bg-blue-300 disabled:hover:bg-blue-300"
            >
              {procesando === plan.nivel ? "Redirigiendo..." : etiquetaBoton(plan)}
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}

function HistorialPagos({ pagos }: { pagos: PagoSuscripcion[] }) {
  return (
    <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
      <h2 className="mb-4 text-lg font-semibold text-ink">Historial de pagos</h2>
      {pagos.length === 0 ? (
        <p className="text-sm text-ink-3">
          Aún no tienes pagos registrados. Cuando realices tu primera compra o renovación, aparecerá aquí.
        </p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-line text-xs uppercase text-ink-3">
                <th className="py-2 pr-4">Fecha</th>
                <th className="py-2 pr-4">Plan</th>
                <th className="py-2 pr-4">Ciclo</th>
                <th className="py-2 pr-4">Total</th>
                <th className="py-2 pr-4">Estado</th>
                <th className="py-2">Orden PayPal</th>
              </tr>
            </thead>
            <tbody>
              {pagos.map((pago) => (
                <tr key={pago.id} className="border-b border-line last:border-0">
                  <td className="py-3 pr-4 text-ink-2">
                    {formatearFecha(pago.fechaUtc)}
                  </td>
                  <td className="py-3 pr-4 font-semibold text-ink">{nombrePlan(pago.nivel)}</td>
                  <td className="py-3 pr-4 capitalize text-ink-2">{pago.ciclo}</td>
                  <td className="py-3 pr-4 text-ink-2">
                    {pago.moneda} ${pago.monto.toFixed(2)}
                  </td>
                  <td className="py-3 pr-4">
                    <span className={estadoPagoClass(pago.estado)}>{estadoPagoLabel(pago.estado)}</span>
                  </td>
                  <td className="py-3 text-ink-2">{pago.ordenIdPayPal ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <p className="mt-4 text-xs text-ink-3">
        Te avisamos por correo cuando tu plan esté por vencer, para que lo renueves a tiempo.
      </p>
    </div>
  );
}

interface PropsCupon {
  onAplicar: (codigo: string) => Promise<CuponAplicado | null>;
  procesando: string | null;
}

function TarjetaCupon({ onAplicar, procesando }: PropsCupon) {
  const [codigo, setCodigo] = useState("");

  const aplicar = async () => {
    if (!codigo.trim() || procesando !== null) return;

    try {
      const resultado = await onAplicar(codigo);

      if (resultado) {
        setCodigo("");
        await Swal.fire({
          icon: "success",
          title: "¡Cupón aplicado!",
          html: `${resultado.mensaje}<br/><strong>Plan ${nombrePlan(
            resultado.nivel,
          )}</strong> · ${resultado.dias} días · vence ${formatearFecha(
            resultado.fechaVencimientoUtc,
          )}`,
          confirmButtonColor: "#3b82f6",
        });
      }
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
    <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-amber-100 text-amber-600">
            <FaTicketAlt />
          </div>
          <div>
            <p className="text-sm font-semibold text-ink">¿Tienes un cupón?</p>
            <p className="text-xs text-ink-3">
              Canjea un código promocional para obtener días de plan.
            </p>
          </div>
        </div>

        <div className="flex w-full gap-2 sm:w-auto">
          <input
            value={codigo}
            onChange={(e) => setCodigo(e.target.value.toUpperCase())}
            onKeyDown={(e) => e.key === "Enter" && aplicar()}
            placeholder="CÓDIGO"
            disabled={procesando !== null}
            maxLength={30}
            className="w-full rounded-lg border border-line bg-page px-4 py-2.5 text-sm font-semibold uppercase tracking-wide text-ink outline-none transition focus:border-brand focus:ring-4 focus:ring-brand/10 disabled:opacity-50 sm:w-44"
          />
          <button
            type="button"
            onClick={aplicar}
            disabled={procesando !== null || !codigo.trim()}
            className="shrink-0 rounded-lg bg-amber-400 px-5 py-2.5 text-sm font-bold text-amber-950 transition-colors hover:bg-amber-300 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {procesando === "cupon" ? "Aplicando..." : "Aplicar"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default function DashboardSuscripcion() {
  const {
    cargando,
    suscripcion,
    anunciosActivos,
    ciclo,
    procesando,
    pagos,
    planesPago,
    esActiva,
    esCanceladaConVigencia,
    etiquetaBoton,
    setCiclo,
    handlePago,
    handleCancelar,
    aplicarCuponDealer,
  } = useSuscripcionPage();

  if (cargando) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6 p-6">
      {/* Estado actual */}
      <TarjetaEstadoSuscripcion
        suscripcion={suscripcion}
        anunciosActivos={anunciosActivos}
        esCanceladaConVigencia={esCanceladaConVigencia}
        onCancelar={handleCancelar}
      />

      {/* Cupón promocional */}
      <TarjetaCupon onAplicar={aplicarCuponDealer} procesando={procesando} />

      {!esActiva && !(suscripcion && suscripcion.estado === "Cancelada") && (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
          Tu suscripción está vencida. Renuevala para seguir publicando tus
          anuncios sin interrupciones.
        </div>
      )}

      {/* Selector de ciclo */}
      <SelectorCiclo ciclo={ciclo} procesando={procesando} onChange={setCiclo} />

      {/* Planes de pago */}
      {planesPago.length > 0 ? (
        <GrillaPlanes
          planes={planesPago}
          ciclo={ciclo}
          procesando={procesando}
          etiquetaBoton={etiquetaBoton}
          onPagar={handlePago}
        />
      ) : (
        <p className="text-center text-ink-3">Los planes están disponibles próximamente.</p>
      )}

      {/* Historial de pagos */}
      <HistorialPagos pagos={pagos} />
    </div>
  );
}

function estadoPagoLabel(estado: string): string {
  const mapa: Record<string, string> = {
    Completado: "Completado",
    Fallido: "Fallido",
    Reembolsado: "Reembolsado",
  };
  return mapa[estado] ?? estado;
}

function estadoPagoClass(estado: string): string {
  if (estado === "Completado") return "rounded-full bg-green-100 px-2 py-0.5 text-xs font-semibold text-green-700";
  if (estado === "Fallido") return "rounded-full bg-red-100 px-2 py-0.5 text-xs font-semibold text-red-700";
  return "rounded-full bg-surface-2 px-2 py-0.5 text-xs font-semibold text-ink-2";
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