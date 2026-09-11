import { useEffect, useState, useRef, useCallback } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { AnimatePresence, motion } from "motion/react";
import { idDesdeSlug, urlVendedor } from "../utils/slug";
import Swal from "sweetalert2";
import {
  FaArrowLeft,
  FaBalanceScale,
  FaCalendarAlt,
  FaCar,
  FaCheckCircle,
  FaEnvelope,
FaFlag,
  FaHeart,
  FaMapMarkerAlt,
  FaPaperPlane,
  FaRegHeart,
  FaStore,
  FaTachometerAlt,
  FaUsers,
  FaWhatsapp,
} from "react-icons/fa";
import { authService } from "../services/auth.service";
import type { AnuncioDetalle } from "../types/anuncio.types";
import type { UsuarioAuth } from "../types/auth.types";
import { useComparador } from "../context/ComparadorContext";
import { useVehiculoDetalle } from "../hooks/useVehiculos";
import { useCrearLead } from "../hooks/useLeads";
import {
  useMisFavoritos,
  useAgregarFavorito,
  useQuitarFavorito,
} from "../hooks/useFavoritos";
import { useRegistrarVisita } from "../hooks/useHistorial";
import { useUsuarioCuenta } from "../hooks/useUsuario";
import { urlImagen } from "../utils/imagen";
import { formatearPrecio } from "../utils/formato";
import HeaderPublico from "../components/layout/HeaderPublico";
import SectionBackground from "../components/SectionBackground";
import BadgeVerificado from "../components/BadgeVerificado";
import AdSlotRenderer from "../components/ads/AdSlotRenderer";
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from "../constants/vehiculo.opciones";

const IMAGEN_VACIA = "/sin-foto.svg";

interface PropsFotos {
  anuncio: AnuncioDetalle;
  fotoActiva: number;
  fotoAmpliada: boolean;
  fotoPrincipal: string;
  onAmpliar: () => void;
  onPausar: () => void;
  onReanudar: () => void;
  onAnterior: () => void;
  onSiguiente: () => void;
  onSeleccionarMiniatura: (indice: number) => void;
}

function GaleriaFotos({
  anuncio,
  fotoActiva,
  fotoAmpliada,
  fotoPrincipal,
  onAmpliar,
  onPausar,
  onReanudar,
  onAnterior,
  onSiguiente,
  onSeleccionarMiniatura,
}: PropsFotos) {
  const hayVariasFotos = anuncio.fotos.length > 1;
  const touchStartX = useRef(0);

  const handleTouchStart = useCallback((e: React.TouchEvent) => {
    touchStartX.current = e.touches[0].clientX;
  }, []);

  const handleTouchEnd = useCallback(
    (e: React.TouchEvent) => {
      const dx = e.changedTouches[0].clientX - touchStartX.current;
      if (Math.abs(dx) > 50) {
        if (dx < 0) onSiguiente();
        else onAnterior();
      }
    },
    [onAnterior, onSiguiente],
  );

  return (
    <div className="overflow-hidden rounded-2xl border border-line bg-surface">
      <div
        className="group relative aspect-[16/10]"
        onMouseEnter={onPausar}
        onMouseLeave={onReanudar}
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
      >
        <button
          type="button"
          aria-label="Ampliar foto"
          onClick={onAmpliar}
          className="absolute right-3 top-3 z-10 flex h-10 w-10 items-center justify-center rounded-full bg-black/50 text-white opacity-0 backdrop-blur transition-opacity group-hover:opacity-100"
        >
          ⤢
        </button>
        <img
          src={fotoPrincipal}
          alt={`${anuncio.marca} ${anuncio.modelo} ${anuncio.anio}`}
          className={`h-full w-full object-cover transition-opacity duration-300 ${
            fotoAmpliada ? "opacity-0" : ""
          }`}
          onError={(e) => {
            const img = e.currentTarget;
            if (img.src !== IMAGEN_VACIA) img.src = IMAGEN_VACIA;
          }}
        />

        {!fotoAmpliada && hayVariasFotos && (
          <div className="absolute left-0 right-0 top-1/2 flex -translate-y-1/2 items-center justify-between px-3 opacity-0 transition-opacity group-hover:opacity-100">
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                onAnterior();
              }}
              aria-label="Foto anterior"
              className="flex h-10 w-10 items-center justify-center rounded-full bg-black/60 text-lg text-white backdrop-blur transition-colors hover:bg-black/80"
            >
              ‹
            </button>
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                onSiguiente();
              }}
              aria-label="Siguiente foto"
              className="flex h-10 w-10 items-center justify-center rounded-full bg-black/60 text-lg text-white backdrop-blur transition-colors hover:bg-black/80"
            >
              ›
            </button>
          </div>
        )}

        {hayVariasFotos && (
          <div className="absolute bottom-3 left-0 right-0 flex justify-center gap-1.5">
            {anuncio.fotos.map((foto, i) => (
              <span
                key={foto}
                className={`h-1.5 rounded-full transition-all ${
                  i === fotoActiva ? "w-5 bg-blue-500" : "w-1.5 bg-white/40"
                }`}
              />
            ))}
          </div>
        )}
      </div>

      {hayVariasFotos && (
        <div className="flex flex-wrap gap-2 border-t border-line p-3">
          {anuncio.fotos.map((foto, i) => (
            <button
              key={foto}
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                onSeleccionarMiniatura(i);
              }}
              onMouseEnter={onPausar}
              onMouseLeave={onReanudar}
              className={`h-16 w-24 overflow-hidden rounded-lg border transition-colors ${
                i === fotoActiva
                  ? "border-blue-500"
                  : "border-line hover:border-ink-3"
              }`}
            >
              <img
                src={urlImagen(foto)}
                alt={`Foto ${i + 1} de ${anuncio.marca} ${anuncio.modelo}`}
                className="h-full w-full object-cover"
                onError={(e) => {
                  e.currentTarget.style.display = "none";
                }}
              />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

function DescripcionYEquipamiento({ anuncio }: { anuncio: AnuncioDetalle }) {
  return (
    <>
      {anuncio.descripcion && (
        <section className="mt-8 rounded-2xl border border-line bg-surface p-6">
          <h2 className="text-lg font-bold">Descripción</h2>
          <p className="mt-3 whitespace-pre-line text-sm leading-relaxed text-ink-3">
            {anuncio.descripcion}
          </p>
        </section>
      )}

      {anuncio.accesorios && anuncio.accesorios.length > 0 && (
        <section className="mt-8 rounded-2xl border border-line bg-surface p-6">
          <h2 className="text-lg font-bold">Equipamiento</h2>
          <div className="mt-4 flex flex-wrap gap-2">
            {anuncio.accesorios.map((accesorio) => (
              <span
                key={accesorio}
                className="rounded-full border border-line bg-surface-2 px-3 py-1.5 text-xs font-medium text-ink-2"
              >
                {accesorio}
              </span>
            ))}
          </div>
        </section>
      )}
    </>
  );
}

interface PropsTarjeta {
  anuncio: AnuncioDetalle;
  esVehiculoNuevo: boolean;
  esOferta: boolean;
  esPropietario: boolean;
  esFavorito: boolean;
  cargandoFavorito: boolean;
  onToggleFavorito: () => void;
  esSeleccionado: (id: number) => boolean;
  onToggleComparar: (id: number) => void;
}

function TarjetaDatos({
  anuncio,
  esVehiculoNuevo,
  esOferta,
  esPropietario,
  esFavorito,
  cargandoFavorito,
  onToggleFavorito,
  esSeleccionado,
  onToggleComparar,
}: PropsTarjeta) {
  return (
    <div className="rounded-2xl border border-line bg-surface p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">
            {anuncio.marca} {anuncio.modelo}
          </h1>
          <p className="mt-1 text-sm text-ink-2">
            {anuncio.version || "—"}
          </p>
        </div>
        {esVehiculoNuevo ? (
          <span className="rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
            Nuevo
          </span>
        ) : (
          <span className="rounded-full bg-surface-2 px-3 py-1 text-xs font-medium text-ink-2">
            Usado
          </span>
        )}
      </div>

      <p className="mt-4 text-3xl font-bold text-blue-500">
        {formatearPrecio(anuncio.precio, anuncio.moneda)}
      </p>

      {esOferta && anuncio.precioAnterior != null && (
        <p className="mt-1 text-sm text-ink-2">
          <span className="mr-2 line-through">
            {formatearPrecio(anuncio.precioAnterior, anuncio.moneda)}
          </span>
          <span className="font-semibold text-green-400">Oferta</span>
        </p>
      )}

      {!esPropietario && (
        <button
          type="button"
          onClick={onToggleFavorito}
          disabled={cargandoFavorito}
          aria-pressed={esFavorito}
          className={`mt-4 flex w-full items-center justify-center gap-2 rounded-lg border px-5 py-2.5 text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${
            esFavorito
              ? "border-red-500/40 bg-red-500/10 text-red-400 hover:bg-red-500/20"
              : "border-line bg-surface-2 text-ink-2 hover:border-red-500/40 hover:text-red-400"
          }`}
        >
          {esFavorito ? (
            <>
              <FaHeart className="text-red-500" />
              En tus favoritos
            </>
          ) : (
            <>
              <FaRegHeart />
              Guardar en favoritos
            </>
          )}
        </button>
      )}

      {!esPropietario && (
        <button
          type="button"
          onClick={() => onToggleComparar(anuncio.id)}
          aria-pressed={esSeleccionado(anuncio.id)}
          className={`mt-3 flex w-full items-center justify-center gap-2 rounded-lg border px-5 py-2.5 text-sm font-semibold transition-colors ${
            esSeleccionado(anuncio.id)
              ? "border-blue-500/50 bg-blue-500/15 text-blue-400"
              : "border-line bg-surface-2 text-ink-2 hover:border-blue-500/40 hover:text-blue-400"
          }`}
        >
          {esSeleccionado(anuncio.id) ? (
            <>
              <FaCheckCircle className="text-blue-400" />
              En comparación · Quitar
            </>
          ) : (
            <>
              <FaBalanceScale />
              Agregar a comparar
            </>
          )}
        </button>
      )}

      <div className="mt-5 grid grid-cols-2 gap-3 text-sm">
        <div className="flex items-center gap-2 text-ink-3">
          <FaCalendarAlt className="text-ink-3" />
          {anuncio.anio}
        </div>
        <div className="flex items-center gap-2 text-ink-3">
          <FaTachometerAlt className="text-ink-3" />
          {esVehiculoNuevo
            ? "Nuevo"
            : `${anuncio.kilometraje.toLocaleString("es-DO")} km`}
        </div>
        {anuncio.ubicacion && (
          <div className="col-span-2 flex items-center gap-2 text-ink-3">
            <FaMapMarkerAlt className="text-ink-3" />
            {anuncio.ubicacion}
          </div>
        )}
      </div>
    </div>
  );
}

function FichaTecnica({
  anuncio,
  propsMostradas,
}: {
  anuncio: AnuncioDetalle;
  propsMostradas: { etiqueta: string; valor: string }[];
}) {
  return (
    <div className="rounded-2xl border border-line bg-surface p-6">
      <h2 className="text-sm font-bold uppercase tracking-wide text-ink-2">
        Ficha técnica
      </h2>
      <dl className="mt-4 grid grid-cols-2 gap-x-4 gap-y-4 text-sm">
        {propsMostradas.map((fila) => (
          <div key={fila.etiqueta}>
            <dt className="text-xs text-ink-2">{fila.etiqueta}</dt>
            <dd className="mt-0.5 font-medium text-ink">
              {fila.valor}
            </dd>
          </div>
        ))}
        <div>
          <dt className="text-xs text-ink-2">Color exterior</dt>
          <dd className="mt-0.5 font-medium text-ink">
            {anuncio.colorExterior || "—"}
          </dd>
        </div>
        <div>
          <dt className="text-xs text-ink-2">Color interior</dt>
          <dd className="mt-0.5 font-medium text-ink">
            {anuncio.colorInterior || "—"}
          </dd>
        </div>
      </dl>
    </div>
  );
}

interface PropsContacto {
  anuncio: AnuncioDetalle;
  esPropietario: boolean;
  esVendedorParticular: boolean;
  usuario: UsuarioAuth | null;
  nombre: string;
  email: string;
  telefono: string;
  mensaje: string;
  mostrarFormulario: boolean;
  enviando: boolean;
  error: string;
  onChangeNombre: (valor: string) => void;
  onChangeEmail: (valor: string) => void;
  onChangeTelefono: (valor: string) => void;
  onChangeMensaje: (valor: string) => void;
  onAbrirFormulario: () => void;
  onEnviar: (e: React.FormEvent) => void;
  onWhatsApp: () => void;
}

function SeccionContacto({
  anuncio,
  esPropietario,
  esVendedorParticular,
  usuario,
  nombre,
  email,
  telefono,
  mensaje,
  mostrarFormulario,
  enviando,
  error,
  onChangeNombre,
  onChangeEmail,
  onChangeTelefono,
  onChangeMensaje,
  onAbrirFormulario,
  onEnviar,
  onWhatsApp,
}: PropsContacto) {
  return esPropietario ? (
    <div className="rounded-2xl border border-blue-500/30 bg-blue-500/10 p-6 text-center">
      <FaUsers className="mx-auto text-3xl text-blue-400" />
      <p className="mt-3 text-sm text-blue-300">
        Este es tu vehículo. Puedes ver su detalle en el panel.
      </p>
      <Link
        to="/dashboard"
        className="mt-4 inline-block rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
      >
        Ir a mi panel
      </Link>
    </div>
  ) : (
    <div className="rounded-2xl border border-line bg-surface p-6">
      <h2 className="text-lg font-bold">
        {esVendedorParticular ? "Contactar vendedor" : "Contactar agencia"}
      </h2>
      {anuncio.nombreVendedor && (
        <p className="mt-1 flex items-center gap-2 text-sm font-medium text-ink-3">
          {anuncio.nombreVendedor}
          {anuncio.esDealerVerificado && <BadgeVerificado />}
        </p>
      )}

      <div className="mt-2 flex flex-col items-start gap-1.5">
        <span
          className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ${
            esVendedorParticular
              ? "bg-violet-500/15 text-violet-300"
              : "bg-blue-500/15 text-blue-300"
          }`}
        >
          {esVendedorParticular ? "Vendedor particular" : "Dealer (agencia)"}
        </span>

        <Link
          to={urlVendedor(anuncio.usuarioId, anuncio.nombreVendedor ?? "vendedor")}
          className="inline-flex items-center gap-2 text-sm font-semibold text-blue-400 transition-colors hover:text-blue-300"
        >
          <FaStore />
          {esVendedorParticular
            ? "Ver perfil del vendedor"
            : "Ver perfil de la agencia"}
        </Link>
      </div>

      <div className="mt-5 grid grid-cols-1 gap-3">
        {anuncio.whatsAppContacto && (
          <button
            type="button"
            onClick={onWhatsApp}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-green-500 px-6 py-3 text-sm font-semibold transition-colors hover:bg-green-600"
          >
            <FaWhatsapp className="text-lg" />
            Contactar por WhatsApp
          </button>
        )}

        {!mostrarFormulario && (
          <button
            type="button"
            onClick={onAbrirFormulario}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-blue-500 px-6 py-3 text-sm font-semibold transition-colors hover:bg-blue-600"
          >
            <FaEnvelope className="text-lg" />
            Contactar por correo
          </button>
        )}
      </div>

      {mostrarFormulario && (
        <form onSubmit={onEnviar} className="mt-5 space-y-4">
          {usuario ? (
            <div className="rounded-lg border border-line bg-page px-3 py-2.5 text-xs text-ink-2">
              Enviarás este mensaje con los datos de tu cuenta:{" "}
              <span className="font-medium text-ink-3">
                {nombre} · {email}
                {telefono ? ` · ${telefono}` : ""}
              </span>
            </div>
          ) : (
            <>
              <div>
                <label
                  htmlFor="contactoNombre"
                  className="mb-1 block text-xs font-medium text-ink-2"
                >
                  Nombre *
                </label>
                <input
                  id="contactoNombre"
                  type="text"
                  required
                  maxLength={100}
                  value={nombre}
                  onChange={(e) => onChangeNombre(e.target.value)}
                  placeholder="Tu nombre"
                  className="w-full rounded-lg border border-line bg-page px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                />
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label
                    htmlFor="contactoEmail"
                    className="mb-1 block text-xs font-medium text-ink-2"
                  >
                    Email
                  </label>
                  <input
                    id="contactoEmail"
                    type="email"
                    maxLength={150}
                    value={email}
                    onChange={(e) => onChangeEmail(e.target.value)}
                    placeholder="tucorreo@ejemplo.com"
                    className="w-full rounded-lg border border-line bg-page px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label
                    htmlFor="contactoTelefono"
                    className="mb-1 block text-xs font-medium text-ink-2"
                  >
                    Teléfono
                  </label>
                  <input
                    id="contactoTelefono"
                    type="tel"
                    maxLength={20}
                    value={telefono}
                    onChange={(e) => onChangeTelefono(e.target.value)}
                    placeholder="809-000-0000"
                    className="w-full rounded-lg border border-line bg-page px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                  />
                </div>
              </div>
            </>
          )}

          <div>
            <label
              htmlFor="contactoMensaje"
              className="mb-1 block text-xs font-medium text-ink-2"
            >
              Mensaje *
            </label>
            <textarea
              id="contactoMensaje"
              required
              maxLength={1000}
              rows={4}
              value={mensaje}
              onChange={(e) => onChangeMensaje(e.target.value)}
              placeholder="Hola, me interesa este vehículo. ¿Sigue disponible?"
              className="w-full rounded-lg border border-line bg-page px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
            />
          </div>

          {error && (
            <p className="rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2 text-xs text-red-400">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={enviando}
            className="mt-2 flex w-full items-center justify-center gap-2 rounded-lg bg-blue-500 px-6 py-3 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed"
          >
            <FaPaperPlane />
            {enviando ? "Enviando..." : "Enviar mensaje"}
          </button>
        </form>
      )}

      <div className="mt-5 border-t border-line pt-4 text-center text-[10px] text-[#6b7280]">
        {esVendedorParticular
          ? "Al contactar, el vendedor recibirá tus datos para responderte directamente."
          : "Al contactar, la agencia recibirá tus datos para responderte directamente."}
      </div>
    </div>
  );
}

interface PropsLightbox {
  anuncio: AnuncioDetalle;
  fotoActiva: number;
  fotoPrincipal: string;
  onCerrar: () => void;
  onAnterior: () => void;
  onSiguiente: () => void;
}

function GaleriaCompleta({ anuncio }: { anuncio: AnuncioDetalle }) {
  const [fotoActiva, setFotoActiva] = useState(0);
  const [fotoAmpliada, setFotoAmpliada] = useState(false);
  const [enPausa, setEnPausa] = useState(false);

  const fotoPrincipal =
    anuncio.fotos && anuncio.fotos.length > 0
      ? urlImagen(anuncio.fotos[fotoActiva])
      : IMAGEN_VACIA;

  // Carrusel: avanza solo cada 2s mientras haya fotos y el usuario
  // no esté viendo una ampliada ni haya pausado (hover).
  useEffect(() => {
    if (!anuncio.fotos || anuncio.fotos.length <= 1) return;
    if (fotoAmpliada || enPausa) return;

    const intervalo = window.setInterval(() => {
      setFotoActiva((actual) =>
        actual + 1 >= anuncio.fotos!.length ? 0 : actual + 1,
      );
    }, 2000);

    return () => window.clearInterval(intervalo);
  }, [anuncio, fotoAmpliada, enPausa]);

  const siguienteFoto = () => {
    if (!anuncio.fotos || anuncio.fotos.length === 0) return;
    setFotoActiva((actual) =>
      actual + 1 >= anuncio.fotos!.length ? 0 : actual + 1,
    );
  };

  const anteriorFoto = () => {
    if (!anuncio.fotos || anuncio.fotos.length === 0) return;
    setFotoActiva((actual) =>
      actual - 1 < 0 ? anuncio.fotos!.length - 1 : actual - 1,
    );
  };

  const seleccionarMiniatura = (i: number) => {
    setEnPausa(true);
    setFotoActiva(i);
    window.setTimeout(() => setEnPausa(false), 5000);
  };

  const cerrarLightbox = () => {
    setFotoAmpliada(false);
    setEnPausa(false);
  };

  return (
    <>
      <GaleriaFotos
        anuncio={anuncio}
        fotoActiva={fotoActiva}
        fotoAmpliada={fotoAmpliada}
        fotoPrincipal={fotoPrincipal}
        onAmpliar={() => setFotoAmpliada(true)}
        onPausar={() => setEnPausa(true)}
        onReanudar={() => setEnPausa(false)}
        onAnterior={anteriorFoto}
        onSiguiente={siguienteFoto}
        onSeleccionarMiniatura={seleccionarMiniatura}
      />

      <AnimatePresence>
        {fotoAmpliada && (
          <motion.div
            key="lightbox"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
          >
            <LightboxFoto
              anuncio={anuncio}
              fotoActiva={fotoActiva}
              fotoPrincipal={fotoPrincipal}
              onCerrar={cerrarLightbox}
              onAnterior={anteriorFoto}
              onSiguiente={siguienteFoto}
            />
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}

function LightboxFoto({
  anuncio,
  fotoActiva,
  fotoPrincipal,
  onCerrar,
  onAnterior,
  onSiguiente,
}: PropsLightbox) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <button
        type="button"
        aria-label="Cerrar imagen ampliada"
        onClick={onCerrar}
        className="absolute inset-0 bg-black/90"
      />
      <div className="relative flex max-h-full max-w-full items-center justify-center">
        <button
          type="button"
          onClick={onCerrar}
          aria-label="Cerrar imagen"
          className="absolute right-5 top-5 z-10 flex h-11 w-11 items-center justify-center rounded-full bg-white/10 text-2xl text-white backdrop-blur transition-colors hover:bg-white/20"
        >
          ✕
        </button>

        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onAnterior();
          }}
          aria-label="Foto anterior"
          className="absolute left-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-2xl text-white backdrop-blur transition-colors hover:bg-white/20 sm:left-8"
        >
          ‹
        </button>

        <img
          src={fotoPrincipal}
          alt={`${anuncio.marca} ${anuncio.modelo} ${anuncio.anio} (ampliada)`}
          className="max-h-full max-w-full rounded-lg object-contain"
          onClick={(e) => e.stopPropagation()}
          onError={(e) => {
            const img = e.currentTarget;
            if (img.src !== IMAGEN_VACIA) img.src = IMAGEN_VACIA;
          }}
        />

        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onSiguiente();
          }}
          aria-label="Siguiente foto"
          className="absolute right-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-2xl text-white backdrop-blur transition-colors hover:bg-white/20 sm:right-8"
        >
          ›
        </button>

        {anuncio.fotos && anuncio.fotos.length > 1 && (
          <div className="absolute bottom-5 left-0 right-0 flex justify-center gap-1.5">
            {anuncio.fotos.map((_, i) => (
              <span
                key={i}
                className={`h-1.5 rounded-full transition-all ${
                  i === fotoActiva ? "w-5 bg-blue-500" : "w-1.5 bg-white/40"
                }`}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function useDetalleAnuncio(anuncioId: number, idValido: boolean) {
  const navigate = useNavigate();

  const {
    data: anuncio,
    isLoading: cargando,
    error: errorCarga,
  } = useVehiculoDetalle(idValido ? anuncioId : 0, idValido);

  const registrarVisita = useRegistrarVisita();
  const crearLead = useCrearLead();
  const agregarFavorito = useAgregarFavorito();
  const quitarFavorito = useQuitarFavorito();

  const [error, setError] = useState("");

  const [esFavorito, setEsFavorito] = useState(false);
  const [cargandoFavorito, setCargandoFavorito] = useState(false);

  const { esSeleccionado, toggle: toggleComparar } = useComparador();

  // Ref para asegurar que la visita solo se registra una vez por anuncio + usuario
  const visitaRegistradaRef = useRef(0);

  const [nombre, setNombre] = useState("");
  const [email, setEmail] = useState("");
  const [telefono, setTelefono] = useState("");
  const [mensaje, setMensaje] = useState("");
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [enviando, setEnviando] = useState(false);

  const usuario = authService.getCurrentUser();

  const esPropietario =
    usuario != null &&
    anuncio != null &&
    usuario.usuarioId === anuncio.usuarioId;

  const usuarioIdLogueado = usuario?.usuarioId;
  const nombreCompletoUsuario = usuario
    ? [usuario.nombre, usuario.apellido].filter(Boolean).join(" ").trim()
    : "";
  const emailUsuario = usuario?.email ?? "";

  // Si el visitante inició sesión, se precargan sus datos de contacto
  const cuentaQuery = useUsuarioCuenta(usuarioIdLogueado);

  useEffect(() => {
    const cuenta = cuentaQuery.data;
    if (!cuenta) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setNombre(nombreCompletoUsuario || cuenta.nombre || "");
    setEmail(cuenta.email ?? emailUsuario ?? "");
    setTelefono(cuenta.telefonoPersonal ?? "");
  }, [cuentaQuery.data, nombreCompletoUsuario, emailUsuario]);

  const esVendedorParticular = anuncio?.esVendedorParticular === true;

  const handleEnviar = async (e: React.FormEvent) => {
    e.preventDefault();

    if (crearLead.isPending) return;

    if (anuncio == null) return;

    if (esPropietario) return;

    if (mensaje.trim().length < 10) {
      setError("Escribe un mensaje de al menos 10 caracteres.");
      return;
    }

    setEnviando(true);

    try {
      await crearLead.mutateAsync({
        anuncioId: anuncio.id,
        nombreContacto: nombre.trim(),
        emailContacto: email.trim() || undefined,
        telefonoContacto: telefono.trim() || undefined,
        mensaje: mensaje.trim(),
        canal: "Formulario",
      });

      await Swal.fire({
        icon: "success",
        title: "Mensaje enviado",
        text: esVendedorParticular
          ? "El vendedor recibió tu mensaje y te contactará pronto."
          : "La agencia recibió tu mensaje y te contactará pronto.",
        confirmButtonColor: "#3b82f6",
      });

      setNombre("");
      setEmail("");
      setTelefono("");
      setMensaje("");
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message || "No se pudo enviar el mensaje.");
      } else {
        setError("No se pudo enviar el mensaje. Inténtalo nuevamente.");
      }
    } finally {
      setEnviando(false);
    }
  };

  const handleWhatsApp = async () => {
    if (anuncio == null || !anuncio.whatsAppContacto) return;

    const numero = anuncio.whatsAppContacto.replace(/[^\d]/g, "");
    if (!numero) return;

    const texto = `Hola, me interesa tu ${anuncio.marca} ${anuncio.modelo} (${anuncio.anio}). ¿Sigue disponible?`;

    if (usuario && !esPropietario) {
      try {
        await crearLead.mutateAsync({
          anuncioId: anuncio.id,
          nombreContacto: nombreCompletoUsuario || nombre || "Interesado",
          emailContacto: email || undefined,
          telefonoContacto: telefono || undefined,
          mensaje: texto,
          canal: "WhatsApp",
        });
      } catch {
        // Abrir WhatsApp no debe bloquearse aunque falle el registro del lead.
      }
    }

    window.open(
      `https://wa.me/${numero}?text=${encodeURIComponent(texto)}`,
      "_blank",
      "noopener,noreferrer",
    );
  };

  const propsMostradas = (() => {
    if (anuncio == null) return [];

    return [
      {
        etiqueta: "Tipo",
        valor: etiquetaDe(anuncio.tipoVehiculo, TIPOS_VEHICULO),
      },
      {
        etiqueta: "Transmisión",
        valor: etiquetaDe(anuncio.transmision, TRANSMISIONES),
      },
      {
        etiqueta: "Combustible",
        valor: etiquetaDe(anuncio.combustible, COMBUSTIBLES),
      },
      { etiqueta: "Motor", valor: anuncio.motor || "—" },
      { etiqueta: "Tracción", valor: anuncio.traccion || "—" },
    ];
  })();

  const esVehiculoNuevo = anuncio != null && anuncio.kilometraje <= 100;

  const esOferta =
    anuncio != null &&
    anuncio.precioAnterior != null &&
    anuncio.precioAnterior > anuncio.precio;

  const favoritosQuery = useMisFavoritos(
    usuario != null && anuncio != null && !esPropietario,
  );

  useEffect(() => {
    if (!anuncio || !favoritosQuery.data) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setEsFavorito(favoritosQuery.data.some((f) => f.id === anuncio.id));
  }, [favoritosQuery.data, anuncio]);

  // Registra la visita en el historial del comprador (sin bloquear la página).
  useEffect(() => {
    if (!anuncio || !usuario || esPropietario) return;

    // Solo registrar una vez por anuncio+usuario
    if (visitaRegistradaRef.current === anuncio.id) return;
    visitaRegistradaRef.current = anuncio.id;

    registrarVisita.mutate(anuncio.id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [anuncio?.id, usuario?.usuarioId, esPropietario]);

  const toggleFavorito = () => {
    if (!authService.isAuthenticated()) {
      navigate("/login");
      return;
    }

    if (anuncio == null || cargandoFavorito) return;

    setCargandoFavorito(true);
    const onSettled = () => setCargandoFavorito(false);

    if (esFavorito) {
      quitarFavorito.mutate(anuncio.id, { onSettled });
    } else {
      agregarFavorito.mutate(anuncio.id, { onSettled });
    }
  };

  const abrirFormulario = () => {
    setMostrarFormulario(true);
    setError("");
    if (!mensaje.trim() && anuncio != null) {
      setMensaje(
        `Hola, me interesa tu ${anuncio.marca} ${anuncio.modelo} (${anuncio.anio}). ¿Sigue disponible?`,
      );
    }
  };

  return {
    anuncio,
    cargando,
    errorCarga,
    esVehiculoNuevo,
    esOferta,
    esPropietario,
    esVendedorParticular,
    esFavorito,
    cargandoFavorito,
    toggleFavorito,
    esSeleccionado,
    toggleComparar,
    propsMostradas,
    usuario,
    nombre,
    email,
    telefono,
    mensaje,
    mostrarFormulario,
    enviando,
    error,
    setNombre,
    setEmail,
    setTelefono,
    setMensaje,
    abrirFormulario,
    handleEnviar,
    handleWhatsApp,
  };
}

export default function DetalleAnuncio() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const [reportando, setReportando] = useState(false);

  const handleReportar = async () => {
    if (!idValido || reportando) return;

    const resultado = await Swal.fire({
      icon: "warning",
      title: "Reportar anuncio",
      html: `
        <p class="text-sm text-left mb-3">Cuéntanos qué pasa con este anuncio. Nuestro equipo lo revisará.</p>
        <select id="reporte-motivo" class="swal2-select w-full">
          <option value="ContenidoInapropiado">Contenido inapropiado (+18, violencia, etc.)</option>
          <option value="FraudeEstafa">Fraude o estafa</option>
          <option value="InformacionFalsa">Información falsa</option>
          <option value="Duplicado">Anuncio duplicado</option>
          <option value="Otro" selected>Otro motivo</option>
        </select>
        <textarea id="reporte-detalle" class="swal2-textarea mt-2" placeholder="Detalles opcionales (máx. 500 caracteres)" maxlength="500"></textarea>
      `,
      showCancelButton: true,
      confirmButtonText: "Enviar reporte",
      cancelButtonText: "Cancelar",
      confirmButtonColor: "#dc2626",
      preConfirm: () => {
        const motivo = document.getElementById("reporte-motivo") as HTMLSelectElement | null;
        const detalle = document.getElementById("reporte-detalle") as HTMLTextAreaElement | null;
        return {
          motivo: motivo?.value ?? "Otro",
          detalle: detalle?.value.trim() ?? "",
        };
      },
    });

    if (!resultado.isConfirmed || !resultado.value) return;

    setReportando(true);
    try {
      const { reportesService } = await import("../services/reportes.service");
      await reportesService.reportar({
        anuncioId: anuncioId,
        motivo: resultado.value.motivo,
        detalle: resultado.value.detalle || undefined,
      });

      await Swal.fire({
        icon: "success",
        title: "Gracias por tu reporte",
        text: "Nuestro equipo lo revisará a la brevedad.",
        timer: 2500,
        showConfirmButton: false,
      });
    } catch (err) {
      const mensaje =
        (err as { message?: string })?.message ?? "No pudimos enviar tu reporte.";
      await Swal.fire({ icon: "error", title: "Error", text: mensaje });
    } finally {
      setReportando(false);
    }
  };

  // El slug es decorativo: el ID viaja al final ("honda-civic-2019-25" -> 25).
  // También acepta IDs puros ("/anuncio/25") para links antiguos.
  const anuncioId = idDesdeSlug(slug);
  const idValido = Number.isInteger(anuncioId) && anuncioId > 0;

  const detalle = useDetalleAnuncio(anuncioId, idValido);

  const {
    anuncio,
    cargando,
    errorCarga,
    esVehiculoNuevo,
    esOferta,
    esPropietario,
    esVendedorParticular,
    esFavorito,
    cargandoFavorito,
    toggleFavorito,
    esSeleccionado,
    toggleComparar,
    propsMostradas,
    usuario,
    nombre,
    email,
    telefono,
    mensaje,
    mostrarFormulario,
    enviando,
    error,
    setNombre,
    setEmail,
    setTelefono,
    setMensaje,
    abrirFormulario,
    handleEnviar,
    handleWhatsApp,
  } = detalle;

  // SEO: título de la pestaña con los datos del vehículo.
  useEffect(() => {
    if (anuncio) {
      document.title = `${anuncio.nombreAnuncio} | AutoMarket RD`;
    }
    return () => {
      document.title = "AutoMarket RD — Compra y venta de vehículos en República Dominicana";
    };
  }, [anuncio]);

  return (
    <div className="relative min-h-screen overflow-hidden bg-page text-ink">
      <HeaderPublico />

      <SectionBackground variant="gallery" className="mx-auto max-w-6xl px-6 py-8 sm:px-8">
      <main className="mx-auto max-w-6xl px-6 py-8 sm:px-8">
        {/* VOLVER */}
        <button
          type="button"
          onClick={() => navigate("/")}
          className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-ink-2 transition-colors hover:text-ink"
        >
          <FaArrowLeft />
          Volver a la vitrina
        </button>

        {cargando ? (
          <div className="flex items-center justify-center py-32">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-line border-t-blue-500" />
          </div>
        ) : (errorCarga || !idValido) && !anuncio ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-12 text-center">
            <FaCar className="mx-auto text-5xl text-red-400/60" />
            <h2 className="mt-4 text-lg font-semibold">
              Vehículo no disponible
            </h2>
            <p className="mt-2 text-sm text-ink-2">
              {!idValido
                ? "No se indicó el vehículo a consultar."
                : errorCarga instanceof Error
                  ? errorCarga.message
                  : "No se pudo cargar el vehículo."}
            </p>
            <button
              type="button"
              onClick={() => navigate("/")}
              className="mt-6 rounded-lg bg-blue-500 px-6 py-2 text-sm font-semibold transition-colors hover:bg-blue-600"
            >
              Ir a la vitrina
            </button>
          </div>
        ) : anuncio != null ? (
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            {/* COLUMNA IZQUIERDA: FOTOS + DESCRIPCIÓN */}
            <div className="lg:col-span-2">
              <GaleriaCompleta key={anuncio.id} anuncio={anuncio} />

              <DescripcionYEquipamiento anuncio={anuncio} />
            </div>

            {/* COLUMNA DERECHA: DATOS */}
            <motion.div
              className="space-y-6"
              initial="hidden"
              whileInView="visible"
              viewport={{ once: true, margin: "-40px" }}
              variants={{
                hidden: {},
                visible: { transition: { staggerChildren: 0.08 } },
              }}
            >
              <motion.div variants={{ hidden: { opacity: 0, y: 14 }, visible: { opacity: 1, y: 0 } }}>
                <TarjetaDatos
                  anuncio={anuncio}
                  esVehiculoNuevo={esVehiculoNuevo}
                  esOferta={esOferta}
                  esPropietario={esPropietario}
                  esFavorito={esFavorito}
                  cargandoFavorito={cargandoFavorito}
                  onToggleFavorito={toggleFavorito}
                  esSeleccionado={esSeleccionado}
                  onToggleComparar={toggleComparar}
                />
              </motion.div>

              <motion.div variants={{ hidden: { opacity: 0, y: 14 }, visible: { opacity: 1, y: 0 } }}>
                <FichaTecnica anuncio={anuncio} propsMostradas={propsMostradas} />
              </motion.div>

              <motion.div variants={{ hidden: { opacity: 0, y: 14 }, visible: { opacity: 1, y: 0 } }}>
                <SeccionContacto
                  anuncio={anuncio}
                  esPropietario={esPropietario}
                  esVendedorParticular={esVendedorParticular}
                  usuario={usuario}
                  nombre={nombre}
                  email={email}
                  telefono={telefono}
                  mensaje={mensaje}
                  mostrarFormulario={mostrarFormulario}
                  enviando={enviando}
                  error={error}
                  onChangeNombre={setNombre}
                  onChangeEmail={setEmail}
                  onChangeTelefono={setTelefono}
                  onChangeMensaje={setMensaje}
                  onAbrirFormulario={abrirFormulario}
                  onEnviar={handleEnviar}
                  onWhatsApp={handleWhatsApp}
                />
              </motion.div>

              {!esPropietario && (
                <motion.div variants={{ hidden: { opacity: 0, y: 14 }, visible: { opacity: 1, y: 0 } }}>
                  <button
                    type="button"
                    onClick={handleReportar}
                    disabled={reportando}
                    className="flex w-full items-center justify-center gap-2 rounded-lg px-4 py-2 text-xs font-medium text-ink-3 transition-colors hover:bg-hover hover:text-red-400 disabled:opacity-50"
                  >
                    <FaFlag />
                    {reportando ? "Enviando reporte..." : "Reportar este anuncio"}
                  </button>
                </motion.div>
              )}

              <motion.div variants={{ hidden: { opacity: 0, y: 14 }, visible: { opacity: 1, y: 0 } }}>
                <AdSlotRenderer ubicacion="DetalleLateral" />
              </motion.div>
            </motion.div>
          </div>
        ) : null}
      </main>
      </SectionBackground>

      {/* FOOTER */}
      <footer className="border-t border-line py-8">
        <div className="mx-auto max-w-6xl px-6 sm:px-8">
          <AdSlotRenderer ubicacion="DetalleFooter" orientacion="horizontal" className="mb-6" />
          <div className="flex flex-col items-center justify-between gap-4 text-sm text-ink-2 sm:flex-row">
            <span>© 2026 AutoMarket RD. Todos los derechos reservados.</span>
            <Link to="/precios" className="transition-colors hover:text-ink">
              Planes y precios
            </Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
