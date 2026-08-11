import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import {
  FaEnvelopeOpenText,
  FaEnvelope,
  FaPhoneAlt,
  FaReply,
} from "react-icons/fa";
import { leadService } from "../services/lead.service";
import type { LeadDealer } from "../types/lead.types";
import Spinner from "../components/Spinner";
import { formatearFecha } from "../utils/fecha";

const construirMailtoRespuesta = (lead: LeadDealer): string => {
  const vehiculo = lead.anuncio
    ? `${lead.anuncio.marca} ${lead.anuncio.modelo} (${lead.anuncio.anio})`
    : "tu vehículo";

  const asunto = `Re: Tu mensaje sobre ${vehiculo}`;
  const cuerpo = `Hola ${lead.nombreContacto},\n\nGracias por tu interés en ${vehiculo}. Te escribo para responder tu consulta.\n\nSaludos,`;

  return `mailto:${encodeURIComponent(lead.emailContacto)}?subject=${encodeURIComponent(asunto)}&body=${encodeURIComponent(cuerpo)}`;
};

export default function Leads() {
  const [leads, setLeads] = useState<LeadDealer[]>([]);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    const fetchLeads = async () => {
      try {
        setCargando(true);
        const data = await leadService.obtenerMisLeads();
        setLeads(data);

        // Al abrir el panel se marcan todos como leídos automáticamente:
        // evita que el dealer tenga que marcar mensaje por mensaje.
        leadService
          .marcarTodosLeidos()
          .then(() => {
            setLeads((prev) => prev.map((lead) => ({ ...lead, leido: true })));
            window.dispatchEvent(new Event("leads:cambiado"));
          })
          .catch(() => {});
      } catch (error) {
        console.error(error);
        Swal.fire({
          title: "Error",
          text: "No se pudieron cargar los leads.",
          icon: "error",
          confirmButtonColor: "#ef4444",
        });
      } finally {
        setCargando(false);
      }
    };

    fetchLeads();
  }, []);

  const noLeidos = leads.filter((lead) => !lead.leido).length;

  return (
    <div className="mx-auto max-w-6xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mis Leads</h1>
          <p className="mt-1 text-sm text-gray-500">
            Contactos de interesados en tus vehículos
          </p>
        </div>

        <div className="rounded-lg bg-blue-50 px-4 py-2 text-sm font-semibold text-blue-700">
          {noLeidos} sin leer
        </div>
      </div>

      {cargando ? (
        <Spinner />
      ) : leads.length === 0 ? (
        <div className="flex min-h-[300px] flex-col items-center justify-center rounded-2xl border border-gray-200 bg-white px-6 text-center shadow-sm">
          <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-blue-50">
            <FaEnvelope className="text-3xl text-blue-600" />
          </div>
          <h2 className="mb-1 text-xl font-bold text-gray-800">
            No tienes leads todavía
          </h2>
          <p className="max-w-md text-gray-500">
            Cuando un comprador te contacte desde tus anuncios, verás su
            mensaje aquí.
          </p>
        </div>
      ) : (
        <ul className="space-y-4">
          {leads.map((lead) => (
            <li
              key={lead.id}
              className={`rounded-2xl border bg-white p-5 shadow-sm transition-shadow hover:shadow-md ${
                !lead.leido ? "border-blue-200 ring-1 ring-blue-100" : "border-gray-200"
              }`}
            >
              <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className="text-lg font-bold text-gray-900">
                      {lead.nombreContacto}
                    </h3>

                    {!lead.leido && (
                      <span className="inline-flex items-center gap-1 rounded-full bg-blue-100 px-3 py-0.5 text-xs font-semibold text-blue-700">
                        <FaEnvelopeOpenText /> Nuevo
                      </span>
                    )}
                  </div>

                  <p className="mt-3 whitespace-pre-wrap text-sm leading-relaxed text-gray-700">
                    {lead.mensaje}
                  </p>

                  <div className="mt-4 flex flex-wrap gap-2 text-xs text-gray-500">
                    {lead.telefonoContacto && (
                      <span className="inline-flex items-center gap-1.5 rounded-full bg-gray-100 px-3 py-1">
                        <FaPhoneAlt /> {lead.telefonoContacto}
                      </span>
                    )}
                    {lead.emailContacto && (
                      <span className="inline-flex items-center gap-1.5 rounded-full bg-gray-100 px-3 py-1">
                        <FaEnvelope /> {lead.emailContacto}
                      </span>
                    )}
                    <span className="rounded-full bg-gray-100 px-3 py-1">
                      Vía: {lead.canal}
                    </span>
                    <span className="rounded-full bg-gray-100 px-3 py-1">
                      {formatearFecha(lead.fechaCreacionUtc, true)}
                    </span>
                  </div>

                  {lead.emailContacto && (
                    <a
                      href={construirMailtoRespuesta(lead)}
                      className="mt-4 inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-blue-700"
                    >
                      <FaReply /> Responder por correo
                    </a>
                  )}
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
