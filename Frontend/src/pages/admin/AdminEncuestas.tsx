import { FaClipboardList, FaStar } from "react-icons/fa";
import {
  useEncuestaActiva,
  useResultadosEncuesta,
} from "../../hooks/useEncuestas";

/**
 * Resultados de la encuesta de satisfacción (admin): promedios por pregunta
 * de escala, distribución 1-5 y comentarios abiertos.
 */
export default function AdminEncuestas() {
  const { data: encuesta } = useEncuestaActiva(true);
  const { data: resultados, isLoading } = useResultadosEncuesta(
    encuesta?.id ?? null,
  );

  if (!encuesta) {
    return (
      <div className="flex flex-col items-center justify-center gap-3 rounded-2xl border border-line bg-surface p-12 text-center">
        <FaClipboardList className="text-4xl text-ink-3" />
        <h1 className="text-lg font-semibold text-ink">
          No hay encuestas activas
        </h1>
        <p className="text-sm text-ink-2">
          Cuando se cree una encuesta (seeder o panel), sus resultados
          aparecerán aquí.
        </p>
      </div>
    );
  }

  if (isLoading || !resultados) {
    return (
      <div className="rounded-2xl border border-line bg-surface p-12 text-center text-sm text-ink-2">
        Cargando resultados...
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-ink">{resultados.titulo}</h1>
          <p className="mt-1 text-sm text-ink-2">
            {resultados.totalRespondentes}{" "}
            {resultados.totalRespondentes === 1
              ? "usuario ha respondido"
              : "usuarios han respondido"}
            {!resultados.activa && " · encuesta inactiva"}
          </p>
        </div>
      </div>

      {resultados.preguntas.map((p) => (
        <div
          key={p.preguntaId}
          className="rounded-2xl border border-line bg-surface p-5"
        >
          <div className="mb-4 flex flex-wrap items-start justify-between gap-2">
            <h2 className="font-semibold text-ink">{p.texto}</h2>
            <span className="shrink-0 rounded-full bg-surface-2 px-3 py-1 text-xs font-semibold text-ink-2">
              {p.totalRespuestas}{" "}
              {p.totalRespuestas === 1 ? "respuesta" : "respuestas"}
            </span>
          </div>

          {p.tipo === "Escala" ? (
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <FaStar className="text-amber-400" />
                <span className="text-lg font-bold text-ink">
                  {p.promedioEscala?.toFixed(2) ?? "—"}
                </span>
                <span className="text-xs text-ink-3">/ 5 promedio</span>
              </div>

              <div className="space-y-1.5">
                {[5, 4, 3, 2, 1].map((valor) => {
                  const cantidad = p.distribucion[String(valor)] ?? 0;
                  const porcentaje =
                    p.totalRespuestas > 0
                      ? Math.round((cantidad / p.totalRespuestas) * 100)
                      : 0;

                  return (
                    <div key={valor} className="flex items-center gap-3 text-xs">
                      <span className="w-10 shrink-0 text-right text-ink-2">
                        {valor} ★
                      </span>
                      <div className="h-2.5 flex-1 overflow-hidden rounded-full bg-surface-2">
                        <div
                          className={`h-full rounded-full ${
                            valor >= 4
                              ? "bg-green-500"
                              : valor === 3
                                ? "bg-amber-400"
                                : "bg-red-400"
                          }`}
                          style={{ width: `${porcentaje}%` }}
                        />
                      </div>
                      <span className="w-14 shrink-0 text-ink-3">
                        {cantidad} ({porcentaje}%)
                      </span>
                    </div>
                  );
                })}
              </div>
            </div>
          ) : (
            <div className="space-y-3">
              {p.comentarios.length === 0 ? (
                <p className="text-sm text-ink-3">Sin comentarios aún.</p>
              ) : (
                p.comentarios.map((c, i) => (
                  <blockquote
                    key={i}
                    className="rounded-lg border border-line bg-surface-2 p-3 text-sm text-ink"
                  >
                    <p>"{c.texto}"</p>
                    <p className="mt-1 text-xs text-ink-3">
                      {new Date(c.fechaUtc).toLocaleDateString("es-DO", {
                        year: "numeric",
                        month: "short",
                        day: "numeric",
                      })}
                    </p>
                  </blockquote>
                ))
              )}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
