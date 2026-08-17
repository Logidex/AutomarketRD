import { FaCheck, FaCrown } from "react-icons/fa";
import { type PlanCatalogo } from "../services/planes.service";
import { type Ciclo } from "../utils/formato";

// Prioridad de cada plan en la vitrina y búsquedas (1 = menor, 4 = máxima)
const PRIORIDAD_PLAN: Record<string, { etiqueta: string; nivel: number }> = {
  Gratis: { etiqueta: "Estándar", nivel: 1 },
  Basico: { etiqueta: "Media", nivel: 2 },
  Pro: { etiqueta: "Alta", nivel: 3 },
  Elite: { etiqueta: "Máxima", nivel: 4 },
};

const ESTILO_ACENTO: Record<string, string> = {
  Gratis: "text-ink-3 dark:text-gray-400",
  Basico: "text-blue-600 dark:text-blue-400",
  Pro: "text-violet-600 dark:text-violet-400",
  Elite: "text-amber-600 dark:text-amber-400",
};

const ESTILO_BARRA: Record<string, string> = {
  Gratis: "bg-ink-3 dark:bg-gray-400",
  Basico: "bg-blue-600 dark:bg-blue-400",
  Pro: "bg-violet-600 dark:bg-violet-400",
  Elite: "bg-amber-600 dark:bg-amber-400",
};

interface PropsPlanCard {
  plan: PlanCatalogo;
  precio: string;
  ciclo: Ciclo;
  etiqueta?: string;
  destacado?: boolean;
  premium?: boolean;
  boton: React.ReactNode;
}

export default function PlanCard({
  plan,
  precio,
  ciclo,
  etiqueta,
  destacado = false,
  premium = false,
  boton,
}: PropsPlanCard) {
  const prioridad = PRIORIDAD_PLAN[plan.nivel] ?? { etiqueta: "Estándar", nivel: 1 };
  const acento = ESTILO_ACENTO[plan.nivel] ?? "text-ink-3 dark:text-gray-400";
  const barra = ESTILO_BARRA[plan.nivel] ?? "bg-ink-3 dark:bg-gray-400";
  const esGratis = plan.nivel === "Gratis";

  return (
    <div
      className={`relative flex flex-col rounded-2xl border p-6 transition-colors ${
        destacado
          ? "border-violet-500/60 bg-violet-500/10 shadow-lg shadow-violet-500/10 dark:bg-[#181327]"
          : premium
            ? "border-amber-500/40 bg-amber-500/10 dark:bg-[#191410]"
            : "border-line bg-surface hover:border-blue-500/40"
      }`}
    >
      {destacado && (
        <span className="absolute -top-3 left-1/2 -translate-x-1/2 rounded-full bg-violet-500 px-4 py-1 text-xs font-bold text-white">
          MÁS POPULAR
        </span>
      )}
      {premium && !destacado && (
        <FaCrown className="absolute -top-3 right-4 rounded-full bg-amber-500 p-1 text-white text-2xl" />
      )}

      <div className="flex items-center justify-between mb-2">
        <h3 className="text-lg font-semibold text-ink dark:text-white">{plan.nombre}</h3>
        {etiqueta && (
          <span className="rounded-full bg-black/10 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide text-ink-2 dark:bg-white/10 dark:text-white/80">
            {etiqueta}
          </span>
        )}
      </div>

      <p className="text-sm text-ink-2 mb-4">{plan.descripcion}</p>

      <div className="text-3xl font-bold text-ink mb-1 dark:text-white">
        {precio}
        {!esGratis && (
          <span className="ml-1 text-sm font-normal text-ink-2">/mes</span>
        )}
      </div>
      {!esGratis && (
        <p className="text-xs text-ink-2 mb-4">
          {ciclo === "Mensual"
            ? "Renovación mensual"
            : ciclo === "Trimestral"
              ? "Facturación trimestral"
              : "Facturación anual"}
          {plan.descuentoAnualPorcentaje > 0 &&
            " · -" + plan.descuentoAnualPorcentaje + "% en Anual"}
        </p>
      )}

      <div className="space-y-2 mt-2">
        <FilaBeneficio
          texto={`Hasta ${plan.limiteAnuncios} vehículos publicados`}
          icono={<FaCheck className="text-blue-400" />}
        />
        <FilaBeneficio
          texto={
            plan.cuotaDestacados > 0
              ? `${plan.cuotaDestacados} destacados en la portada`
              : "Sin destacados en la portada"
          }
          icono={
            plan.cuotaDestacados > 0 ? (
              <FaCheck className="text-amber-400" />
            ) : (
              <span className="text-ink-3 dark:text-gray-600">—</span>
            )
          }
        />
        <FilaBeneficio
          texto={`Hasta ${plan.maxFotos} fotos por anuncio`}
          icono={<FaCheck className="text-blue-400" />}
        />
        <FilaBeneficio
          texto={`Anuncio vigente ${plan.diasVigencia} días`}
          icono={<FaCheck className="text-blue-400" />}
        />
        <div className="flex items-start justify-between gap-2 py-1">
          <span className="text-sm text-ink-2 dark:text-gray-300">Prioridad en la vitrina</span>
          <span className={`text-sm font-semibold ${acento}`}>
            {prioridad.etiqueta}
          </span>
        </div>
        <div className="flex gap-1">
          {[1, 2, 3, 4].map((n) => (
            <div
              key={n}
              className={`h-1.5 flex-1 rounded-full ${
                n <= prioridad.nivel ? barra : "bg-black/10 dark:bg-white/10"
              }`}
            />
          ))}
        </div>
        <FilaBeneficio
          texto="Badge del plan en tus vehículos"
          icono={<FaCheck className="text-green-400" />}
        />
        <FilaBeneficio
          texto="Estadísticas de vistas y contactos"
          icono={<FaCheck className="text-green-400" />}
        />
      </div>

      <div className="mt-auto pt-6">{boton}</div>
    </div>
  );
}

function FilaBeneficio({ texto, icono }: { texto: string; icono: React.ReactNode }) {
  return (
    <div className="flex items-start gap-2">
      <span className="mt-0.5 shrink-0">{icono}</span>
      <span className="text-sm text-ink-2 dark:text-gray-300">{texto}</span>
    </div>
  );
}
