import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FaBell, FaCheckDouble, FaEnvelope } from "react-icons/fa";
import { leadService, type LeadNoLeidosResumen } from "../../services/lead.service";
import { formatearFecha } from "../../utils/fecha";

interface CampanaNotificacionesProps {
  rutaLeads: string;
  tema?: "oscuro" | "claro";
}

export default function CampanaNotificaciones({
  rutaLeads,
  tema = "oscuro",
}: CampanaNotificacionesProps) {
  const navigate = useNavigate();
  const [resumen, setResumen] = useState<LeadNoLeidosResumen>({
    cantidadNoLeidos: 0,
    recientes: [],
  });
  const [abierto, setAbierto] = useState(false);
  const [marcando, setMarcando] = useState(false);
  const contenedorRef = useRef<HTMLDivElement>(null);

  const cargar = useCallback(async () => {
    try {
      const datos = await leadService.obtenerResumenNoLeidos();
      setResumen(datos);
    } catch {
      setResumen({ cantidadNoLeidos: 0, recientes: [] });
    }
  }, []);

  useEffect(() => {
    const cargarInicial = window.setTimeout(cargar, 0);
    const intervalo = window.setInterval(cargar, 30000);
    return () => {
      window.clearTimeout(cargarInicial);
      window.clearInterval(intervalo);
    };
  }, [cargar]);

  useEffect(() => {
    const atenderCambio = () => cargar();
    window.addEventListener("leads:cambiado", atenderCambio);
    return () => window.removeEventListener("leads:cambiado", atenderCambio);
  }, [cargar]);

  useEffect(() => {
    const cerrarFuera = (e: MouseEvent) => {
      if (
        contenedorRef.current &&
        !contenedorRef.current.contains(e.target as Node)
      ) {
        setAbierto(false);
      }
    };

    document.addEventListener("mousedown", cerrarFuera);
    return () => document.removeEventListener("mousedown", cerrarFuera);
  }, []);

  const marcarTodos = async () => {
    setMarcando(true);
    try {
      await leadService.marcarTodosLeidos();
      setResumen({ cantidadNoLeidos: 0, recientes: [] });
    } catch {
      setResumen({ cantidadNoLeidos: 0, recientes: [] });
    } finally {
      setMarcando(false);
    }
  };

  const irALeads = () => {
    setAbierto(false);
    navigate(rutaLeads);
  };

  const tienePendientes = resumen.cantidadNoLeidos > 0;

  const estiloBoton =
    tema === "claro"
      ? "border-line bg-surface text-ink-3 hover:border-blue-400 hover:text-blue-600"
      : "border-line bg-hover text-ink-2 hover:border-blue-500/50 hover:text-ink";

  return (
    <div ref={contenedorRef} className="relative">
      <button
        type="button"
        onClick={() => setAbierto((v) => !v)}
        aria-expanded={abierto}
        aria-haspopup="menu"
        title="Notificaciones"
        className={`relative flex h-10 w-10 items-center justify-center rounded-full border transition-colors ${estiloBoton}`}
      >
        <FaBell />
        {tienePendientes && (
          <span className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
            {resumen.cantidadNoLeidos > 99
              ? "99+"
              : resumen.cantidadNoLeidos}
          </span>
        )}
      </button>

      {abierto && (
        <div
          role="menu"
          className="absolute right-0 top-[calc(100%+10px)] z-50 w-80 overflow-hidden rounded-xl border border-line bg-surface shadow-xl"
        >
          <div className="flex items-center justify-between border-b border-line px-4 py-3">
            <p className="text-sm font-semibold text-ink">Notificaciones</p>
            {tienePendientes && (
              <button
                type="button"
                onClick={marcarTodos}
                disabled={marcando}
                className="flex items-center gap-1 text-xs text-blue-400 transition-colors hover:text-blue-300 disabled:opacity-50"
              >
                <FaCheckDouble />
                {marcando ? "Marcando..." : "Marcar todo"}
              </button>
            )}
          </div>

          {resumen.recientes.length === 0 ? (
            <div className="flex flex-col items-center gap-2 px-4 py-8 text-center">
              <FaEnvelope className="text-2xl text-ink-3" />
              <p className="text-sm text-ink-2">
                {tienePendientes
                  ? "Cargando notificaciones…"
                  : "No tienes mensajes sin atender."}
              </p>
            </div>
          ) : (
            <ul className="max-h-80 overflow-y-auto">
              {resumen.recientes.map((lead) => (
                <li key={lead.id}>
                  <button
                    type="button"
                    onClick={irALeads}
                    className="flex w-full flex-col gap-1 border-b border-line px-4 py-3 text-left transition-colors hover:bg-hover"
                  >
                    <span className="flex items-center justify-between gap-2">
                      <strong className="truncate text-sm text-ink">
                        {lead.nombreContacto}
                      </strong>
                      <span className="shrink-0 text-[10px] text-ink-3">
                        {formatearFecha(lead.fechaCreacionUtc)}
                      </span>
                    </span>
                    <span className="line-clamp-2 text-xs text-ink-2">
                      {lead.mensaje}
                    </span>
                    {lead.anuncio && (
                      <span className="text-[11px] text-ink-3">
                        {lead.anuncio.marca} {lead.anuncio.modelo} (
                        {lead.anuncio.anio})
                      </span>
                    )}
                  </button>
                </li>
              ))}
            </ul>
          )}

          <button
            type="button"
            onClick={irALeads}
            className="block w-full border-t border-line px-4 py-3 text-center text-sm font-medium text-blue-400 transition-colors hover:bg-hover"
          >
            Ver todos los mensajes
          </button>
        </div>
      )}
    </div>
  );
}