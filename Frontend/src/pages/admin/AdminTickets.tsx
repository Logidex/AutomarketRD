import { useState } from "react";
import {
  FaEnvelope,
  FaHeadset,
  FaPaperPlane,
  FaUserTie,
} from "react-icons/fa";
import type {
  TicketCategoria,
  TicketDetalle,
  TicketEstado,
  TicketPrioridad,
} from "../../types/ticket.types";
import Spinner from "../../components/ui/Spinner";
import { formatearFecha } from "../../utils/fecha";
import {
  useCambiarEstadoTicket,
  useResponderTicketAdmin,
  useTicketAdmin,
  useTicketsAdmin,
} from "../../hooks/useTickets";

const ESTADO_LABEL: Record<TicketEstado, string> = {
  Abierto: "Abierto",
  EnProceso: "En proceso",
  Resuelto: "Resuelto",
  Cerrado: "Cerrado",
  Detenido: "Detenido",
};

const ESTADO_CLASES: Record<TicketEstado, string> = {
  Abierto: "bg-blue-100 text-blue-700",
  EnProceso: "bg-amber-100 text-amber-700",
  Resuelto: "bg-green-100 text-green-700",
  Cerrado: "bg-surface-2 text-ink-2",
  Detenido: "bg-slate-200 text-slate-700",
};

const PRIORIDAD_CLASES: Record<TicketPrioridad, string> = {
  Baja: "bg-surface-2 text-ink-2",
  Normal: "bg-blue-50 text-blue-600",
  Alta: "bg-orange-100 text-orange-700",
  Urgente: "bg-red-100 text-red-700",
};

const CATEGORIA_LABEL: Record<TicketCategoria, string> = {
  General: "General",
  Facturacion: "Facturación",
  Anuncios: "Anuncios",
  SoporteTecnico: "Soporte técnico",
  Cuenta: "Cuenta",
};

const FILTROS_ESTADO: Array<TicketEstado | "Todos"> = [
  "Todos",
  "Abierto",
  "EnProceso",
  "Detenido",
  "Resuelto",
  "Cerrado",
];

export default function AdminTickets() {
  const { data: tickets = [], isLoading: cargandoLista } = useTicketsAdmin();
  const [seleccionado, setSeleccionado] = useState<number | null>(null);
  const [filtro, setFiltro] = useState<TicketEstado | "Todos">("Todos");

  const seleccionadoReal = seleccionado ?? tickets[0]?.id ?? null;
  const ticketActivo = useTicketAdmin(seleccionadoReal);

  const ticketsFiltrados =
    filtro === "Todos"
      ? tickets
      : tickets.filter((t) => t.estado === filtro);

  const abiertos = tickets.filter(
    (t) => t.estado === "Abierto" || t.estado === "EnProceso",
  ).length;

  return (
    <div className="mx-auto max-w-7xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="flex items-center gap-3 text-2xl font-bold text-ink">
            <FaHeadset className="text-violet-600" />
            Tickets de soporte
          </h1>
          <p className="mt-1 text-sm text-ink-3">
            Atiende las solicitudes de ayuda de tus clientes
          </p>
        </div>

        <div className="rounded-lg bg-violet-50 px-4 py-2 text-sm font-semibold text-violet-700">
          {abiertos} sin atender
        </div>
      </div>

      <div className="mb-4 flex flex-wrap gap-2">
        {FILTROS_ESTADO.map((estado) => (
          <button
            key={estado}
            type="button"
            onClick={() => setFiltro(estado)}
            className={`rounded-full px-4 py-1.5 text-xs font-semibold transition-colors ${
              filtro === estado
                ? "bg-violet-600 text-white"
                : "bg-surface text-ink-2 hover:bg-surface-2"
            }`}
          >
            {estado === "Todos" ? "Todos" : ESTADO_LABEL[estado]}
          </button>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(0,3fr)]">
        {/* LISTADO */}
        <section className="rounded-2xl border border-line bg-surface shadow-sm">
          <div className="border-b border-line px-5 py-4">
            <h2 className="text-sm font-bold uppercase tracking-wide text-ink-3">
              {filtro === "Todos" ? "Todos los tickets" : ESTADO_LABEL[filtro]} (
              {ticketsFiltrados.length})
            </h2>
          </div>

          {cargandoLista ? (
            <div className="p-6">
              <Spinner />
            </div>
          ) : ticketsFiltrados.length === 0 ? (
            <div className="flex flex-col items-center gap-3 px-6 py-12 text-center">
              <FaEnvelope className="text-3xl text-ink-3" />
              <p className="text-ink-3">No hay tickets en este filtro.</p>
            </div>
          ) : (
            <ul className="max-h-[70vh] divide-y divide-line overflow-y-auto">
              {ticketsFiltrados.map((ticket) => (
                <li key={ticket.id}>
                  <button
                    type="button"
                    onClick={() => setSeleccionado(ticket.id)}
                    className={`flex w-full flex-col gap-2 px-5 py-4 text-left transition-colors ${
                      ticket.id === seleccionadoReal
                        ? "bg-violet-50/70"
                        : "hover:bg-surface-2"
                    }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <span className="truncate text-sm font-bold text-ink">
                        #{ticket.id} · {ticket.asunto}
                      </span>
                      <span
                        className={`shrink-0 rounded-full px-2.5 py-0.5 text-xs font-semibold ${ESTADO_CLASES[ticket.estado]}`}
                      >
                        {ESTADO_LABEL[ticket.estado]}
                      </span>
                    </div>

                    <div className="flex flex-wrap items-center gap-1.5 text-xs">
                      <span className="inline-flex items-center gap-1 rounded-full bg-surface-2 px-2.5 py-0.5 text-ink-2">
                        <FaUserTie /> {ticket.usuarioNombre}
                      </span>
                      <span
                        className={`rounded-full px-2.5 py-0.5 font-semibold ${PRIORIDAD_CLASES[ticket.prioridad]}`}
                      >
                        {ticket.prioridad}
                      </span>
                      <span className="rounded-full bg-surface-2 px-2.5 py-0.5 text-ink-2">
                        {CATEGORIA_LABEL[ticket.categoria]}
                      </span>
                    </div>

                    <p className="truncate text-xs text-ink-3">
                      {ticket.ultimoMensaje}
                    </p>

                    <p className="text-[11px] text-ink-3">
                      {ticket.usuarioEmail} · {formatearFecha(ticket.fechaActualizacionUtc, true)}
                    </p>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>

        {/* DETALLE */}
        <section className="min-h-[500px] rounded-2xl border border-line bg-surface shadow-sm">
          <DetalleAdminTicket
            key={ticketActivo.data?.id ?? "ninguno"}
            ticket={ticketActivo.data}
            cargando={ticketActivo.isLoading}
          />
        </section>
      </div>
    </div>
  );
}

function DetalleAdminTicket({
  ticket,
  cargando,
}: {
  ticket?: TicketDetalle;
  cargando: boolean;
}) {
  const responder = useResponderTicketAdmin();
  const cambiarEstado = useCambiarEstadoTicket();
  const [mensaje, setMensaje] = useState("");

  if (cargando) {
    return (
      <div className="flex h-full items-center justify-center p-10">
        <Spinner />
      </div>
    );
  }

  if (!ticket) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-3 p-10 text-center text-ink-3">
        <FaEnvelope className="text-4xl text-ink-3" />
        <p>Selecciona un ticket para ver la conversación.</p>
      </div>
    );
  }

  const estaCerrado = ticket.estado === "Cerrado";

  const enviarRespuesta = () => {
    const texto = mensaje.trim();
    if (!texto || estaCerrado) return;
    responder.mutate(
      { id: ticket.id, mensaje: texto },
      { onSuccess: () => setMensaje("") },
    );
  };

  const estadosAccion = (["EnProceso", "Detenido", "Resuelto", "Cerrado"] as TicketEstado[]).filter(
    (e) => e !== ticket.estado,
  );

  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-line px-6 py-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h2 className="text-lg font-bold text-ink">
            #{ticket.id} · {ticket.asunto}
          </h2>
          <span
            className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTADO_CLASES[ticket.estado]}`}
          >
            {ESTADO_LABEL[ticket.estado]}
          </span>
        </div>

        <div className="mt-2 flex flex-wrap items-center gap-2 text-xs text-ink-3">
          <span className="inline-flex items-center gap-1 rounded-full bg-surface-2 px-2.5 py-0.5">
            <FaUserTie /> {ticket.usuarioNombre} ({ticket.usuarioEmail})
          </span>
          <span
            className={`rounded-full px-2.5 py-0.5 font-semibold ${PRIORIDAD_CLASES[ticket.prioridad]}`}
          >
            {ticket.prioridad}
          </span>
          <span className="rounded-full bg-surface-2 px-2.5 py-0.5">
            {CATEGORIA_LABEL[ticket.categoria]}
          </span>
          <span>Abierto: {formatearFecha(ticket.fechaCreacionUtc, true)}</span>
        </div>

        {estadosAccion.length > 0 && (
          <div className="mt-3 flex flex-wrap items-center gap-2">
            <span className="text-xs font-semibold text-ink-3">Cambiar estado:</span>
            {estadosAccion.map((estado) => (
              <button
                key={estado}
                type="button"
                onClick={() => cambiarEstado.mutate({ id: ticket.id, estado })}
                disabled={cambiarEstado.isPending}
                className={`rounded-full px-3 py-1 text-xs font-semibold transition-colors ${ESTADO_CLASES[estado]} hover:ring-1 hover:ring-violet-400 disabled:opacity-50`}
              >
                Marcar {ESTADO_LABEL[estado]}
              </button>
            ))}
          </div>
        )}
      </div>

      <div className="flex-1 space-y-4 overflow-y-auto p-6">
        {ticket.mensajes.map((mensajeItem) => (
          <div
            key={mensajeItem.id}
            className={`flex flex-col gap-1 ${
              mensajeItem.esAdmin ? "items-end" : "items-start"
            }`}
          >
            <div
              className={`max-w-[85%] rounded-2xl px-4 py-3 text-sm leading-relaxed shadow-sm ${
                mensajeItem.esAdmin
                  ? "rounded-tr-sm bg-violet-600 text-white"
                  : "rounded-tl-sm bg-surface-2 text-ink"
              }`}
            >
              <p className="whitespace-pre-wrap">{mensajeItem.mensaje}</p>
            </div>
            <p className="text-[11px] text-ink-3">
              {mensajeItem.esAdmin ? "Tú (Soporte)" : mensajeItem.autorNombre} ·{" "}
              {formatearFecha(mensajeItem.fechaCreacionUtc, true)}
            </p>
          </div>
        ))}
      </div>

      <div className="border-t border-line p-4">
        {estaCerrado ? (
          <div className="rounded-lg bg-surface-2 px-4 py-3 text-sm text-ink-3">
            Este ticket está cerrado. Para responder, primero vuelve a abrirlo
            cambiando su estado.
          </div>
        ) : (
          <div className="flex gap-2">
            <textarea
              value={mensaje}
              onChange={(e) => setMensaje(e.target.value)}
              rows={2}
              maxLength={2000}
              placeholder="Escribe tu respuesta al cliente..."
              className="flex-1 resize-none rounded-xl border border-line px-4 py-3 text-sm focus:border-violet-500 focus:outline-none focus:ring-1 focus:ring-violet-500"
            />
            <button
              type="button"
              onClick={enviarRespuesta}
              disabled={!mensaje.trim() || responder.isPending}
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-violet-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-violet-700 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <FaPaperPlane /> Enviar
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
