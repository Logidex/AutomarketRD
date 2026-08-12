import LayoutPublico from "../../components/layout/LayoutPublico";
import { Link } from "react-router-dom";
import { FaEnvelope, FaClock, FaShieldAlt } from "react-icons/fa";

export default function Contacto() {
  const soporteEmail = "soporte.automarketrd@gmail.com";

  return (
    <LayoutPublico titulo="Contacto">
      <article className="space-y-10">
        <header className="text-center">
          <h1 className="text-4xl font-bold mb-3 text-white">
            ¿Cómo podemos ayudarte?
          </h1>
          <p className="text-[#9aa1b1]">
            Nuestro equipo de soporte está disponible para resolver tus dudas,
            problemas técnicos o consultas comerciales.
          </p>
        </header>

        <div className="grid gap-6 sm:grid-cols-3">
          <div className="rounded-2xl border border-white/10 bg-white/[0.02] p-6 text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-blue-500/10 text-blue-400">
              <FaEnvelope />
            </div>
            <h2 className="text-lg font-semibold text-white mb-2">Email</h2>
            <p className="text-sm text-[#9aa1b1] mb-3">Escríbanos a</p>
            <a
              href={`mailto:${soporteEmail}`}
              className="inline-block max-w-full break-words rounded-lg bg-blue-500/10 px-4 py-2 text-sm font-medium text-blue-300 transition-colors hover:bg-blue-500/20"
            >
              {soporteEmail}
            </a>
          </div>

          <div className="rounded-2xl border border-white/10 bg-white/[0.02] p-6 text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-500/10 text-green-400">
              <FaClock />
            </div>
            <h2 className="text-lg font-semibold text-white mb-2">
              Horario de atención
            </h2>
            <p className="text-sm text-[#9aa1b1]">
              Lunes a Viernes
              <br />
              9:00 AM — 5:00 PM
              <br />
              (Hora de República Dominicana)
            </p>
          </div>

          <div className="rounded-2xl border border-white/10 bg-white/[0.02] p-6 text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-amber-500/10 text-amber-400">
              <FaShieldAlt />
            </div>
            <h2 className="text-lg font-semibold text-white mb-2">
              Tiempo de respuesta
            </h2>
            <p className="text-sm text-[#9aa1b1]">
              Hasta 48 horas hábiles
              <br />
              para consultas generales
              <br />
              hasta 24 horas para pagos
            </p>
          </div>
        </div>

        <section className="rounded-2xl border border-white/10 bg-white/[0.02] p-8">
          <h2 className="text-2xl font-bold text-white mb-6">
            Temas frecuentes
          </h2>

          <div className="space-y-6 text-[#cdd3e0]">
            <div>
              <h3 className="font-semibold text-white mb-1">
                Pagos y suscripciones
              </h3>
              <p className="text-sm">
                Dudas sobre tu plan, facturación, cancelación, reembolsos.
                Incluye tu correo de cuenta y el ID de orden de PayPal si
                aplica. Consulta también nuestra{" "}
                <Link
                  to="/reembolso"
                  className="text-blue-400 hover:text-blue-300 underline"
                >
                  Política de Reembolso
                </Link>
                .
              </p>
            </div>

            <div>
              <h3 className="font-semibold text-white mb-1">
                Problemas técnicos
              </h3>
              <p className="text-sm">
                Error al publicar un anuncio, no recibí notificaciones,
                problemas para iniciar sesión. Indica captura del error,
                navegador y pasos para reproducirlo.
              </p>
            </div>

            <div>
              <h3 className="font-semibold text-white mb-1">
                Verificación de cuenta
              </h3>
              <p className="text-sm">
                Si eres Dealer y necesitas verificar tu cuenta para desbloquear
                funciones avanzadas, envía tus documentos desde tu panel en{" "}
                <Link
                  to="/dashboard/mi-perfil"
                  className="text-blue-400 hover:text-blue-300 underline"
                >
                  Mi Perfil
                </Link>
                .
              </p>
            </div>

            <div>
              <h3 className="font-semibold text-white mb-1">
                Disputas entre usuarios
              </h3>
              <p className="text-sm">
                AutoMarket RD actúa como intermediario y no participa en
                contratos de compraventa. Para disputas sobre vehículos,
                contacta directamente al Dealer o a las autoridades competentes.
              </p>
            </div>
          </div>
        </section>

        <section className="rounded-2xl border border-blue-500/20 bg-blue-500/[0.03] p-6 text-center">
          <p className="text-[#cdd3e0]">
            ¿Listo para escribirnos? Envía tu correo a{" "}
            <a
              href={`mailto:${soporteEmail}`}
              className="font-semibold text-blue-300 hover:text-blue-200 underline"
            >
              {soporteEmail}
            </a>{" "}
            y te responderemos lo antes posible.
          </p>
        </section>
      </article>
    </LayoutPublico>
  );
}
