import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Swal from "sweetalert2";
import {
  FaArrowLeft,
  FaCalendarAlt,
  FaCar,
  FaEnvelope,
  FaHeart,
  FaMapMarkerAlt,
  FaPaperPlane,
  FaRegHeart,
  FaStore,
  FaTachometerAlt,
  FaUsers,
  FaWhatsapp,
} from "react-icons/fa";
import { anuncioService } from "../services/anuncio.service";
import { leadService } from "../services/lead.service";
import { favoritoService } from "../services/favorito.service";
import { historialService } from "../services/historial.service";
import { authService } from "../services/auth.service";
import type { AnuncioDetalle } from "../types/anuncio.types";
import logo from "../assets/AutoMarketRD_Logo.svg";
import NavbarUsuario from "../components/layout/NavbarUsuario";
import {
  TIPOS_VEHICULO,
  TRANSMISIONES,
  COMBUSTIBLES,
  etiquetaDe,
} from "../constants/vehiculo.opciones";

const IMAGEN_VACIA =
  "https://via.placeholder.com/800x500?text=Sin+Foto";

export default function DetalleAnuncio() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [anuncio, setAnuncio] = useState<AnuncioDetalle | null>(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");

  const [esFavorito, setEsFavorito] = useState(false);
  const [cargandoFavorito, setCargandoFavorito] = useState(false);

  const [fotoActiva, setFotoActiva] = useState(0);
  const [fotoAmpliada, setFotoAmpliada] = useState(false);
  const [enPausa, setEnPausa] = useState(false);

  const [nombre, setNombre] = useState("");
  const [email, setEmail] = useState("");
  const [telefono, setTelefono] = useState("");
  const [mensaje, setMensaje] = useState("");
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    let activo = true;

    async function cargar() {
      if (!id) {
        setError("No se indicó el vehículo a consultar.");
        setCargando(false);
        return;
      }

      try {
        const datos = await anuncioService.obtenerPorId(id);
        if (!activo) return;

        setAnuncio(datos);
        setFotoActiva(0);
        setError("");

        if (datos.estado === "Publicado") {
          try {
            await anuncioService.registrarVista(datos.id);
          } catch {
            // Registrar la vista es transaccional; si falla no bloquea la ficha.
          }
        }
      } catch (err) {
        if (!activo) return;
        setError(
          err instanceof Error
            ? err.message
            : "No se pudo cargar el vehículo.",
        );
      } finally {
        if (activo) setCargando(false);
      }
    }

    cargar();

    return () => {
      activo = false;
    };
  }, [id]);

  const usuario = authService.getCurrentUser();

  const esPropietario =
    usuario != null && anuncio != null && usuario.usuarioId === anuncio.usuarioId;

  const esVendedorParticular = anuncio?.esVendedorParticular === true;

  const fotoPrincipal =
    anuncio != null && anuncio.fotos && anuncio.fotos.length > 0
      ? anuncio.fotos[fotoActiva]
      : IMAGEN_VACIA;

  // Carrusel: avanza solo cada 2s mientras haya fotos y el usuario
  // no esté viendo una ampliada ni haya pausado (hover).
  useEffect(() => {
    if (!anuncio || !anuncio.fotos || anuncio.fotos.length <= 1) return;
    if (fotoAmpliada || enPausa) return;

    const intervalo = window.setInterval(() => {
      setFotoActiva((actual) =>
        actual + 1 >= anuncio.fotos!.length ? 0 : actual + 1,
      );
    }, 2000);

    return () => window.clearInterval(intervalo);
  }, [anuncio, fotoAmpliada, enPausa]);

  const siguienteFoto = () => {
    if (!anuncio || !anuncio.fotos || anuncio.fotos.length === 0) return;
    setFotoActiva((actual) =>
      actual + 1 >= anuncio.fotos!.length ? 0 : actual + 1,
    );
  };

  const anteriorFoto = () => {
    if (!anuncio || !anuncio.fotos || anuncio.fotos.length === 0) return;
    setFotoActiva((actual) =>
      actual - 1 < 0 ? anuncio.fotos!.length - 1 : actual - 1,
    );
  };

  const handleEnviar = async (e: React.FormEvent) => {
    e.preventDefault();

    if (anuncio == null) return;

    if (esPropietario) return;

    if (mensaje.trim().length < 10) {
      setError("Escribe un mensaje de al menos 10 caracteres.");
      return;
    }

    setEnviando(true);

    try {
      await leadService.crearLead({
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

  const handleWhatsApp = () => {
    if (anuncio == null || !anuncio.whatsAppContacto) return;

    const numero = anuncio.whatsAppContacto.replace(/[^\d]/g, "");
    if (!numero) return;

    const texto = `Hola, me interesa tu ${anuncio.marca} ${anuncio.modelo} (${anuncio.anio}). ¿Sigue disponible?`;

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

  // Estado de favorito: solo aplica a compradores autenticados que no sean dueños
  useEffect(() => {
    if (!authService.isAuthenticated() || anuncio == null || esPropietario)
      return;

    let activo = true;
    favoritoService
      .obtenerMisFavoritos()
      .then((lista) => {
        if (activo) setEsFavorito(lista.some((f) => f.id === anuncio.id));
      })
      .catch(() => {});

    // Registra la visita en el historial del comprador (sin bloquear la página)
    historialService.registrarVista(anuncio.id).catch(() => {});

    return () => {
      activo = false;
    };
  }, [anuncio, esPropietario]);

  const toggleFavorito = async () => {
    if (!authService.isAuthenticated()) {
      navigate("/login");
      return;
    }

    if (anuncio == null || cargandoFavorito) return;

    setCargandoFavorito(true);
    try {
      if (esFavorito) {
        await favoritoService.quitar(anuncio.id);
        setEsFavorito(false);
      } else {
        await favoritoService.agregar(anuncio.id);
        setEsFavorito(true);
      }
    } catch {
      // El interceptor de api.ts ya expone el mensaje del backend en error.message.
    } finally {
      setCargandoFavorito(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      {/* HEADER */}
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link to="/" className="flex items-center gap-4">
          <img
            src={logo}
            alt="AutoMarket RD"
            className="h-20 w-auto object-contain"
          />
        </Link>

        <nav className="flex items-center gap-6 text-sm font-medium">
          <Link
            to="/precios"
            className="text-[#9aa1b1] transition-colors hover:text-white"
          >
            Precios
          </Link>
          <NavbarUsuario />
        </nav>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-8 sm:px-8">
        {/* VOLVER */}
        <button
          type="button"
          onClick={() => navigate("/")}
          className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white"
        >
          <FaArrowLeft />
          Volver a la vitrina
        </button>

        {cargando ? (
          <div className="flex items-center justify-center py-32">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
          </div>
        ) : error && !anuncio ? (
          <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-12 text-center">
            <FaCar className="mx-auto text-5xl text-red-400/60" />
            <h2 className="mt-4 text-lg font-semibold">Vehículo no disponible</h2>
            <p className="mt-2 text-sm text-[#9aa1b1]">{error}</p>
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
              <div className="overflow-hidden rounded-2xl border border-white/10 bg-[#13161d]">
                <div
                  className="group relative aspect-[16/10] cursor-pointer"
                  onClick={() => setFotoAmpliada(true)}
                  onMouseEnter={() => setEnPausa(true)}
                  onMouseLeave={() => setEnPausa(false)}
                >
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

                  {!fotoAmpliada &&
                    anuncio.fotos &&
                    anuncio.fotos.length > 1 && (
                      <div className="absolute left-0 right-0 top-1/2 flex -translate-y-1/2 items-center justify-between px-3 opacity-0 transition-opacity group-hover:opacity-100">
                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            anteriorFoto();
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
                            siguienteFoto();
                          }}
                          aria-label="Siguiente foto"
                          className="flex h-10 w-10 items-center justify-center rounded-full bg-black/60 text-lg text-white backdrop-blur transition-colors hover:bg-black/80"
                        >
                          ›
                        </button>
                      </div>
                    )}

                  {anuncio.fotos && anuncio.fotos.length > 1 && (
                    <div className="absolute bottom-3 left-0 right-0 flex justify-center gap-1.5">
                      {anuncio.fotos.map((_, i) => (
                        <span
                          key={i}
                          className={`h-1.5 rounded-full transition-all ${
                            i === fotoActiva
                              ? "w-5 bg-blue-500"
                              : "w-1.5 bg-white/40"
                          }`}
                        />
                      ))}
                    </div>
                  )}
                </div>

                {anuncio.fotos && anuncio.fotos.length > 1 && (
                  <div className="flex flex-wrap gap-2 border-t border-white/10 p-3">
                    {anuncio.fotos.map((foto, i) => (
                      <button
                        key={i}
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          setEnPausa(true);
                          setFotoActiva(i);
                          window.setTimeout(() => setEnPausa(false), 5000);
                        }}
                        onMouseEnter={() => setEnPausa(true)}
                        onMouseLeave={() => setEnPausa(false)}
                        className={`h-16 w-24 overflow-hidden rounded-lg border transition-colors ${
                          i === fotoActiva
                            ? "border-blue-500"
                            : "border-white/10 hover:border-white/30"
                        }`}
                      >
                        <img
                          src={foto}
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

              {/* DESCRIPCIÓN */}
              {anuncio.descripcion && (
                <section className="mt-8 rounded-2xl border border-white/10 bg-[#13161d] p-6">
                  <h2 className="text-lg font-bold">Descripción</h2>
                  <p className="mt-3 whitespace-pre-line text-sm leading-relaxed text-[#c3c9d4]">
                    {anuncio.descripcion}
                  </p>
                </section>
              )}

              {/* ACCESORIOS */}
              {anuncio.accesorios && anuncio.accesorios.length > 0 && (
                <section className="mt-8 rounded-2xl border border-white/10 bg-[#13161d] p-6">
                  <h2 className="text-lg font-bold">Equipamiento</h2>
                  <div className="mt-4 flex flex-wrap gap-2">
                    {anuncio.accesorios.map((accesorio) => (
                      <span
                        key={accesorio}
                        className="rounded-full border border-white/10 bg-white/5 px-3 py-1.5 text-xs font-medium text-gray-300"
                      >
                        {accesorio}
                      </span>
                    ))}
                  </div>
                </section>
              )}
            </div>

            {/* COLUMNA DERECHA: DATOS */}
            <div className="space-y-6">
              {/* TÍTULO + PRECIO */}
              <div className="rounded-2xl border border-white/10 bg-[#13161d] p-6">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h1 className="text-2xl font-bold">
                      {anuncio.marca} {anuncio.modelo}
                    </h1>
                    <p className="mt-1 text-sm text-[#9aa1b1]">
                      {anuncio.version || "—"}
                    </p>
                  </div>
                  {esVehiculoNuevo && (
                    <span className="rounded-full bg-green-500/10 px-3 py-1 text-xs font-semibold text-green-400">
                      Nuevo
                    </span>
                  )}
                </div>

                <p className="mt-4 text-3xl font-bold text-blue-500">
                  RD$ {anuncio.precio.toLocaleString("es-DO")}
                </p>

                {!esPropietario && (
                  <button
                    type="button"
                    onClick={toggleFavorito}
                    disabled={cargandoFavorito}
                    aria-pressed={esFavorito}
                    className={`mt-4 flex w-full items-center justify-center gap-2 rounded-lg border px-5 py-2.5 text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${
                      esFavorito
                        ? "border-red-500/40 bg-red-500/10 text-red-400 hover:bg-red-500/20"
                        : "border-white/10 bg-white/5 text-[#9aa1b1] hover:border-red-500/40 hover:text-red-400"
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

                <div className="mt-5 grid grid-cols-2 gap-3 text-sm">
                  <div className="flex items-center gap-2 text-[#c3c9d4]">
                    <FaCalendarAlt className="text-gray-500" />
                    {anuncio.anio}
                  </div>
                  <div className="flex items-center gap-2 text-[#c3c9d4]">
                    <FaTachometerAlt className="text-gray-500" />
                    {esVehiculoNuevo
                      ? "Nuevo"
                      : `${anuncio.kilometraje.toLocaleString("es-DO")} km`}
                  </div>
                  {anuncio.ubicacion && (
                    <div className="col-span-2 flex items-center gap-2 text-[#c3c9d4]">
                      <FaMapMarkerAlt className="text-gray-500" />
                      {anuncio.ubicacion}
                    </div>
                  )}
                </div>
              </div>

              {/* SPECS */}
              <div className="rounded-2xl border border-white/10 bg-[#13161d] p-6">
                <h2 className="text-sm font-bold uppercase tracking-wide text-[#9aa1b1]">
                  Ficha técnica
                </h2>
                <dl className="mt-4 grid grid-cols-2 gap-x-4 gap-y-4 text-sm">
                  {propsMostradas.map((fila) => (
                    <div key={fila.etiqueta}>
                      <dt className="text-xs text-[#9aa1b1]">{fila.etiqueta}</dt>
                      <dd className="mt-0.5 font-medium text-[#e8eaf0]">
                        {fila.valor}
                      </dd>
                    </div>
                  ))}
                  <div>
                    <dt className="text-xs text-[#9aa1b1]">Color exterior</dt>
                    <dd className="mt-0.5 font-medium text-[#e8eaf0]">
                      {anuncio.colorExterior || "—"}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-xs text-[#9aa1b1]">Color interior</dt>
                    <dd className="mt-0.5 font-medium text-[#e8eaf0]">
                      {anuncio.colorInterior || "—"}
                    </dd>
                  </div>
                </dl>
              </div>

              {/* CONTACTAR */}
              {esPropietario ? (
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
                <div className="rounded-2xl border border-white/10 bg-[#13161d] p-6">
                  <h2 className="text-lg font-bold">
                    {esVendedorParticular ? "Contactar vendedor" : "Contactar agencia"}
                  </h2>
                  {anuncio.nombreVendedor && (
                    <p className="mt-1 text-sm font-medium text-[#c3c9d4]">
                      {anuncio.nombreVendedor}
                    </p>
                  )}

                  <span
                    className={`mt-2 inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                      esVendedorParticular
                        ? "bg-violet-500/15 text-violet-300"
                        : "bg-blue-500/15 text-blue-300"
                    }`}
                  >
                    {esVendedorParticular ? "Vendedor particular" : "Dealer (agencia)"}
                  </span>

                  <Link
                    to={`/vendedor/${anuncio.usuarioId}`}
                    className="mt-2 inline-flex items-center gap-2 text-sm font-semibold text-blue-400 transition-colors hover:text-blue-300"
                  >
                    <FaStore />
                    {esVendedorParticular
                      ? "Ver perfil del vendedor"
                      : "Ver perfil de la agencia"}
                  </Link>

                  <div className="mt-5 grid grid-cols-1 gap-3">
                    {anuncio.whatsAppContacto && (
                      <button
                        type="button"
                        onClick={handleWhatsApp}
                        className="flex w-full items-center justify-center gap-2 rounded-lg bg-green-500 px-6 py-3 text-sm font-semibold transition-colors hover:bg-green-600"
                      >
                        <FaWhatsapp className="text-lg" />
                        Contactar por WhatsApp
                      </button>
                    )}

                    {!mostrarFormulario && (
                      <button
                        type="button"
                        onClick={() => {
                          setMostrarFormulario(true);
                          setError("");
                        }}
                        className="flex w-full items-center justify-center gap-2 rounded-lg bg-blue-500 px-6 py-3 text-sm font-semibold transition-colors hover:bg-blue-600"
                      >
                        <FaEnvelope className="text-lg" />
                        Contactar por correo
                      </button>
                    )}
                  </div>

                  {mostrarFormulario && (
                    <form onSubmit={handleEnviar} className="mt-5 space-y-4">
                    <div>
                      <label className="mb-1 block text-xs font-medium text-[#9aa1b1]">
                        Nombre *
                      </label>
                      <input
                        type="text"
                        required
                        maxLength={100}
                        value={nombre}
                        onChange={(e) => setNombre(e.target.value)}
                        placeholder="Tu nombre"
                        className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                      />
                    </div>

                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      <div>
                        <label className="mb-1 block text-xs font-medium text-[#9aa1b1]">
                          Email
                        </label>
                        <input
                          type="email"
                          maxLength={150}
                          value={email}
                          onChange={(e) => setEmail(e.target.value)}
                          placeholder="tucorreo@ejemplo.com"
                          className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                      <div>
                        <label className="mb-1 block text-xs font-medium text-[#9aa1b1]">
                          Teléfono
                        </label>
                        <input
                          type="tel"
                          maxLength={20}
                          value={telefono}
                          onChange={(e) => setTelefono(e.target.value)}
                          placeholder="809-000-0000"
                          className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
                        />
                      </div>
                    </div>

                    <div>
                      <label className="mb-1 block text-xs font-medium text-[#9aa1b1]">
                        Mensaje *
                      </label>
                      <textarea
                        required
                        maxLength={1000}
                        rows={4}
                        value={mensaje}
                        onChange={(e) => setMensaje(e.target.value)}
                        placeholder="Hola, me interesa este vehículo. ¿Sigue disponible?"
                        className="w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none"
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

                  <div className="mt-5 border-t border-white/10 pt-4 text-center text-[10px] text-[#6b7280]">
                    {esVendedorParticular
                      ? "Al contactar, el vendedor recibirá tus datos para responderte directamente."
                      : "Al contactar, la agencia recibirá tus datos para responderte directamente."}
                  </div>
                </div>
              )}
            </div>
          </div>
        ) : null}
      </main>

      {/* LIGHTBOX / FOTO AMPLIADA */}
      {fotoAmpliada && anuncio != null && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/90 p-4"
          onClick={() => {
            setFotoAmpliada(false);
            setEnPausa(false);
          }}
        >
          <button
            type="button"
            onClick={() => {
              setFotoAmpliada(false);
              setEnPausa(false);
            }}
            aria-label="Cerrar imagen"
            className="absolute right-5 top-5 flex h-11 w-11 items-center justify-center rounded-full bg-white/10 text-2xl text-white backdrop-blur transition-colors hover:bg-white/20"
          >
            ✕
          </button>

          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              anteriorFoto();
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
              siguienteFoto();
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
                    i === fotoActiva
                      ? "w-5 bg-blue-500"
                      : "w-1.5 bg-white/40"
                  }`}
                />
              ))}
            </div>
          )}
        </div>
      )}

      {/* FOOTER */}
      <footer className="border-t border-white/10 py-8">
        <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-6 text-sm text-[#9aa1b1] sm:flex-row sm:px-8">
          <span>© 2026 AutoMarket RD. Todos los derechos reservados.</span>
          <Link to="/precios" className="transition-colors hover:text-white">
            Planes y precios
          </Link>
        </div>
      </footer>
    </div>
  );
}