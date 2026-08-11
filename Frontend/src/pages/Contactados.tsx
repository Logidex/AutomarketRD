import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { FaEnvelope, FaWhatsapp, FaComments, FaCar } from "react-icons/fa";
import { leadService } from "../services/lead.service";
import type { LeadContactoUsuario } from "../types/lead.types";
import { formatearFecha } from "../utils/fecha";

const IMAGEN_VACIA =
  "https://via.placeholder.com/600x400?text=Sin+Foto";

export default function Contactados() {
  const navigate = useNavigate();

  const [contactos, setContactos] = useState<LeadContactoUsuario[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");

  const cargar = useCallback(async () => {
    try {
      const lista = await leadService.obtenerMisContactos();
      setContactos(lista);
      setError("");
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "No se pudo cargar tus contactos.",
      );
    } finally {
      setCargando(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    cargar();
  }, [cargar]);

  const iconoCanal = (canal: string) => {
    switch (canal) {
      case "WhatsApp":
        return <FaWhatsapp className="text-green-400" />;
      case "Formulario":
        return <FaEnvelope className="text-blue-400" />;
      default:
        return <FaComments className="text-gray-400" />;
    }
  };

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link
          to="/perfil"
          className="text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white"
        >
          ← Mi cuenta
        </Link>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <div className="mb-8">
          <h1 className="flex items-center gap-3 text-2xl font-bold">
            <FaEnvelope className="text-blue-400" />
            Vehículos que contactaste
            {contactos.length > 0 && (
              <span className="text-sm font-normal text-[#9aa1b1]">
                ({contactos.length})
              </span>
            )}
          </h1>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            Todos los vendedores o agencias a los que les escribiste por correo
            o WhatsApp, con el mensaje que enviaste.
          </p>
        </div>

        {cargando ? (
          <div className="flex items-center justify-center py-24">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
          </div>
        ) : error ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-400">{error}</p>
            <button
              type="button"
              onClick={() => {
                setCargando(true);
                cargar();
              }}
              className="mt-4 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
            >
              Reintentar
            </button>
          </div>
        ) : contactos.length === 0 ? (
          <div className="rounded-2xl border border-white/10 bg-[#13161d] p-16 text-center">
            <FaEnvelope className="mx-auto text-5xl text-gray-600" />
            <h2 className="mt-4 text-lg font-semibold">
              Aún no has contactado ningún vehículo
            </h2>
            <p className="mt-2 text-sm text-[#9aa1b1]">
              Cuando escribas a un vendedor desde un anuncio, aparecerá aquí.
            </p>
            <button
              type="button"
              onClick={() => navigate("/")}
              className="mt-6 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
            >
              Explorar vehículos
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {contactos.map((contacto) => (
              <button
                key={contacto.id}
                type="button"
                onClick={() => navigate(`/anuncio/${contacto.anuncioId}`)}
                className="group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] text-left transition-all hover:border-blue-500/40 hover:shadow-lg"
              >
                <div className="relative aspect-[16/10] overflow-hidden">
                  <img
                    src={contacto.fotoPrincipal || IMAGEN_VACIA}
                    alt={`${contacto.marca} ${contacto.modelo}`}
                    className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                    onError={(e) => {
                      const img = e.currentTarget;
                      if (img.src !== IMAGEN_VACIA) img.src = IMAGEN_VACIA;
                    }}
                  />
                  <span className="absolute left-3 top-3 inline-flex items-center gap-1.5 rounded-full bg-black/60 px-2.5 py-1 text-xs font-medium text-white backdrop-blur">
                    {iconoCanal(contacto.canal)}
                    {contacto.canal}
                  </span>
                  <span className="absolute bottom-3 left-3 rounded-full bg-black/60 px-2.5 py-1 text-xs font-medium text-white backdrop-blur">
                    {formatearFecha(contacto.fechaCreacionUtc, true)}
                  </span>
                </div>

                <div className="flex flex-1 flex-col p-5">
                  <div className="flex items-start justify-between gap-3">
                    <h3 className="truncate text-lg font-bold">
                      {contacto.marca} {contacto.modelo}
                    </h3>
                    <span className="shrink-0 rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
                      {contacto.anio}
                    </span>
                  </div>

                  <p className="mt-1 flex items-center gap-1.5 text-xs text-[#9aa1b1]">
                    <FaCar className="text-gray-500" />
                    {contacto.nombreVendedor ||
                      (contacto.esVendedorParticular
                        ? "Vendedor particular"
                        : "Agencia")}
                  </p>

                  <p className="mt-3 line-clamp-3 flex-1 text-sm leading-relaxed text-[#c3c9d4]">
                    “{contacto.mensaje}”
                  </p>

                  <div className="mt-4 flex items-center gap-3 border-t border-white/10 pt-4 text-xs text-[#9aa1b1]">
                    <span className="inline-flex items-center gap-1.5 text-blue-400 group-hover:text-blue-300">
                      Ver vehículo →
                    </span>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
