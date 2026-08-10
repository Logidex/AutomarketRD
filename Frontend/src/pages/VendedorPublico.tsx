import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  FaArrowLeft,
  FaCalendarAlt,
  FaCar,
  FaClock,
  FaMapMarkerAlt,
  FaPhone,
  FaStore,
  FaTachometerAlt,
  FaUser,
  FaWhatsapp,
} from "react-icons/fa";
import { catalogoService } from "../services/catalogo.service";
import { dealerService, type PerfilDealerPublico } from "../services/dealer.service";
import type { AnuncioListado } from "../types/anuncio.types";
import Spinner from "../components/Spinner";

const CANTIDAD_ANUNCIOS = 50;

export default function VendedorPublico() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const vendedorId = Number(id);
  const esIdInvalido = !Number.isInteger(vendedorId) || vendedorId <= 0;

  const [perfil, setPerfil] = useState<PerfilDealerPublico | null>(null);
  const [anuncios, setAnuncios] = useState<AnuncioListado[]>([]);
  const [cargando, setCargando] = useState(true);
  const [noEncontrado, setNoEncontrado] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (esIdInvalido) return;

    let activo = true;

    const cargar = async () => {
      try {
        const [datosPerfil, resultado] = await Promise.all([
          dealerService.obtenerPerfilPublico(vendedorId),
          catalogoService.buscar({
            vendedorId,
            paginaActual: 1,
            cantidadAnuncios: CANTIDAD_ANUNCIOS,
          }),
        ]);

        if (!activo) return;

        const items = resultado.items.map((a) => ({
          ...a,
          precio: Number(a.precio ?? 0),
          kilometraje: Number(a.kilometraje ?? 0),
        }));

        setPerfil(datosPerfil);
        setAnuncios(items);
      } catch (err) {
        if (!activo) return;

        if (err instanceof Error && err.message.includes("404")) {
          setNoEncontrado(true);
        } else {
          setError(
            err instanceof Error
              ? err.message
              : "No se pudo cargar el perfil del vendedor."
          );
        }
      } finally {
        if (activo) setCargando(false);
      }
    };

    cargar();

    return () => {
      activo = false;
    };
  }, [vendedorId, esIdInvalido]);

  if (cargando && !esIdInvalido) {
    return <Spinner />;
  }

  const esParticular = perfil?.esVendedorParticular === true;
  const inicial = (perfil?.nombreAgencia ?? "V").trim().charAt(0).toUpperCase() || "V";
  const numeroWhatsApp = perfil?.whatsApp
    ? perfil.whatsApp.replace(/[^\d]/g, "")
    : "";

  const fotoPrincipal = (anuncio: AnuncioListado): string =>
    anuncio.fotos && anuncio.fotos.length > 0
      ? anuncio.fotos[0]
      : "https://via.placeholder.com/600x400?text=Sin+Foto";

  const renderAnuncioCard = (
    anuncio: AnuncioListado,
    destacada?: boolean
  ) => (
    <button
      key={anuncio.id}
      type="button"
      onClick={() => navigate(`/anuncio/${anuncio.id}`)}
      className={`group flex flex-col overflow-hidden rounded-2xl border border-white/10 bg-[#13161d] text-left transition-all hover:border-blue-500/40 hover:shadow-lg ${
        destacada ? "border-violet-500/40" : ""
      }`}
    >
      <div className="relative aspect-[16/10] overflow-hidden">
        <img
          src={fotoPrincipal(anuncio)}
          alt={`${anuncio.marca} ${anuncio.modelo}`}
          className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
      </div>

      <div className="flex flex-1 flex-col p-5">
        <div className="flex items-start justify-between gap-3">
          <h3 className="truncate text-lg font-bold">
            {anuncio.marca} {anuncio.modelo}
            <span className="ml-2 text-sm font-normal text-gray-400">
              {anuncio.version}
            </span>
          </h3>
          <span className="shrink-0 rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
            {anuncio.anio}
          </span>
        </div>

        <p className="mt-2 text-xl font-bold text-blue-500">
          RD$ {anuncio.precio.toLocaleString("es-DO")}
        </p>

        <div className="mt-4 flex flex-wrap gap-x-4 gap-y-2 border-t border-white/10 pt-4 text-xs text-[#9aa1b1]">
          <span className="inline-flex items-center gap-1.5">
            <FaTachometerAlt className="text-gray-500" />
            {anuncio.kilometraje.toLocaleString("es-DO")} km
          </span>
          <span className="inline-flex items-center gap-1.5">
            <FaCalendarAlt className="text-gray-500" />
            {anuncio.anio}
          </span>
          {anuncio.ubicacion && (
            <span className="inline-flex items-center gap-1.5">
              <FaMapMarkerAlt className="text-gray-500" />
              {anuncio.ubicacion}
            </span>
          )}
        </div>
      </div>
    </button>
  );

  const renderBotonesContacto = () => (
    <div className="flex flex-col gap-3 sm:items-end">
      {perfil?.telefonoAgencia && (
        <a
          href={`tel:${perfil.telefonoAgencia.replace(/[^\d]/g, "")}`}
          className="inline-flex items-center justify-center gap-2 rounded-lg border border-white/10 px-5 py-2.5 text-sm font-semibold text-gray-200 transition-colors hover:border-white/30"
        >
          <FaPhone className="text-blue-400" />
          {perfil.telefonoAgencia}
        </a>
      )}
      {numeroWhatsApp && (
        <a
          href={`https://wa.me/${numeroWhatsApp}`}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-green-500 px-5 py-2.5 text-sm font-semibold transition-colors hover:bg-green-600"
        >
          <FaWhatsapp className="text-lg" />
          Escribir por WhatsApp
        </a>
      )}
    </div>
  );

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white"
        >
          <FaArrowLeft />
          Volver al inicio
        </Link>
      </header>

      {noEncontrado || esIdInvalido ? (
        <main className="mx-auto max-w-2xl px-6 py-20 text-center sm:px-8">
          <FaStore className="mx-auto text-5xl text-gray-600" />
          <h1 className="mt-4 text-2xl font-bold">Vendedor no encontrado</h1>
          <p className="mt-2 text-sm text-[#9aa1b1]">
            El perfil que buscas no existe o ya no está disponible.
          </p>
          <Link
            to="/"
            className="mt-6 inline-block rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-blue-600"
          >
            Explorar vehículos
          </Link>
        </main>
      ) : (
        <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
          {error ? (
            <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
              <p className="text-red-400">{error}</p>
            </div>
          ) : (
            <>
              {/* BLOQUE DEL PERFIL */}
              {esParticular ? (
                /* VENDEDOR PARTICULAR: perfil personal, un solo vehículo */
                <section className="rounded-2xl border border-violet-500/30 bg-gradient-to-br from-[#1b1630] to-[#0c101b] p-6 sm:p-8">
                  <span className="inline-flex items-center gap-2 rounded-full bg-violet-500/15 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-violet-300">
                    <FaUser className="text-sm" />
                    Vendedor particular
                  </span>

                  <div className="mt-5 flex flex-col gap-6 sm:flex-row sm:items-center">
                    <div className="flex h-20 w-20 shrink-0 items-center justify-center rounded-full bg-violet-500/20 text-3xl font-bold text-violet-300">
                      {inicial}
                    </div>

                    <div className="min-w-0 flex-1">
                      <h1 className="truncate text-2xl font-bold sm:text-3xl">
                        {perfil?.nombreAgencia}
                      </h1>
                      <p className="mt-1 text-sm text-[#9aa1b1]">
                        Vende de forma particular en AutoMarket RD.
                      </p>
                    </div>

                    {renderBotonesContacto()}
                  </div>

                  <div className="mt-6 flex items-start gap-3 rounded-xl border border-violet-500/20 bg-violet-500/5 p-4 text-sm text-[#c3c9d4]">
                    <FaCar className="mt-0.5 shrink-0 text-lg text-violet-300" />
                    <p>
                      Este vendedor es un <strong>particular</strong>: publica por
                      cuenta propia y actualmente solo tiene{" "}
                      <strong>
                        {anuncios.length === 1
                          ? "un vehículo en venta"
                          : `${anuncios.length} vehículos en venta`}
                      </strong>
                      .
                    </p>
                  </div>
                </section>
              ) : (
                /* DEALER: perfil de agencia */
                <section className="rounded-2xl border border-white/10 bg-gradient-to-br from-[#13161d] to-[#0c101b] p-6 sm:p-8">
                  <div className="flex flex-col gap-6 sm:flex-row sm:items-center">
                    <div className="flex h-20 w-20 shrink-0 items-center justify-center overflow-hidden rounded-2xl bg-white/5">
                      {perfil?.logoUrl ? (
                        <img
                          src={perfil.logoUrl}
                          alt={perfil.nombreAgencia}
                          className="h-full w-full object-cover"
                        />
                      ) : (
                        <FaStore className="text-3xl text-blue-400" />
                      )}
                    </div>

                    <div className="min-w-0 flex-1">
                      <h1 className="truncate text-2xl font-bold sm:text-3xl">
                        {perfil?.nombreAgencia}
                      </h1>
                      {perfil?.ubicacion && (
                        <p className="mt-1 flex items-center gap-2 text-sm text-[#9aa1b1]">
                          <FaMapMarkerAlt className="text-gray-500" />
                          {perfil.ubicacion}
                        </p>
                      )}
                      {perfil?.horarios && (
                        <p className="mt-1 flex items-center gap-2 text-sm text-[#9aa1b1]">
                          <FaClock className="text-gray-500" />
                          {perfil.horarios}
                        </p>
                      )}
                    </div>

                    {renderBotonesContacto()}
                  </div>

                  {perfil?.descripcion && (
                    <p className="mt-6 whitespace-pre-line border-t border-white/10 pt-6 text-sm leading-relaxed text-[#c3c9d4]">
                      {perfil.descripcion}
                    </p>
                  )}
                </section>
              )}

              {/* INVENTARIO */}
              <section className="mt-10">
                <h2 className="text-xl font-bold">
                  {esParticular ? "Su vehículo en venta" : "Vehículos en venta"}
                  <span className="ml-2 text-sm font-normal text-[#9aa1b1]">
                    ({anuncios.length})
                  </span>
                </h2>
                {esParticular && (
                  <p className="mt-1 text-sm text-[#9aa1b1]">
                    Los vendedores particulares publican un solo vehículo.
                  </p>
                )}

                {anuncios.length === 0 ? (
                  <div className="mt-6 rounded-2xl border border-white/10 bg-[#13161d] p-16 text-center">
                    <FaCar className="mx-auto text-5xl text-gray-600" />
                    <h3 className="mt-4 text-lg font-semibold">
                      Sin vehículos publicados
                    </h3>
                    <p className="mt-2 text-sm text-[#9aa1b1]">
                      Este vendedor no tiene vehículos disponibles en este momento.
                    </p>
                  </div>
                ) : esParticular ? (
                  /* Un solo vehículo, presentación destacada */
                  <div className="mt-6 mx-auto max-w-2xl">
                    {renderAnuncioCard(anuncios[0], true)}
                  </div>
                ) : (
                  <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {anuncios.map((anuncio) => renderAnuncioCard(anuncio))}
                  </div>
                )}
              </section>
            </>
          )}
        </main>
      )}

      <footer className="border-t border-white/10 py-8">
        <div className="mx-auto px-6 text-center text-sm text-[#9aa1b1] sm:px-8">
          © 2026 AutoMarket RD. Todos los derechos reservados.
        </div>
      </footer>
    </div>
  );
}