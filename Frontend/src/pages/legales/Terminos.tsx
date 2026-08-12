import LayoutPublico from "../../components/layout/LayoutPublico";

export default function Terminos() {
  return (
    <LayoutPublico titulo="Términos y Condiciones">
      <article className="prose prose-invert max-w-none space-y-6 text-[#cdd3e0]">
        <header>
          <p className="text-sm text-[#9aa1b1]">Última actualización: TODO</p>
        </header>

        <section>
          <h2 className="text-2xl font-bold text-white">1. Aceptación de los términos</h2>
          <p>
            Al acceder y utilizar AutoMarket RD (el "Sitio"), aceptas quedar
            vinculado por estos Términos y Condiciones. Si no estás de acuerdo
            con alguna parte, no podrás acceder al servicio.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">2. Definiciones</h2>
          <ul className="list-disc pl-6">
            <li>
              <strong>AutoMarket RD</strong>: plataforma digital de
              compraventa de vehículos operada por TODO.
            </li>
            <li>
              <strong>Dealer / Vendedor</strong>: usuario registrado que
              publica vehículos para la venta.
            </li>
            <li>
              <strong>Comprador</strong>: usuario que contacta a un Dealer con
              la intención de adquirir un vehículo.
            </li>
            <li>
              <strong>Anuncio</strong>: publicación de un vehículo en la
              plataforma.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">3. Uso de la plataforma</h2>
          <p>
            El usuario se compromete a utilizar el Sitio conforme a la ley
            aplicable en la República Dominicana. Queda prohibido:
          </p>
          <ul className="list-disc pl-6">
            <li>Publicar información falsa o engañosa sobre vehículos.</li>
            <li>Usar la plataforma para fines ilícitos.</li>
            <li>Reproducir o copiar contenido sin autorización.</li>
            <li>Intentar acceder a cuentas ajenas o vulnerar la seguridad.</li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">4. Cuentas y registro</h2>
          <p>
            Para publicar vehículos o usar funciones exclusivas, debes crear
            una cuenta con datos verdaderos y actualizados. Eres responsable
            de mantener la confidencialidad de tu contraseña y de todas las
            actividades realizadas bajo tu cuenta.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">5. Planes y suscripciones</h2>
          <p>
            Algunas funciones del Sitio requieren la contratación de un plan
            de suscripción pagado. Los precios, ciclos de facturación y
            características de cada plan se muestran en la página{" "}
            <a
              href="/precios"
              className="text-blue-400 hover:text-blue-300 underline"
            >
              Planes y precios
            </a>
            . El pago se procesa a través de PayPal.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">6. Responsabilidad sobre los anuncios</h2>
          <p>
            AutoMarket RD actúa como intermediario y no es parte del contrato
            de compraventa entre Dealer y Comprador. No nos hacemos
            responsables por la veracidad de los anuncios, el estado de los
            vehículos ni el cumplimiento de las transacciones.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">7. Propiedad intelectual</h2>
          <p>
            El contenido del Sitio (logos, textos, código) es propiedad de
            AutoMarket RD. Los anuncios y fotos Publicados por los Dealers
            siguen siendo propiedad de quienes los Publican, quienes nos
            conceden una licencia para mostrarlos en la plataforma.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">8. Suspensión y cancelación</h2>
          <p>
            Nos reservamos el derecho de suspender o eliminar cuentas y
            anuncios que infrinjan estos Términos. Los usuarios pueden cerrar
            su cuenta en cualquier momento desde el panel correspondiente.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">9. Ley aplicable</h2>
          <p>
            Estos Términos se rigen por las leyes de la República Dominicana.
            Cualquier disputa se resolverá ante los tribunales competentes del
            distrito judicial correspondiente.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-white">10. Contacto</h2>
          <p>
            Para consultas relacionadas con estos Términos, escríbenos a{" "}
            <a
              href="mailto:soporte@automarket.com"
              className="text-blue-400 hover:text-blue-300 underline"
            >
              soporte@automarket.com
            </a>{" "}
            (TODO: email real de soporte).
          </p>
        </section>

        <div className="rounded-xl border border-yellow-500/30 bg-yellow-500/5 p-4 text-sm text-yellow-200/80">
          <strong className="font-semibold">TODO:</strong> este texto es una
          plantilla genérica. Debes revisarlo con asesoría legal antes de
          publicarlo oficialmente, sustituir el correo de contacto, la fecha
          de última actualización y cualquier dato específico de tu empresa.
        </div>
      </article>
    </LayoutPublico>
  );
}
