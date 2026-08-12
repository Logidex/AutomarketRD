import { useState } from "react";
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
      <p className="py-10 text-center text-sm text-gray-500">
        Cargando interesados…
        <span className="sr-only">Cargando.</span>
      </p>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="text-lg font-semibold text-gray-900">Interesados</h1>
        <p className="text-sm text-gray-500">
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
          className="text-sm text-gray-500 underline underline-offset-2 hover:text-gray-700"
        >
          Ver también los ya atendidos ({leidos.length})
        </button>
      )}

      {visibles.length === 0 ? (
        <div className="rounded-lg border border-dashed border-gray-300 bg-white p-10 text-center">
          <p className="text-sm text-gray-500">
            {pendientes.length === 0
              ? "No tienes interesados todavía."
              : "Estás al día con todos tus interesados."}
          </p>
        </div>
      ) : (
        <ul className="space-y-4">
          {visibles.map((lead) => (
            <li
              key={lead.id}
              className="rounded-lg border border-gray-200 bg-white p-5 shadow-sm"
            >
              <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                <div className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-gray-400" aria-hidden="true" />
                  <strong className="text-sm font-semibold text-gray-800">
                    {lead.nombreContacto}
                  </strong>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-xs text-gray-400">
                    {formatearFecha(lead.fechaCreacionUtc)}
                  </span>
                  {!lead.leido && (
                    <button
                      type="button"
                      onClick={() => void handleMarcarLeido(lead.id)}
                      className="rounded-md border border-gray-300 px-2.5 py-1 text-xs font-medium text-gray-600 transition-colors hover:bg-gray-50"
                    >
                      Marcar atendido
                    </button>
                  )}
                </div>
              </div>

              <p className="text-sm text-gray-600">{lead.mensaje}</p>

              <div className="mt-3 space-y-1 border-t border-gray-100 pt-3 text-xs text-gray-500">
                <p>
                  <span className="font-medium text-gray-600">Email:</span>{" "}
                  {lead.emailContacto}
                </p>
                {lead.telefonoContacto && (
                  <p>
                    <span className="font-medium text-gray-600">Teléfono:</span>{" "}
                    {lead.telefonoContacto}
                  </p>
                )}
                {lead.canal && (
                  <p>
                    <span className="font-medium text-gray-600">Canal:</span>{" "}
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
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}