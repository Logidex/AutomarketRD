import LayoutPublico from "../../components/layout/LayoutPublico";

export default function Reembolso() {
  return (
    <LayoutPublico titulo="Política de Reembolso">
      <article className="prose prose-invert max-w-none space-y-6 text-[#cdd3e0]">
        <header>
          <p className="text-sm text-[#9aa1b1]">
            Última actualización: 12 de agosto de 2026
          </p>
        </header>

        <section>
          <h2 className="text-2xl font-bold text-white">1. Aplicación</h2>
          <p>
            Esta Política de Reembolso aplica a las suscripciones pagadas de
            AutoMarket RD (planes Básicos y posteriores) procesadas a través de
            PayPal. No aplica a la publicación de anuncios individuales ni a
            transacciones entre Compradores y Dealers, las cuales son
            responsabilidad exclusiva de las partes.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">2. Periodo de reembolso</h2>
          <p>
            Tienes derecho a solicitar un reembolso completo dentro de los{" "}
            <strong>7 días naturales</strong> Posteriores a la fecha de pago
            de tu suscripción, siempre que cumplas las condiciones indicadas
            en esta política.
          </p>
          <p>
            Pasado ese periodo, las suscripciones se consideran definitivas y
            no son reembolsables, salvo en los casos excepcionales descritos
            en la sección 4.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">3. Condiciones para solicitarlo</h2>
          <ul className="list-disc pl-6">
            <li>
              La solicitud se debe realizar dentro del plazo de 7 días
              Posteriores al pago.
            </li>
            <li>
              No se debe haber hecho un uso intensivo o fraudulento del plan
              contratado (p. ej., publicar y eliminar docenas de anuncios
              antes de pedir reembolso).
            </li>
            <li>
              Solo aplica a pagos realizados directamente a través de
              PayPal. Cargos por servicios de terceros (si los hubiera) no
              son reembolsables.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">4. Casos excepcionales</h2>
          <p>
            Se podrá evaluar un reembolso fuera del plazo de 7 días en
            situaciones como:
          </p>
          <ul className="list-disc pl-6">
            <li>
              Fallo técnico imputable a AutoMarket RD que impida usar las
              funciones del plan.
            </li>
            <li>Cobro duplicado por error del sistema o del webhook.</li>
            <li>
              Cancelación del servicio por parte de AutoMarket RD antes del
              fin del ciclo facturado.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">5. Cómo solicitar el reembolso</h2>
          <ol className="list-decimal pl-6">
            <li>
              Envía un correo a{" "}
              <a
                href="mailto:soporte.automarketrd@gmail.com"
                className="text-blue-400 hover:text-blue-300 underline"
              >
                soporte.automarketrd@gmail.com
              </a>{" "}
               con el asunto{" "}
              <em>"Solicitud de reembolso — [tu correo de cuenta]"</em>.
            </li>
            <li>
              Incluye: nombre del titular, correo de la cuenta, ID de orden
              de PayPal (visible en tu historial de pagos) y motivo de la
              solicitud.
            </li>
            <li>
              Responderemos dentro de los 5 días hábiles Posteriores.
            </li>
          </ol>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">6. Procesamiento del reembolso</h2>
          <p>
            Una vez aprobado, el reembolso se procesará a través de PayPal
            hacia el mismo método de pago original. El tiempo de reflejo en
            la cuenta del usuario puede tomar de 3 a 10 días hábiles,
            dependiendo de PayPal y del banco emisor.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">7. Efecto sobre la suscripción</h2>
          <p>
            Al procesarse un reembolso, la suscripción asociada se dará de
            baja inmediatamente, perdiendo el acceso a las funciones del plan
            contratado. La cuenta se mantiene activa pero degradada al plan{" "}
            <strong>Gratis</strong>.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">8. Denegación de reembolso</h2>
          <p>
            Nos reservamos el derecho de denegar solicitudes que no cumplan
            las condiciones de esta política. En ese caso, notificaremos al
            usuario con la justificación correspondiente.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">9. Contacto</h2>
          <p>
            Para consultas sobre esta política, escríbenos a{" "}
            <a
              href="mailto:soporte.automarketrd@gmail.com"
              className="text-blue-400 hover:text-blue-300 underline"
            >
              soporte.automarketrd@gmail.com
            </a>{" "}
          </p>
        </section>

      </article>
    </LayoutPublico>
  );
}
