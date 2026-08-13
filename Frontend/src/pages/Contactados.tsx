import { Link, useNavigate } from "react-router-dom";
import { FaEnvelope, FaWhatsapp, FaComments, FaCar } from "react-icons/fa";
import { formatearFecha } from "../utils/fecha";
import { useMisContactos } from "../hooks/useLeads";
import { urlImagen } from "../utils/imagen";

const IMAGEN_VACIA =
  "https://via.placeholder.com/600x400?text=Sin+Foto";

type Tema = "oscuro" | "claro";

interface Props {
  tema?: Tema;
  rutaVolver?: string;
}

export default function Contactados({
  tema = "oscuro",
  rutaVolver = "/perfil",
}: Props) {
  const navigate = useNavigate();

  const { data: contactos = [], isLoading: cargando, isError, refetch } = useMisContactos();

  const mensajeError = isError ? "No se pudo cargar tus contactos." : "";

  const claro = tema === "claro";

  const c = {
    envoltorio: claro ? "" : "min-h-screen bg-[#0c101b] text-white",
    headerBar: claro ? "" : "border-b border-white/10",
    enlaceVolver: claro
      ? "text-sm font-medium text-gray-500 transition-colors hover:text-gray-900"
      : "text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white",
    titulo: claro ? "text-2xl font-bold text-gray-900" : "text-2xl font-bold",
    subtitulo: claro ? "mt-1 text-sm text-gray-500" : "mt-1 text-sm text-[#9aa1b1]",
    card: claro
      ? "group flex flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white text-left transition-all hover:border-blue-500/60 hover:shadow-md"
      : "group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] text-left transition-all hover:border-blue-500/40 hover:shadow-lg",
    anioBadge: claro
      ? "shrink-0 rounded-full bg-green-100 px-3 py-1 text-xs font-semibold text-green-700"
      : "shrink-0 rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400",
    lineaVendedor: claro ? "text-xs text-gray-500" : "text-xs text-[#9aa1b1]",
    mensaje: claro
      ? "mt-3 line-clamp-3 flex-1 text-sm leading-relaxed text-gray-600"
      : "mt-3 line-clamp-3 flex-1 text-sm leading-relaxed text-[#c3c9d4]",
    pie: claro
      ? "mt-4 flex items-center gap-3 border-t border-gray-100 pt-4 text-xs text-gray-500"
      : "mt-4 flex items-center gap-3 border-t border-white/10 pt-4 text-xs text-[#9aa1b1]",
    verVehiculo: claro ? "text-blue-600 group-hover:text-blue-500" : "text-blue-400 group-hover:text-blue-300",
    vacioCard: claro
      ? "rounded-2xl border border-gray-200 bg-white p-16 text-center"
      : "rounded-2xl border border-white/10 bg-[#13161d] p-16 text-center",
    vacioTitulo: claro ? "mt-4 text-lg font-semibold text-gray-900" : "mt-4 text-lg font-semibold",
    vacioTexto: claro ? "mt-2 text-sm text-gray-500" : "mt-2 text-sm text-[#9aa1b1]",
  };

  const iconoCanal = (canal: string) => {
    if (claro) {
      switch (canal) {
        case "WhatsApp":
          return <FaWhatsapp className="text-green-600" />;
        case "Formulario":
          return <FaEnvelope className="text-blue-600" />;
        default:
          return <FaComments className="text-gray-400" />;
      }
    }

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
    <div className={c.envoltorio}>
      {!claro && (
        <header
          className={`flex items-center justify-between px-6 py-5 sm:px-8 ${c.headerBar}`}
        >
          <Link to={rutaVolver} className={c.enlaceVolver}>
            ← Mi cuenta
          </Link>
        </header>
      )}

      <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
        <div className="mb-8">
          <h1 className={c.titulo}>
            <span className="mr-3 inline-flex items-center gap-3">
              <FaEnvelope className={claro ? "text-blue-600" : "text-blue-400"} />
              Vehículos que contactaste
            </span>
            {contactos.length > 0 && (
              <span className="text-sm font-normal text-[#9aa1b1]">
                ({contactos.length})
              </span>
            )}
          </h1>
          <p className={c.subtitulo}>
            Todos los vendedores o agencias a los que les escribiste por correo
            o WhatsApp, con el mensaje que enviaste.
          </p>
        </div>

        {cargando ? (
          <div className="flex items-center justify-center py-24">
            <div
              className={`h-10 w-10 animate-spin rounded-full border-2 border-t-blue-500 ${
                claro ? "border-gray-200" : "border-white/10"
              }`}
            />
          </div>
        ) : mensajeError ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
            <p className="text-red-400">{mensajeError}</p>
            <button
              type="button"
              onClick={() => refetch()}
              className="mt-4 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold text-white transition-colors hover:bg-blue-600"
            >
              Reintentar
            </button>
          </div>
        ) : contactos.length === 0 ? (
          <div className={c.vacioCard}>
            <FaEnvelope className="mx-auto text-5xl text-gray-600" />
            <h2 className={c.vacioTitulo}>
              Aún no has contactado ningún vehículo
            </h2>
            <p className={c.vacioTexto}>
              Cuando escribas a un vendedor desde un anuncio, aparecerá aquí.
            </p>
            <button
              type="button"
              onClick={() => navigate("/")}
              className="mt-6 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold text-white transition-colors hover:bg-blue-600"
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
                className={c.card}
              >
                <div className="relative aspect-[16/10] overflow-hidden">
                  <img
                    src={urlImagen(contacto.fotoPrincipal) || IMAGEN_VACIA}
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
                    <h3
                      className={`truncate text-lg font-bold ${claro ? "text-gray-900" : ""}`}
                    >
                      {contacto.marca} {contacto.modelo}
                    </h3>
                    <span className={c.anioBadge}>
                      {contacto.anio}
                    </span>
                  </div>

                  <p className={`mt-1 flex items-center gap-1.5 ${c.lineaVendedor}`}>
                    <FaCar className={claro ? "text-gray-400" : "text-gray-500"} />
                    {contacto.nombreVendedor ||
                      (contacto.esVendedorParticular
                        ? "Vendedor particular"
                        : "Agencia")}
                  </p>

                  <p className={`line-clamp-3 ${c.mensaje}`}>
                    “{contacto.mensaje}”
                  </p>

                  <div className={c.pie}>
                    <span className={`inline-flex items-center gap-1.5 ${c.verVehiculo}`}>
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