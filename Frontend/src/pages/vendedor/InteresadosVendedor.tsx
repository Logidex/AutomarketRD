import { useState } from "react";
import { motion } from "motion/react";
import Swal from "sweetalert2";
import { FaReply } from "react-icons/fa";
import type { LeadDealer } from "../../types/lead.types";
import { formatearFecha } from "../../utils/fecha";
import { useMisLeads, useMarcarLeido } from "../../hooks/useLeads";

const construirMailtoRespuesta = (lead: LeadDealer): string => {
  const vehiculo = lead.anuncio
    ? `${lead.anuncio.marca} ${lead.anuncio.modelo} (${lead.anuncio.anio})`
    : "tu vehículo";

  const asunto = `Re: Tu mensaje sobre ${vehiculo}`;
  const cuerpo = `Hola ${lead.nombreContacto},\n\nGracias por tu interés en ${vehiculo}. Te escribo para responder tu consulta.\n\nSaludos,`;

  return `mailto:${encodeURIComponent(lead.emailContacto)}?subject=${encodeURIComponent(asunto)}&body=${encodeURIComponent(cuerpo)}`;
};

export default function InteresadosVendedor() {
  const { data: leads = [], isLoading: cargando } = useMisLeads();
  const marcarLeido = useMarcarLeido();
  const [mostrandoPendientes, setMostrandoPendientes] = useState(false);

  const pendientes = leads.filter((lead) => !lead.leido);
  const leidos = leads.filter((lead) => lead.leido);
  const visibles = mostrandoPendientes ? pendientes : leads;

  const handleMarcarLeido = async (id: number) => {
    try {
      await marcarLeido.mutateAsync(id);
      window.dispatchEvent(new Event("leads:cambiado"));
    } catch {
      Swal.fire({
        title: "Error",
        text: "No se pudo actualizar el interesado.",
        icon: "error",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  if (cargando) {
    return (
      <p className="py-10 text-center text-sm text-ink-3">
        Cargando interesados…
        <span className="sr-only">Cargando.</span>
      </p>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="text-lg font-semibold text-ink">Interesados</h1>
        <p className="text-sm text-ink-3">
          Mensajes de personas que contactaron por tu vehículo.
        </p>
      </div>

      {pendientes.length > 0 && (
        <div className="flex items-center justify-between rounded-md border border-amber-200 bg-amber-50 px-4 py-3">
          <p className="text-sm text-amber-800">
            Tienes {pendientes.length} mensaje
            {pendientes.length === 1 ? "" : "s"} sin atender.
          </p>
          {mostrandoPendientes && (
            <button
              type="button"
              onClick={() => setMostrandoPendientes(false)}
              className="text-xs text-amber-700 underline underline-offset-2"
            >
              Ver todos
            </button>
          )}
        </div>
      )}

      {!mostrandoPendientes && leidos.length > 0 && (
        <button
          type="button"
          onClick={() => setMostrandoPendientes(true)}
          className="text-sm text-ink-3 underline underline-offset-2 hover:text-ink-2"
        >
          Ver también los ya atendidos ({leidos.length})
        </button>
      )}

      {visibles.length === 0 ? (
        <div className="rounded-lg border border-dashed border-line bg-surface p-10 text-center">
          <p className="text-sm text-ink-3">
            {pendientes.length === 0
              ? "No tienes interesados todavía."
              : "Estás al día con todos tus interesados."}
          </p>
        </div>
      ) : (
        <motion.ul
          className="space-y-4"
          initial="hidden"
          animate="visible"
          variants={{
            hidden: {},
            visible: { transition: { staggerChildren: 0.06 } },
          }}
        >
          {visibles.map((lead) => (
            <motion.li
              key={lead.id}
              variants={{ hidden: { opacity: 0, y: 12 }, visible: { opacity: 1, y: 0 } }}
              transition={{ duration: 0.3 }}
              className="rounded-lg border border-line bg-surface p-5 shadow-sm"
            >
              <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                <div className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-gray-400" aria-hidden="true" />
                  <strong className="text-sm font-semibold text-ink">
                    {lead.nombreContacto}
                  </strong>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-xs text-ink-3">
                    {formatearFecha(lead.fechaCreacionUtc)}
                  </span>
                  {!lead.leido && (
                    <button
                      type="button"
                      onClick={() => void handleMarcarLeido(lead.id)}
                      className="rounded-md border border-line px-2.5 py-1 text-xs font-medium text-ink-2 transition-colors hover:bg-surface-2"
                    >
                      Marcar atendido
                    </button>
                  )}
                </div>
              </div>

              <p className="text-sm text-ink-2">{lead.mensaje}</p>

              <div className="mt-3 space-y-1 border-t border-line pt-3 text-xs text-ink-3">
                <p>
                  <span className="font-medium text-ink-2">Email:</span>{" "}
                  {lead.emailContacto}
                </p>
                {lead.telefonoContacto && (
                  <p>
                    <span className="font-medium text-ink-2">Teléfono:</span>{" "}
                    {lead.telefonoContacto}
                  </p>
                )}
                {lead.canal && (
                  <p>
                    <span className="font-medium text-ink-2">Canal:</span>{" "}
                    {lead.canal}
                  </p>
                )}
              </div>

              {lead.emailContacto && (
                <a
                  href={construirMailtoRespuesta(lead)}
                  className="mt-4 inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-blue-700"
                >
                  <FaReply /> Responder por correo
                </a>
              )}
            </motion.li>
          ))}
        </motion.ul>
      )}
    </div>
  );
}