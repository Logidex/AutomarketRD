import { Link, useNavigate, useParams } from "react-router-dom";
import { useEffect } from "react";
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
import type { AnuncioListado } from "../types/anuncio.types";
import type { PerfilDealerPublico } from "../services/dealer.service";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";
import { idDesdeSlug, urlAnuncio } from "../utils/slug";
import Spinner from "../components/Spinner";
import BadgeVerificado from "../components/BadgeVerificado";
import { usePerfilDealerPublico, useAnunciosVendedor } from "../hooks/usePerfilDealer";

const fotoPrincipal = (anuncio: AnuncioListado): string =>
  urlImagen(anuncio.fotos?.[0]) || "/sin-foto.svg";

function useVendedorPublico(slug: string | undefined) {
  // El slug es decorativo: el ID viaja al final ("autoventas-rd-5" -> 5).
  // También acepta IDs puros ("/vendedor/5") para links antiguos.
  const vendedorId = idDesdeSlug(slug);
  const esIdInvalido = !Number.isInteger(vendedorId) || vendedorId <= 0;

  const { data: perfil = null, isLoading: cargandoPerfil, isError, error } =
    usePerfilDealerPublico(vendedorId, !esIdInvalido);
  const { data: anuncios = [], isLoading: cargandoAnuncios } = useAnunciosVendedor(
    vendedorId,
    !esIdInvalido,
  );

  const cargando = cargandoPerfil || cargandoAnuncios;

  const noEncontrado = isError && error instanceof Error && error.message.includes("404");
  const errorGeneral = isError && !noEncontrado ? (error instanceof Error ? error.message : "No se pudo cargar el perfil del vendedor.") : "";

  const esParticular = perfil?.esVendedorParticular === true;
  const inicial = (perfil?.nombreAgencia ?? "V").trim().charAt(0).toUpperCase() || "V";
  const numeroWhatsApp = perfil?.whatsApp
    ? perfil.whatsApp.replace(/[^\d]/g, "")
    : "";

  return {
    cargando,
    esIdInvalido,
    noEncontrado,
    errorGeneral,
    esParticular,
    perfil,
    anuncios,
    inicial,
    numeroWhatsApp,
  };
}

interface PropsTarjeta {
  anuncio: AnuncioListado;
  destacada?: boolean;
  onAbrir: () => void;
}

function TarjetaAnuncioVendedor({ anuncio, destacada, onAbrir }: PropsTarjeta) {
  return (
    <button
      type="button"
      onClick={onAbrir}
      className={`group flex flex-col overflow-hidden rounded-2xl border border-line bg-surface text-left transition-all hover:border-blue-500/40 hover:shadow-lg ${
        destacada ? "border-violet-500/40" : ""
      }`}
    >
      <div className="relative aspect-[16/10] overflow-hidden">
        <img
          src={fotoPrincipal(anuncio)}
          alt={`${anuncio.marca} ${anuncio.modelo}`}
          className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
        {anuncio.esDealerVerificado && (
          <BadgeVerificado className="absolute left-3 bottom-3 z-10" />
        )}
      </div>

      <div className="flex flex-1 flex-col p-5">
        <div className="flex items-start justify-between gap-3">
          <h3 className="truncate text-lg font-bold">
            {anuncio.marca} {anuncio.modelo}
            <span className="ml-2 text-sm font-normal text-ink-3">
              {anuncio.version}
            </span>
          </h3>
          <span className="shrink-0 rounded-full bg-success-soft px-3 py-1 text-xs font-semibold text-success">
            {anuncio.anio}
          </span>
        </div>

        <p className="mt-2 text-xl font-bold text-blue-500">
          {formatearPrecio(anuncio.precio, anuncio.moneda)}
        </p>

        <div className="mt-4 flex flex-wrap gap-x-4 gap-y-2 border-t border-line pt-4 text-xs text-ink-2">
          <span className="inline-flex items-center gap-1.5">
            <FaTachometerAlt className="text-ink-3" />
            {anuncio.kilometraje.toLocaleString("es-DO")} km
          </span>
          <span className="inline-flex items-center gap-1.5">
            <FaCalendarAlt className="text-ink-3" />
            {anuncio.anio}
          </span>
          {anuncio.ubicacion && (
            <span className="inline-flex items-center gap-1.5">
              <FaMapMarkerAlt className="text-ink-3" />
              {anuncio.ubicacion}
            </span>
          )}
        </div>
      </div>
    </button>
  );
}

interface PropsContacto {
  telefonoAgencia?: string;
  whatsApp: string;
}

function BotonesContacto({ telefonoAgencia, whatsApp }: PropsContacto) {
  return (
    <div className="flex flex-col gap-3 sm:items-end">
      {telefonoAgencia && (
        <a
          href={`tel:${telefonoAgencia.replace(/[^\d]/g, "")}`}
          className="inline-flex items-center justify-center gap-2 rounded-lg border border-line px-5 py-2.5 text-sm font-semibold text-ink-2 transition-colors hover:border-brand/40 hover:text-ink"
        >
          <FaPhone className="text-brand" />
          {telefonoAgencia}
        </a>
      )}
      {whatsApp && (
        <a
          href={`https://wa.me/${whatsApp}`}
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
}

interface PropsPerfilParticular {
  perfil: PerfilDealerPublico;
  anuncios: AnuncioListado[];
  inicial: string;
  numeroWhatsApp: string;
}

function PerfilParticular({ perfil, anuncios, inicial, numeroWhatsApp }: PropsPerfilParticular) {
  return (
    <section className="rounded-2xl border border-violet-500/30 bg-surface p-6 sm:p-8">
      <span className="inline-flex items-center gap-2 rounded-full bg-violet-500/10 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-violet-600 dark:text-violet-300">
        <FaUser className="text-sm" />
        Vendedor particular
      </span>

      <div className="mt-5 flex flex-col gap-6 sm:flex-row sm:items-center">
        <div className="flex h-20 w-20 shrink-0 items-center justify-center rounded-full bg-violet-500/10 text-3xl font-bold text-violet-600 dark:text-violet-300">
          {inicial}
        </div>

        <div className="min-w-0 flex-1">
          <h1 className="truncate text-2xl font-bold sm:text-3xl">
            {perfil.nombreAgencia}
          </h1>
          <p className="mt-1 text-sm text-ink-2">
            Vende de forma particular en AutoMarket RD.
          </p>
          {perfil.esDealerVerificado && (
            <BadgeVerificado className="mt-2" />
          )}
        </div>

        <BotonesContacto
          telefonoAgencia={perfil.telefonoAgencia}
          whatsApp={numeroWhatsApp}
        />
      </div>

      <div className="mt-6 flex items-start gap-3 rounded-xl border border-violet-500/20 bg-violet-500/5 p-4 text-sm text-ink-2">
        <FaCar className="mt-0.5 shrink-0 text-lg text-violet-500" />
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
  );
}

interface PropsPerfilDealer {
  perfil: PerfilDealerPublico;
  anuncios: AnuncioListado[];
  numeroWhatsApp: string;
}

function PerfilDealer({ perfil, anuncios, numeroWhatsApp }: PropsPerfilDealer) {
  return (
    <section className="relative overflow-hidden rounded-2xl border border-blue-500/30 bg-surface p-6 shadow-sm sm:p-8">
      <div className="pointer-events-none absolute -right-20 -top-20 h-60 w-60 rounded-full bg-blue-500/10 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-24 -left-16 h-52 w-52 rounded-full bg-blue-500/5 blur-3xl" />

      <div className="relative">
        <span className="inline-flex items-center gap-2 rounded-full bg-brand-soft px-3 py-1 text-xs font-semibold uppercase tracking-wide text-brand">
          <FaStore className="text-sm" />
          Agencia de vehículos
        </span>

        <div className="mt-6 flex flex-col gap-6 sm:flex-row sm:items-center">
          <div className="flex h-24 w-24 shrink-0 items-center justify-center overflow-hidden rounded-2xl border-2 border-blue-500/30 bg-surface-2 shadow-lg shadow-blue-500/10">
            {perfil.logoUrl ? (
              <img
                src={urlImagen(perfil.logoUrl)}
                alt={perfil.nombreAgencia}
                className="h-full w-full object-cover"
              />
            ) : (
              <FaStore className="text-4xl text-brand" />
            )}
          </div>

          <div className="min-w-0 flex-1">
            <h1 className="truncate text-2xl font-extrabold sm:text-3xl">
              {perfil.nombreAgencia}
            </h1>
            <p className="mt-1 text-sm text-ink-2">
              Agencia de venta de vehículos en AutoMarket RD.
            </p>

            {perfil.esDealerVerificado && (
              <BadgeVerificado className="mt-2" />
            )}

            <div className="mt-3 flex flex-wrap gap-2">
              {perfil.ubicacion && (
                <span className="inline-flex items-center gap-1.5 rounded-full border border-line bg-surface-2 px-3 py-1 text-xs text-ink-2">
                  <FaMapMarkerAlt className="text-brand" />
                  {perfil.ubicacion}
                </span>
              )}
              {perfil.horarios && (
                <span className="inline-flex items-center gap-1.5 rounded-full border border-line bg-surface-2 px-3 py-1 text-xs text-ink-2">
                  <FaClock className="text-brand" />
                  {perfil.horarios}
                </span>
              )}
            </div>
          </div>

          <BotonesContacto
            telefonoAgencia={perfil.telefonoAgencia}
            whatsApp={numeroWhatsApp}
          />
        </div>

        <div className="mt-6 grid grid-cols-1 gap-3 rounded-xl border border-brand/20 bg-brand-soft p-4 sm:grid-cols-3">
          <div>
            <p className="text-[11px] uppercase tracking-wide text-ink-2">
              Vehículos publicados
            </p>
            <p className="mt-1 text-xl font-bold text-brand">
              {anuncios.length}
            </p>
          </div>
          <div>
            <p className="text-[11px] uppercase tracking-wide text-ink-2">
              Ubicación
            </p>
            <p className="mt-1 truncate text-sm font-semibold text-ink">
              {perfil.ubicacion || "—"}
            </p>
          </div>
          <div>
            <p className="text-[11px] uppercase tracking-wide text-ink-2">
              Horarios
            </p>
            <p className="mt-1 truncate text-sm font-semibold text-ink">
              {perfil.horarios || "—"}
            </p>
          </div>
        </div>

        {perfil.descripcion && (
          <div className="mt-6 rounded-xl border border-line bg-surface-2 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-ink-2">
              Sobre la agencia
            </p>
            <p className="mt-2 whitespace-pre-line text-sm leading-relaxed text-ink-3">
              {perfil.descripcion}
            </p>
          </div>
        )}
      </div>
    </section>
  );
}

interface PropsInventario {
  anuncios: AnuncioListado[];
  esParticular: boolean;
  onAbrir: (anuncio: AnuncioListado) => void;
}

function InventarioVendedor({ anuncios, esParticular, onAbrir }: PropsInventario) {
  return (
    <section className="mt-10">
      <div className="flex items-center gap-3">
        {!esParticular && (
          <span className="h-6 w-1 rounded-full bg-gradient-to-b from-blue-400 to-blue-600" />
        )}
        <h2 className="text-xl font-bold">
          {esParticular ? "Su vehículo en venta" : "Inventario de la agencia"}
          <span className="ml-2 text-sm font-normal text-ink-2">
            ({anuncios.length})
          </span>
        </h2>
      </div>
      <p className="mt-1 text-sm text-ink-2">
        {esParticular
          ? "Los vendedores particulares publican un solo vehículo."
          : "Todos los vehículos disponibles de esta agencia."}
      </p>

      {anuncios.length === 0 ? (
        <div className="mt-6 rounded-2xl border border-line bg-surface p-16 text-center">
          <FaCar className="mx-auto text-5xl text-ink-3" />
          <h3 className="mt-4 text-lg font-semibold">
            Sin vehículos publicados
          </h3>
          <p className="mt-2 text-sm text-ink-2">
            Este vendedor no tiene vehículos disponibles en este momento.
          </p>
        </div>
      ) : esParticular ? (
        <div className="mt-6 mx-auto max-w-2xl">
          <TarjetaAnuncioVendedor
            anuncio={anuncios[0]}
            destacada
            onAbrir={() => onAbrir(anuncios[0])}
          />
        </div>
      ) : (
        <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {anuncios.map((anuncio) => (
            <TarjetaAnuncioVendedor
              key={anuncio.id}
              anuncio={anuncio}
              onAbrir={() => onAbrir(anuncio)}
            />
          ))}
        </div>
      )}
    </section>
  );
}

function NoEncontradoVendedor() {
  return (
    <main className="mx-auto max-w-2xl px-6 py-20 text-center sm:px-8">
      <FaStore className="mx-auto text-5xl text-ink-3" />
      <h1 className="mt-4 text-2xl font-bold">Vendedor no encontrado</h1>
      <p className="mt-2 text-sm text-ink-2">
        El perfil que buscas no existe o ya no está disponible.
      </p>
      <Link
        to="/"
        className="mt-6 inline-block rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-blue-600"
      >
        Explorar vehículos
      </Link>
    </main>
  );
}

export default function VendedorPublico() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();

  const {
    cargando,
    esIdInvalido,
    noEncontrado,
    errorGeneral,
    esParticular,
    perfil,
    anuncios,
    inicial,
    numeroWhatsApp,
  } = useVendedorPublico(slug);

  // SEO: título de la pestaña con el nombre de la agencia/vendedor.
  useEffect(() => {
    if (perfil?.nombreAgencia) {
      document.title = `${perfil.nombreAgencia} | AutoMarket RD`;
    }
    return () => {
      document.title = "AutoMarket RD — Compra y venta de vehículos en República Dominicana";
    };
  }, [perfil?.nombreAgencia]);

  if (cargando && !esIdInvalido) {
    return <Spinner />;
  }

  return (
    <div className="min-h-screen bg-page text-ink">
      <header className="flex items-center justify-between border-b border-line px-6 py-5 sm:px-8">
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-sm font-medium text-ink-2 transition-colors hover:text-ink"
        >
          <FaArrowLeft />
          Volver al inicio
        </Link>
      </header>

      {noEncontrado || esIdInvalido ? (
        <NoEncontradoVendedor />
      ) : (
        <main className="mx-auto max-w-6xl px-6 py-10 sm:px-8">
          {errorGeneral ? (
            <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-8 text-center">
              <p className="text-red-400">{errorGeneral}</p>
            </div>
          ) : (
            <>
              {/* BLOQUE DEL PERFIL */}
              {perfil ? (
                esParticular ? (
                  <PerfilParticular
                    perfil={perfil}
                    anuncios={anuncios}
                    inicial={inicial}
                    numeroWhatsApp={numeroWhatsApp}
                  />
                ) : (
                  <PerfilDealer
                    perfil={perfil}
                    anuncios={anuncios}
                    numeroWhatsApp={numeroWhatsApp}
                  />
                )
              ) : (
                <NoEncontradoVendedor />
              )}

              {/* INVENTARIO */}
              <InventarioVendedor
                anuncios={anuncios}
                esParticular={esParticular}
                onAbrir={(a) => navigate(urlAnuncio(a))}
              />
            </>
          )}
        </main>
      )}

      <footer className="border-t border-line py-8">
        <div className="mx-auto px-6 text-center text-sm text-ink-2 sm:px-8">
          © 2026 AutoMarket RD. Todos los derechos reservados.
        </div>
      </footer>
    </div>
  );
}