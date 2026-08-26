import LayoutPublico from "../../components/layout/LayoutPublico";

export default function Privacidad() {
  return (
    <LayoutPublico titulo="Política de Privacidad">
      <article className="prose prose-invert max-w-none space-y-6 text-ink-3">
        <header>
          <p className="text-sm text-ink-2">
            Última actualización: 15 de agosto de 2026
          </p>
        </header>

        <section>
          <h2 className="text-2xl font-bold text-ink">1. Responsable del tratamiento</h2>
          <p>
            AutoMarket RD, con domicilio en Santo Domingo, República Dominicana, es responsable del
            tratamiento de los datos personales recopilados a través de este
            Sitio. Puedes contactarnos a través del panel de soporte de tu
            cuenta o en{" "}
            <a
              href="mailto:soporte@automarket-rd.com"
              className="text-blue-400 hover:text-blue-300 underline"
            >
              soporte@automarket-rd.com
            </a>{" "}
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">2. Datos que recopilamos</h2>
          <ul className="list-disc pl-6">
            <li>
              <strong>De registro</strong>: nombre, correo electrónico,
              contraseña (encriptada) y rol (Comprador, Vendedor, Dealer o
              Admin).
            </li>
            <li>
              <strong>De perfil público</strong>: foto, teléfono, ubicación y
              descripción del Dealer.
            </li>
            <li>
              <strong>De anuncios</strong>: fotos, descripciones, precio y
              datos del vehículo publicado.
            </li>
            <li>
              <strong>De actividad</strong>: leads generados, favoritos,
              historial de visitas e historial de pagos.
            </li>
            <li>
              <strong>De soporte</strong>: contenido de los tickets y mensajes
              intercambiados con nuestro equipo de soporte.
            </li>
            <li>
              <strong>Técnicos</strong>: dirección IP, tipo de navegador y
              cookies esenciales de sesión.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">3. Finalidad del tratamiento</h2>
          <p>Usamos tus datos para:</p>
          <ul className="list-disc pl-6">
            <li>Permitir el registro y acceso a la plataforma.</li>
            <li>Publicar y gestionar anuncios de vehículos.</li>
            <li>Procesar pagos de suscripciones a través de PayPal.</li>
            <li>Conectar compradores con vendedores mediante leads.</li>
            <li>Enviar notificaciones operativas (recuperación de
              contraseña, confirmación de pago).</li>
            <li>Cumplir obligaciones legales y prevenir fraude.</li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">4. Base legal</h2>
          <p>
            Tratamos tus datos con base en tu consentimiento al registrarte,
            en la ejecución del contrato de suscripción cuando aplicable, y
            en nuestro interés legítimo para operar la plataforma de forma
            segura, en cumplimiento de la{" "}
            <strong>Ley 172-13 sobre Protección de Datos Personales</strong>
            {" "}de la República Dominicana.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">5. Compartición de datos</h2>
          <p>
            No vendemos tus datos. Compartimos información únicamente con:
          </p>
          <ul className="list-disc pl-6">
            <li>
              <strong>PayPal</strong>, para procesar pagos de suscripciones.
            </li>
            <li>
              <strong>AWS S3</strong>, para almacenar las imágenes de los
              anuncios.
            </li>
            <li>
              <strong>Proveedores de email</strong>, para enviar correos
              transaccionales.
            </li>
            <li>
              <strong>Autoridades</strong>, cuando sea requerido por ley.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">6. Conservación de los datos</h2>
          <p>
            Conservamos tus datos mientras tu cuenta esté activa. Puedes
            solicitar su eliminación en cualquier momento escribiendo al
            correo de contacto. Algunos datos podrán conservarse bloqueados
            cuando exista una obligación legal de retención.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">7. Derechos del usuario</h2>
          <p>Como titular de los datos, puedes ejercer:</p>
          <ul className="list-disc pl-6">
            <li>Derecho de acceso a tus datos personales.</li>
            <li>Derecho de rectificación.</li>
            <li>Derecho de supresión ("derecho al olvido").</li>
            <li>Derecho a la portabilidad.</li>
            <li>Derecho a oponerte al tratamiento.</li>
            <li>Derecho a limitar el tratamiento.</li>
          </ul>
          <p>
            Para ejercerlos, abre un ticket de soporte desde tu cuenta o
            escríbenos a{" "}
            <a
              href="mailto:soporte@automarket-rd.com"
              className="text-blue-400 hover:text-blue-300 underline"
            >
              soporte@automarket-rd.com
            </a>{" "}
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">8. Seguridad</h2>
          <p>
            Aplicamos medidas técnicas y organizativas razonables para
            proteger tus datos: contraseñas encriptadas, conexión HTTPS, y
            acceso restringido a personal autorizado. Ningún sistema es 100%
            seguro, pero trabajamos para minimizar riesgos.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">9. Cookies</h2>
          <p>
            Usamos cookies esenciales para mantener la sesión del usuario
            autenticado. No usamos cookies de terceros con fines
            publicitarios.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-bold text-ink">10. Cambios a esta política</h2>
          <p>
            Podemos actualizar esta Política. Te avisaremos de cambios
            importantes mediante la plataforma o por correo electrónico. La
            fecha de "última actualización" al inicio refleja la versión
            vigente. Si haces uso de la plataforma después de los cambios,
            se considerará que los aceptas.
          </p>
        </section>
      </article>
    </LayoutPublico>
  );
}
