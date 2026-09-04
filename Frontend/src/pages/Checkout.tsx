import { useRef, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import Swal from "sweetalert2";
import { FaPaypal, FaSpinner, FaStore, FaUniversity, FaUpload, FaImage, FaTimes, FaCopy } from "react-icons/fa";
import { authService } from "../services/auth.service";
import { usePlanesCatalogo, useGenerarLinkPago, useRegistrarTransferencia } from "../hooks/useSuscripcion";
import { useCuentasBancariasActivas } from "../hooks/useCuentasBancarias";
import { ROLES } from "../constants/roles";
import { PAGOS_HABILITADOS } from "../constants/config";
import { formatearRD$, precioCicloDe, type Ciclo } from "../utils/formato";
import HeaderPublico from "../components/layout/HeaderPublico";
import SectionBackground from "../components/SectionBackground";

function nombreBanco(banco: number): string {
  switch (banco) {
    case 1: return "Popular";
    case 2: return "QIK";
    default: return "Banco";
  }
}

type MetodoPago = "paypal" | "transferencia";

export default function Checkout() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const planNivel = searchParams.get("plan") ?? "";
  const cicloParam = searchParams.get("ciclo");
  const ciclo: Ciclo =
    cicloParam === "Trimestral" || cicloParam === "Anual" ? cicloParam : "Mensual";

  const { data: planes = [] } = usePlanesCatalogo();
  const generarLinkPago = useGenerarLinkPago();
  const registrarTransferencia = useRegistrarTransferencia();
  const { data: cuentasBancarias = [], isLoading: cargandoCuentas } = useCuentasBancariasActivas();
  const [procesando, setProcesando] = useState(false);
  const [metodoSeleccionado, setMetodoSeleccionado] = useState<MetodoPago>("paypal");
  const [archivoCaptura, setArchivoCaptura] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const plan = planes.find((p) => p.nivel === planNivel) ?? null;
  const precio = plan ? precioCicloDe(plan, ciclo) : 0;

  const handleArchivoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const archivo = e.target.files?.[0];
    if (!archivo) return;

    if (archivo.size > 5 * 1024 * 1024) {
      Swal.fire({
        icon: "warning",
        title: "Archivo muy grande",
        text: "La imagen no debe exceder 5MB.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    const extensionesPermitidas = ["image/jpeg", "image/jpg", "image/png"];
    if (!extensionesPermitidas.includes(archivo.type)) {
      Swal.fire({
        icon: "warning",
        title: "Formato no válido",
        text: "Solo se permiten archivos JPG o PNG.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    setArchivoCaptura(archivo);
    const url = URL.createObjectURL(archivo);
    setPreviewUrl(url);
  };

  const handleEliminarArchivo = () => {
    setArchivoCaptura(null);
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    setPreviewUrl(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const handlePagarPayPal = async () => {
    if (generarLinkPago.isPending) return;

    const usuario = authService.getCurrentUser();

    if (!usuario || usuario.rol !== ROLES.DEALER) {
      await Swal.fire({
        icon: "info",
        title: "Inicia sesión como Dealer",
        text: "Para completar el pago debes iniciar sesión con una cuenta de Dealer.",
        confirmButtonColor: "#3b82f6",
      });
      navigate("/login");
      return;
    }

    if (!plan || precio <= 0) return;

    if (!PAGOS_HABILITADOS) {
      await Swal.fire({
        icon: "warning",
        title: "Sistema de pagos en mantenimiento",
        html: "El sistema de pagos se encuentra temporalmente no disponible.<br/><br/>Si deseas adquirir un plan, puedes solicitarlo contactando a nuestro equipo de soporte.",
        confirmButtonColor: "#3b82f6",
        confirmButtonText: "Ir a Contacto",
      });
      navigate("/contacto?asunto=Pagos+y+suscripciones");
      return;
    }

    setProcesando(true);

    try {
      const { url } = await generarLinkPago.mutateAsync({ plan: plan.nivel, ciclo });
      window.location.assign(url);
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error al iniciar el pago",
        text: err instanceof Error ? err.message : "Inténtalo nuevamente.",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setProcesando(false);
    }
  };

  const handleCopiarNumero = async (numero: string) => {
    try {
      await navigator.clipboard.writeText(numero);
      await Swal.fire({
        icon: "success",
        title: "Copiado",
        text: "Número de cuenta copiado al portapapeles.",
        timer: 1500,
        showConfirmButton: false,
      });
    } catch {
      await Swal.fire({
        icon: "error",
        title: "Error",
        text: "No se pudo copiar. Copia el número manualmente.",
        confirmButtonColor: "#3b82f6",
      });
    }
  };

  const handleEnviarTransferencia = async () => {
    if (registrarTransferencia.isPending) return;

    const usuario = authService.getCurrentUser();

    if (!usuario || usuario.rol !== ROLES.DEALER) {
      await Swal.fire({
        icon: "info",
        title: "Inicia sesión como Dealer",
        text: "Para enviar la transferencia debes iniciar sesión con una cuenta de Dealer.",
        confirmButtonColor: "#3b82f6",
      });
      navigate("/login");
      return;
    }

    if (!plan || precio <= 0) return;

    if (!archivoCaptura) {
      await Swal.fire({
        icon: "warning",
        title: "Captura requerida",
        text: "Debes adjuntar la captura de la transferencia.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    setProcesando(true);

    try {
      await registrarTransferencia.mutateAsync({
        plan: plan.nivel,
        ciclo,
        imagen: archivoCaptura,
      });

      await Swal.fire({
        icon: "success",
        title: "Comprobante enviado",
        html: "Tu comprobante fue recibido correctamente.<br/><br/>Tu suscripción se activará por <strong>1 día</strong> mientras nuestro equipo confirma el pago. Te notificaremos por correo cuando se confirme.",
        confirmButtonColor: "#3b82f6",
        confirmButtonText: "Ir a mi Panel",
      });

      navigate("/dashboard/suscripcion");
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error al enviar",
        text: err instanceof Error ? err.message : "Inténtalo nuevamente.",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setProcesando(false);
    }
  };

  return (
    <div className="relative min-h-screen overflow-hidden bg-page text-ink">
      <HeaderPublico titulo="Finalizar compra" />

      <SectionBackground variant="cta" className="mx-auto max-w-xl px-8 py-16">
      <main className="mx-auto max-w-xl px-8 py-16">
        {!plan ? (
          <div className="rounded-2xl border border-line bg-surface-2 p-10 text-center">
            <p className="text-ink-2 mb-6">
              No encontramos el plan seleccionado. Elige uno desde la página de Precios.
            </p>
            <Link
              to="/precios"
              className="inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-3 font-semibold hover:bg-blue-600 transition-colors"
            >
              <FaStore />
              Ver Precios
            </Link>
          </div>
        ) : (
          <div className="rounded-2xl border border-line bg-surface-2 overflow-hidden">
            {/* Resumen del plan */}
            <div className="p-8">
              <h1 className="text-2xl font-bold mb-1">{plan.nombre}</h1>
              <p className="text-ink-2 mb-6">{plan.descripcion}</p>

              <div className="space-y-3 mb-6">
                <div className="flex items-center justify-between border-b border-line pb-3">
                  <span className="text-ink-2">Plan</span>
                  <span className="font-semibold">{plan.nombre}</span>
                </div>
                <div className="flex items-center justify-between border-b border-line pb-3">
                  <span className="text-ink-2">Ciclo de facturación</span>
                  <span className="font-semibold">{ciclo}</span>
                </div>
                <div className="flex items-center justify-between border-b border-line pb-3">
                  <span className="text-ink-2">Anuncios incluidos</span>
                  <span className="font-semibold">{plan.limiteAnuncios}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-ink-2">Total a pagar</span>
                  <span className="text-3xl font-bold text-green-400">
                    {formatearRD$(precio)}
                  </span>
                </div>
              </div>

              {/* Selección de método de pago */}
              <div className="mb-6">
                <p className="text-sm text-ink-2 mb-3">Método de pago</p>
                <div className="grid grid-cols-2 gap-3">
                  <button
                    type="button"
                    onClick={() => setMetodoSeleccionado("paypal")}
                    className={`flex items-center gap-3 rounded-xl border p-4 transition-all ${
                      metodoSeleccionado === "paypal"
                        ? "border-blue-500 bg-blue-500/10 ring-2 ring-blue-500/20"
                        : "border-line bg-surface-2 hover:border-blue-300"
                    }`}
                  >
                    <FaPaypal className="text-2xl text-[#0070ba]" />
                    <div className="text-left">
                      <p className="font-semibold text-ink">PayPal</p>
                      <p className="text-xs text-ink-2">Pago inmediato</p>
                    </div>
                  </button>

                  <button
                    type="button"
                    onClick={() => setMetodoSeleccionado("transferencia")}
                    className={`flex items-center gap-3 rounded-xl border p-4 transition-all ${
                      metodoSeleccionado === "transferencia"
                        ? "border-blue-500 bg-blue-500/10 ring-2 ring-blue-500/20"
                        : "border-line bg-surface-2 hover:border-blue-300"
                    }`}
                  >
                    <FaUniversity className="text-2xl text-green-600" />
                    <div className="text-left">
                      <p className="font-semibold text-ink">Transferencia</p>
                      <p className="text-xs text-ink-2">Requiere confirmación</p>
                    </div>
                  </button>
                </div>
              </div>

              {/* Cuentas bancarias para transferencia */}
              {metodoSeleccionado === "transferencia" && (
                <div className="mb-6">
                  <p className="text-sm font-semibold text-ink mb-3">Cuentas bancarias para transferencia</p>
                  {cargandoCuentas ? (
                    <div className="flex items-center justify-center py-8">
                      <FaSpinner className="animate-spin text-2xl text-ink-3" />
                    </div>
                  ) : cuentasBancarias.length === 0 ? (
                    <p className="text-sm text-ink-2 text-center py-4">
                      No hay cuentas bancarias disponibles en este momento.
                    </p>
                  ) : (
                    <div className="space-y-3">
                      {cuentasBancarias.map((cuenta) => (
                        <div
                          key={cuenta.id}
                          className="rounded-xl border border-line bg-surface p-4 shadow-sm"
                        >
                          <div className="flex items-center justify-between gap-3">
                            <div className="flex items-center gap-2">
                              <FaUniversity className="text-lg text-ink-3" />
                              <p className="font-bold text-ink">{nombreBanco(cuenta.banco)}</p>
                              <span className="rounded-full bg-blue-500/10 px-2 py-0.5 text-xs font-semibold text-blue-500">
                                {cuenta.tipoCuenta}
                              </span>
                            </div>
                            <button
                              type="button"
                              onClick={() => handleCopiarNumero(cuenta.numeroCuenta)}
                              className="flex items-center gap-1.5 rounded-lg border border-line bg-surface-2 px-3 py-1.5 text-xs font-semibold text-ink-2 transition-colors hover:bg-hover hover:text-ink"
                            >
                              <FaCopy /> Copiar número
                            </button>
                          </div>
                          <p className="mt-2 font-mono text-lg font-semibold tracking-wide text-ink">
                            {cuenta.numeroCuenta}
                          </p>
                          <div className="mt-2 space-y-0.5 text-sm text-ink-2">
                            <p>
                              <span className="text-ink-3">Titular:</span> {cuenta.nombreTitular}
                              <span className="mx-2 text-ink-3">·</span>
                              <span className="text-ink-3">Doc:</span> {cuenta.documento}
                            </p>
                            <p>
                              <span className="text-ink-3">Ref:</span> {cuenta.conceptoReferencia}
                              <span className="mx-2 text-ink-3">·</span>
                              <span className="text-ink-3">Monto:</span>{" "}
                              <span className="font-semibold text-green-500">{formatearRD$(precio)}</span>
                            </p>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {/* Formulario de transferencia */}
              {metodoSeleccionado === "transferencia" && (
                <div className="mb-6 rounded-xl border border-line bg-surface-2 p-4">
                  <p className="text-sm font-semibold text-ink mb-3">Captura de la transferencia</p>
                  <p className="text-xs text-ink-2 mb-4">
                    Realiza la transferencia por <strong>{formatearRD$(precio)}</strong> a una de las cuentas anteriores y sube una captura del comprobante.
                    Tu suscripción se activará por 1 día hasta que confirmemos el pago.
                  </p>

                  {previewUrl ? (
                    <div className="relative mb-3">
                      <img
                        src={previewUrl}
                        alt="Captura de transferencia"
                        className="w-full rounded-lg border border-line object-contain max-h-64"
                      />
                      <button
                        type="button"
                        onClick={handleEliminarArchivo}
                        className="absolute top-2 right-2 rounded-full bg-red-500 p-1.5 text-white hover:bg-red-600 transition-colors"
                      >
                        <FaTimes className="text-xs" />
                      </button>
                    </div>
                  ) : (
                    <label className="flex flex-col items-center gap-2 rounded-lg border-2 border-dashed border-line p-6 cursor-pointer hover:border-blue-400 transition-colors">
                      <FaUpload className="text-2xl text-ink-3" />
                      <span className="text-sm text-ink-2">Seleccionar imagen</span>
                      <span className="text-xs text-ink-3">JPG o PNG, máx. 5MB</span>
                      <input
                        ref={fileInputRef}
                        type="file"
                        accept=".jpg,.jpeg,.png,image/jpeg,image/png"
                        onChange={handleArchivoChange}
                        className="hidden"
                      />
                    </label>
                  )}

                  {archivoCaptura && (
                    <div className="mt-2 flex items-center gap-2 text-xs text-ink-2">
                      <FaImage className="text-green-500" />
                      <span>{archivoCaptura.name}</span>
                      <span>({(archivoCaptura.size / 1024).toFixed(0)} KB)</span>
                    </div>
                  )}
                </div>
              )}

              {/* Botón de acción */}
              {metodoSeleccionado === "paypal" ? (
                <button
                  type="button"
                  onClick={handlePagarPayPal}
                  disabled={procesando}
                  className="w-full rounded-lg bg-[#0070ba] py-3.5 font-semibold hover:bg-[#005ea3] disabled:bg-[#3a6580] disabled:cursor-not-allowed flex items-center justify-center gap-2 transition-colors"
                >
                  {procesando ? (
                    <>
                      <FaSpinner className="animate-spin" />
                      Redirigiendo a PayPal...
                    </>
                  ) : (
                    <>
                      <FaPaypal className="text-xl" />
                      Pagar con PayPal
                    </>
                  )}
                </button>
              ) : (
                <button
                  type="button"
                  onClick={handleEnviarTransferencia}
                  disabled={procesando || !archivoCaptura}
                  className="w-full rounded-lg bg-green-600 py-3.5 font-semibold hover:bg-green-700 disabled:bg-green-400 disabled:cursor-not-allowed flex items-center justify-center gap-2 transition-colors"
                >
                  {procesando ? (
                    <>
                      <FaSpinner className="animate-spin" />
                      Enviando comprobante...
                    </>
                  ) : (
                    <>
                      <FaUpload className="text-xl" />
                      Enviar comprobante
                    </>
                  )}
                </button>
              )}

              <p className="text-xs text-ink-2 text-center mt-4">
                {metodoSeleccionado === "paypal"
                  ? "Al continuar serás redirigido a PayPal para completar el pago de forma segura. Al volver, tu suscripción se activará automáticamente."
                  : "Al enviar el comprobante, tu suscripción se activará por 1 día. El admin confirmará el pago y extenderá tu plan."}
              </p>
            </div>
          </div>
        )}
      </main>
      </SectionBackground>
    </div>
  );
}
