import { useState } from "react";
import {
  FaComments,
  FaHeadset,
  FaLock,
  FaPaperPlane,
  FaPlus,
  FaTimes,
} from "react-icons/fa";
import type {
  TicketCategoria,
  TicketDetalle,
  TicketEstado,
  TicketListado,
  TicketPrioridad,
} from "../types/ticket.types";
import Spinner from "../components/Spinner";
import { formatearFecha } from "../utils/fecha";
import {
  useCerrarTicket,
  useCrearTicket,
  useMisTickets,
  useResponderTicket,
  useTicket,
} from "../hooks/useTickets";

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
  Cerrado: "bg-gray-100 text-gray-600",
  Detenido: "bg-slate-200 text-slate-700",
};

const PRIORIDAD_CLASES: Record<TicketPrioridad, string> = {
  Baja: "bg-gray-100 text-gray-600",
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

const CATEGORIAS: TicketCategoria[] = [
  "General",
  "Facturacion",
  "Anuncios",
  "SoporteTecnico",
  "Cuenta",
];

const PRIORIDADES: TicketPrioridad[] = ["Baja", "Normal", "Alta", "Urgente"];

export default function Soporte() {
  const { data: tickets = [], isLoading: cargandoLista } = useMisTickets();
  const [seleccionado, setSeleccionado] = useState<number | null>(null);
  const [mostrandoFormulario, setMostrandoFormulario] = useState(false);

  const seleccionadoReal = seleccionado ?? tickets[0]?.id ?? null;
  const ticketActivo = useTicket(seleccionadoReal);

  return (
    <div className="mx-auto max-w-7xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="flex items-center gap-3 text-2xl font-bold text-gray-900">
            <FaHeadset className="text-blue-600" />
            Soporte
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Tickets de ayuda con tu cuenta, facturación y anuncios
          </p>
        </div>

        <button
          type="button"
          onClick={() => setMostrandoFormulario(true)}
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-700"
        >
          <FaPlus /> Nuevo ticket
        </button>
      </div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(0,3fr)]">
        {/* LISTADO */}
        <section className="rounded-2xl border border-gray-200 bg-white shadow-sm">
          <div className="border-b border-gray-100 px-5 py-4">
            <h2 className="text-sm font-bold uppercase tracking-wide text-gray-500">
              Mis tickets ({tickets.length})
            </h2>
          </div>

          {cargandoLista ? (
            <div className="p-6">
              <Spinner />
            </div>
          ) : tickets.length === 0 ? (
            <div className="flex flex-col items-center gap-3 px-6 py-12 text-center">
              <div className="flex h-14 w-14 items-center justify-center rounded-full bg-blue-50">
                <FaComments className="text-2xl text-blue-600" />
              </div>
              <p className="text-gray-600">Aún no has abierto tickets.</p>
              <button
                type="button"
                onClick={() => setMostrandoFormulario(true)}
                className="text-sm font-semibold text-blue-600 hover:text-blue-700"
              >
                Crear el primero
              </button>
            </div>
          ) : (
            <ul className="max-h-[70vh] divide-y divide-gray-100 overflow-y-auto">
              {tickets.map((ticket) => (
                <ListaItemTicket
                  key={ticket.id}
                  ticket={ticket}
                  activo={ticket.id === seleccionadoReal}
                  onClick={() => setSeleccionado(ticket.id)}
                />
              ))}
            </ul>
          )}
        </section>

        {/* DETALLE */}
        <section className="min-h-[500px] rounded-2xl border border-gray-200 bg-white shadow-sm">
          <DetalleTicket
            key={ticketActivo.data?.id ?? "ninguno"}
            ticket={ticketActivo.data}
            cargando={ticketActivo.isLoading}
          />
        </section>
      </div>

      {mostrandoFormulario && (
        <FormularioCrearTicket
          onCerrar={() => setMostrandoFormulario(false)}
          onCreado={(id) => {
            setMostrandoFormulario(false);
            setSeleccionado(id);
          }}
        />
      )}
    </div>
  );
}

function ListaItemTicket({
  ticket,
  activo,
  onClick,
}: {
  ticket: TicketListado;
  activo: boolean;
  onClick: () => void;
}) {
  const hayRespuestaNueva =
    ticket.ultimoMensajeEsAdmin &&
    ticket.estado !== "Cerrado" &&
    ticket.estado !== "Resuelto";

  return (
    <li>
      <button
        type="button"
        onClick={onClick}
        className={`flex w-full flex-col gap-2 px-5 py-4 text-left transition-colors ${
          activo ? "bg-blue-50/70" : "hover:bg-gray-50"
        }`}
      >
        <div className="flex items-center justify-between gap-2">
          <span className="truncate text-sm font-bold text-gray-900">
            {ticket.asunto}
          </span>
          {hayRespuestaNueva && (
            <span className="shrink-0 rounded-full bg-green-100 px-2 py-0.5 text-[10px] font-bold uppercase text-green-700">
              Respuesta
            </span>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-1.5">
          <span
            className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${ESTADO_CLASES[ticket.estado]}`}
          >
            {ESTADO_LABEL[ticket.estado]}
          </span>
          <span
            className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${PRIORIDAD_CLASES[ticket.prioridad]}`}
          >
            {ticket.prioridad}
          </span>
          <span className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs text-gray-600">
            {CATEGORIA_LABEL[ticket.categoria]}
          </span>
        </div>

        <p className="truncate text-xs text-gray-500">
          {ticket.ultimoMensaje || "Sin mensajes"}
        </p>

        <p className="text-[11px] text-gray-400">
          {ticket.cantidadMensajes} mensaje{ticket.cantidadMensajes === 1 ? "" : "s"} ·{" "}
          {formatearFecha(ticket.fechaActualizacionUtc, true)}
        </p>
      </button>
    </li>
  );
}

function DetalleTicket({
  ticket,
  cargando,
}: {
  ticket?: TicketDetalle;
  cargando: boolean;
}) {
  const responder = useResponderTicket();
  const cerrar = useCerrarTicket();
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
      <div className="flex h-full flex-col items-center justify-center gap-3 p-10 text-center text-gray-500">
        <FaComments className="text-4xl text-gray-300" />
        <p>Selecciona un ticket para ver la conversación.</p>
      </div>
    );
  }

  const estaCerrado = ticket.estado === "Cerrado";
  const estaDetenido = ticket.estado === "Detenido";

  const enviarRespuesta = () => {
    const texto = mensaje.trim();
    if (!texto) return;
    responder.mutate(
      { id: ticket.id, mensaje: texto },
      { onSuccess: () => setMensaje("") },
    );
  };

  const cerrarTicket = () => {
    cerrar.mutate(ticket.id);
  };

  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-gray-100 px-6 py-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h2 className="text-lg font-bold text-gray-900">{ticket.asunto}</h2>
          <span
            className={`rounded-full px-3 py-1 text-xs font-semibold ${ESTADO_CLASES[ticket.estado]}`}
          >
            {ESTADO_LABEL[ticket.estado]}
          </span>
        </div>

        <div className="mt-2 flex flex-wrap items-center gap-2 text-xs text-gray-500">
          <span
            className={`rounded-full px-2.5 py-0.5 font-semibold ${PRIORIDAD_CLASES[ticket.prioridad]}`}
          >
            Prioridad: {ticket.prioridad}
          </span>
          <span className="rounded-full bg-gray-100 px-2.5 py-0.5">
            {CATEGORIA_LABEL[ticket.categoria]}
          </span>
          <span>Abierto: {formatearFecha(ticket.fechaCreacionUtc, true)}</span>
        </div>
      </div>

      <div className="flex-1 space-y-4 overflow-y-auto p-6">
        {ticket.mensajes.map((mensajeItem) => (
          <div
            key={mensajeItem.id}
            className={`flex flex-col gap-1 ${
              mensajeItem.esAdmin ? "items-start" : "items-end"
            }`}
          >
            <div
              className={`max-w-[85%] rounded-2xl px-4 py-3 text-sm leading-relaxed shadow-sm ${
                mensajeItem.esAdmin
                  ? "rounded-tl-sm bg-gray-100 text-gray-800"
                  : "rounded-tr-sm bg-blue-600 text-white"
              }`}
            >
              <p className="whitespace-pre-wrap">{mensajeItem.mensaje}</p>
            </div>
            <p className="text-[11px] text-gray-400">
              {mensajeItem.esAdmin ? "Soporte AutoMarket RD" : "Tú"} ·{" "}
              {formatearFecha(mensajeItem.fechaCreacionUtc, true)}
            </p>
          </div>
        ))}
      </div>

      <div className="border-t border-gray-100 p-4">
        {estaCerrado ? (
          <div className="flex items-center gap-2 rounded-lg bg-gray-50 px-4 py-3 text-sm text-gray-500">
            <FaLock className="text-gray-400" />
            Este ticket está cerrado. Si necesitas más ayuda, abre un nuevo
            ticket.
          </div>
        ) : estaDetenido ? (
          <div className="flex items-center justify-between gap-3">
            <div className="flex items-center gap-2 rounded-lg bg-slate-50 px-4 py-3 text-sm text-slate-600">
              <FaLock className="text-slate-400" />
              Este ticket está detenido mientras resolvemos tu caso. No puedes
              enviar mensajes por ahora.
            </div>
            <button
              type="button"
              onClick={cerrarTicket}
              disabled={cerrar.isPending}
              className="shrink-0 rounded-xl border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-600 transition-colors hover:bg-gray-50 disabled:opacity-50"
            >
              Cerrar ticket
            </button>
          </div>
        ) : (
          <div className="flex gap-2">
            <textarea
              value={mensaje}
              onChange={(e) => setMensaje(e.target.value)}
              rows={2}
              maxLength={2000}
              placeholder="Escribe tu respuesta..."
              className="flex-1 resize-none rounded-xl border border-gray-300 px-4 py-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            <div className="flex flex-col gap-2">
              <button
                type="button"
                onClick={enviarRespuesta}
                disabled={!mensaje.trim() || responder.isPending}
                className="inline-flex items-center justify-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <FaPaperPlane /> Enviar
              </button>
              <button
                type="button"
                onClick={cerrarTicket}
                disabled={cerrar.isPending}
                className="rounded-xl border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-600 transition-colors hover:bg-gray-50 disabled:opacity-50"
              >
                Cerrar ticket
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

function FormularioCrearTicket({
  onCerrar,
  onCreado,
}: {
  onCerrar: () => void;
  onCreado: (id: number) => void;
}) {
  const crear = useCrearTicket();
  const [asunto, setAsunto] = useState("");
  const [categoria, setCategoria] = useState<TicketCategoria>("General");
  const [prioridad, setPrioridad] = useState<TicketPrioridad>("Normal");
  const [mensaje, setMensaje] = useState("");

  const enviar = (e: React.FormEvent) => {
    e.preventDefault();
    if (!asunto.trim() || mensaje.trim().length < 10) return;

    crear.mutate(
      {
        asunto: asunto.trim(),
        categoria,
        prioridad,
        mensaje: mensaje.trim(),
      },
      {
        onSuccess: (data) => onCreado(data.ticketId),
      },
    );
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <form
        onSubmit={enviar}
        className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-white p-6 shadow-xl"
      >
        <div className="mb-5 flex items-center justify-between">
          <h2 className="text-lg font-bold text-gray-900">Nuevo ticket de soporte</h2>
          <button
            type="button"
            onClick={onCerrar}
            className="rounded-lg p-2 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600"
          >
            <FaTimes />
          </button>
        </div>

        <div className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-semibold text-gray-700">
              Asunto
            </label>
            <input
              type="text"
              value={asunto}
              onChange={(e) => setAsunto(e.target.value)}
              maxLength={150}
              placeholder="Ej: No puedo renovar mi plan"
              className="w-full rounded-xl border border-gray-300 px-4 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="mb-1 block text-sm font-semibold text-gray-700">
                Categoría
              </label>
              <select
                value={categoria}
                onChange={(e) => setCategoria(e.target.value as TicketCategoria)}
                className="w-full rounded-xl border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none"
              >
                {CATEGORIAS.map((c) => (
                  <option key={c} value={c}>
                    {CATEGORIA_LABEL[c]}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-1 block text-sm font-semibold text-gray-700">
                Prioridad
              </label>
              <select
                value={prioridad}
                onChange={(e) => setPrioridad(e.target.value as TicketPrioridad)}
                className="w-full rounded-xl border border-gray-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none"
              >
                {PRIORIDADES.map((p) => (
                  <option key={p} value={p}>
                    {p}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold text-gray-700">
              Mensaje
            </label>
            <textarea
              value={mensaje}
              onChange={(e) => setMensaje(e.target.value)}
              rows={5}
              maxLength={2000}
              placeholder="Describe el problema con el mayor detalle posible..."
              className="w-full resize-none rounded-xl border border-gray-300 px-4 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            <p className="mt-1 text-right text-[11px] text-gray-400">
              {mensaje.length}/2000
            </p>
          </div>
        </div>

        {crear.error && (
          <p className="mt-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600">
            {crear.error.message}
          </p>
        )}

        <div className="mt-6 flex justify-end gap-2">
          <button
            type="button"
            onClick={onCerrar}
            className="rounded-xl border border-gray-300 px-4 py-2.5 text-sm font-semibold text-gray-600 transition-colors hover:bg-gray-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={
              crear.isPending ||
              !asunto.trim() ||
              mensaje.trim().length < 10
            }
            className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {crear.isPending ? "Enviando..." : "Abrir ticket"}
          </button>
        </div>
      </form>
    </div>
  );
}
