import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import { FaStar, FaTimes } from "react-icons/fa";
import Swal from "sweetalert2";
import { authService } from "../services/auth.service";
import type { EncuestaActiva } from "../services/encuestas.service";
import { useEncuestaActiva, useResponderEncuesta } from "../hooks/useEncuestas";

const VISITAS_MINIMAS = 3;
const RETRASO_MOSTRAR_MS = 2500;

/**
 * Encuesta de satisfacción: se muestra a partir de la 3ra visita y solo si
 * está autenticado (el límite de 1 respuesta por usuario lo garantiza la BD
 * con un índice único). Si el usuario la cierra sin responder, se oculta por
 * el resto de la sesión y vuelve a intentar en la próxima; una vez
 * respondida, no vuelve a aparecer nunca.
 */
export default function ModalEncuesta() {
  const location = useLocation();
  const usuario = authService.getCurrentUser();

  const [retrasoCumplido, setRetrasoCumplido] = useState(false);
  const [cerrada, setCerrada] = useState(false);
  const [respuestasEscala, setRespuestasEscala] = useState<Record<number, number>>({});
  const [textosAbiertos, setTextosAbiertos] = useState<Record<number, string>>({});

  const responder = useResponderEncuesta();

  // Contador de visitas: 1 por sesión de navegador. No es estado: el valor
  // relevante se lee de localStorage en cada render (solo importa a partir
  // de la 3ra sesión, cuando ya está escrito).
  useEffect(() => {
    if (!sessionStorage.getItem("am-sesion-contada")) {
      sessionStorage.setItem("am-sesion-contada", "1");
      const visitas = Number(localStorage.getItem("am-visitas") ?? "0") + 1;
      localStorage.setItem("am-visitas", String(visitas));
    }
  }, []);

  // Pequeño delay para no interrumpir la carga de la página.
  useEffect(() => {
    const t = setTimeout(() => setRetrasoCumplido(true), RETRASO_MOSTRAR_MS);
    return () => clearTimeout(t);
  }, []);

  const visitas = Number(localStorage.getItem("am-visitas") ?? "0");
  const habilitada = Boolean(usuario) && visitas >= VISITAS_MINIMAS;
  const { data: encuesta } = useEncuestaActiva(habilitada);

  const enAdmin = location.pathname.startsWith("/admin");
  const respondida = encuesta
    ? encuesta.yaRespondio || localStorage.getItem(`encuesta-respondida-${encuesta.id}`) === "1"
    : false;
  // Descarte solo para la sesión actual: si no respondió, reaparece en la próxima.
  const descartada = encuesta
    ? sessionStorage.getItem(`encuesta-descartada-${encuesta.id}`) === "1"
    : false;

  const mostrar =
    habilitada &&
    retrasoCumplido &&
    !enAdmin &&
    !cerrada &&
    encuesta !== undefined &&
    encuesta !== null &&
    !respondida &&
    !descartada;

  const cerrar = () => {
    if (encuesta) {
      // sessionStorage: el descarte dura lo que dura la sesión del navegador.
      // En la próxima sesión vuelve a mostrarse si aún no ha respondido.
      sessionStorage.setItem(`encuesta-descartada-${encuesta.id}`, "1");
    }
    setCerrada(true);
  };

  const todasLasEscalasRespondidas = (e: EncuestaActiva) =>
    e.preguntas
      .filter((p) => p.tipo === "Escala")
      .every((p) => (respuestasEscala[p.id] ?? 0) >= 1);

  const enviar = async () => {
    if (!encuesta || responder.isPending) return;

    const respuestas = encuesta.preguntas.map((p) =>
      p.tipo === "Escala"
        ? { preguntaId: p.id, valorEscala: respuestasEscala[p.id] }
        : { preguntaId: p.id, valorTexto: textosAbiertos[p.id] ?? "" },
    );

    try {
      await responder.mutateAsync({ encuestaId: encuesta.id, respuestas });
      localStorage.setItem(`encuesta-respondida-${encuesta.id}`, "1");
      setCerrada(true);
      await Swal.fire({
        icon: "success",
        title: "¡Gracias por tu opinión!",
        text: "Nos ayuda a mejorar AutoMarket RD para todos.",
        confirmButtonColor: "#3b82f6",
      });
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "No se pudo enviar",
        text: err instanceof Error ? err.message : "Inténtalo nuevamente.",
        confirmButtonColor: "#3b82f6",
      });
    }
  };

  if (!mostrar || !encuesta) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      onClick={cerrar}
    >
      <div
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl border border-line bg-surface p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-1 flex items-start justify-between gap-3">
          <h2 className="text-xl font-bold text-ink">{encuesta.titulo}</h2>
          <button
            type="button"
            onClick={cerrar}
            aria-label="Cerrar encuesta"
            className="shrink-0 rounded-lg p-1.5 text-ink-3 transition-colors hover:bg-hover hover:text-ink"
          >
            <FaTimes />
          </button>
        </div>

        {encuesta.descripcion && (
          <p className="mb-5 text-sm text-ink-2">{encuesta.descripcion}</p>
        )}

        <div className="space-y-5">
          {encuesta.preguntas.map((p) => (
            <div key={p.id}>
              <p className="mb-2 text-sm font-medium text-ink">{p.texto}</p>

              {p.tipo === "Escala" ? (
                <div className="flex gap-1.5">
                  {[1, 2, 3, 4, 5].map((valor) => (
                    <button
                      key={valor}
                      type="button"
                      onClick={() =>
                        setRespuestasEscala((prev) => ({ ...prev, [p.id]: valor }))
                      }
                      aria-label={`${valor} de 5`}
                      className={`text-2xl transition-colors ${
                        (respuestasEscala[p.id] ?? 0) >= valor
                          ? "text-amber-400"
                          : "text-line hover:text-amber-200"
                      }`}
                    >
                      <FaStar />
                    </button>
                  ))}
                </div>
              ) : (
                <textarea
                  value={textosAbiertos[p.id] ?? ""}
                  onChange={(e) =>
                    setTextosAbiertos((prev) => ({ ...prev, [p.id]: e.target.value }))
                  }
                  rows={3}
                  maxLength={1000}
                  placeholder="Cuéntanos (opcional)"
                  className="w-full rounded-lg border border-line bg-input px-3 py-2 text-sm text-ink placeholder:text-ink-3 focus:border-blue-500 focus:outline-none"
                />
              )}
            </div>
          ))}
        </div>

        <button
          type="button"
          onClick={enviar}
          disabled={!todasLasEscalasRespondidas(encuesta) || responder.isPending}
          className="mt-6 w-full rounded-lg bg-blue-500 py-3 text-sm font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {responder.isPending ? "Enviando..." : "Enviar respuestas"}
        </button>
      </div>
    </div>
  );
}
