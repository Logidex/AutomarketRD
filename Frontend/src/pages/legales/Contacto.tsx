import { useState, type FormEvent } from 'react';
import { FaEnvelope, FaClock, FaShieldAlt, FaSpinner } from 'react-icons/fa';
import Swal from 'sweetalert2';
import LayoutPublico from '../../components/layout/LayoutPublico';
import { Link } from 'react-router-dom';
import { useEnviarContacto } from '../../hooks/useContacto';

const soporteEmail = 'soporte.automarketrd@gmail.com';

const ASUNTOS_PREDEFINIDOS = [
  'Pagos y suscripciones',
  'Problema técnico',
  'Verificación de cuenta',
  'Reporte de anuncio',
  'Sugerencia',
  'Otro',
];

export default function Contacto() {
  const enviarContacto = useEnviarContacto();
  const [form, setForm] = useState({
    nombre: '',
    email: '',
    asunto: '',
    mensaje: '',
    website: '',
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
      await enviarContacto.mutateAsync(form);
      await Swal.fire({
        icon: 'success',
        title: 'Mensaje enviado',
        text: 'Gracias por contactarnos. Te responderemos lo antes posible.',
        confirmButtonColor: '#3b82f6',
      });
      setForm({ nombre: '', email: '', asunto: '', mensaje: '', website: '' });
    } catch (err) {
      await Swal.fire({
        icon: 'error',
        title: 'No se pudo enviar',
        text: err instanceof Error ? err.message : 'Inténtalo nuevamente.',
        confirmButtonColor: '#3b82f6',
      });
    }
  };

  const inputClass =
    'w-full rounded-lg border border-white/10 bg-white/[0.02] p-3 text-white placeholder-white/30 transition-colors focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';
  const labelClass = 'mb-1.5 block text-sm font-medium text-white/80';

  return (
    <LayoutPublico titulo='Contacto'>
      <article className='space-y-10'>
        <header className='text-center'>
          <h1 className='mb-3 text-4xl font-bold text-white'>
            ¿Cómo podemos ayudarte?
          </h1>
          <p className='text-[#9aa1b1]'>
            Nuestro equipo de soporte está disponible para resolver tus dudas,
            problemas técnicos o consultas comerciales.
          </p>
        </header>

        <div className='grid gap-6 sm:grid-cols-3'>
          <div className='rounded-2xl border border-white/10 bg-white/[0.02] p-6 text-center'>
            <div className='mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-blue-500/10 text-blue-400'>
              <FaEnvelope />
            </div>
            <h2 className='mb-2 text-lg font-semibold text-white'>Email</h2>
            <p className='mb-3 text-sm text-[#9aa1b1]'>Escríbanos directamente a</p>
            <a
              href={`mailto:${soporteEmail}`}
              className='inline-block max-w-full break-words rounded-lg bg-blue-500/10 px-4 py-2 text-sm font-medium text-blue-300 transition-colors hover:bg-blue-500/20'
            >
              {soporteEmail}
            </a>
          </div>

          <div className='rounded-2xl border border-white/10 bg-white/[0.02] p-6 text-center'>
            <div className='mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-green-500/10 text-green-400'>
              <FaClock />
            </div>
            <h2 className='mb-2 text-lg font-semibold text-white'>
              Horario de atención
            </h2>
            <p className='text-sm text-[#9aa1b1]'>
              Lunes a Viernes
              <br />
              9:00 AM — 5:00 PM
              <br />
              (Hora de República Dominicana)
            </p>
          </div>

          <div className='rounded-2xl border border-white/10 bg-white/[0.02] p-6 text-center'>
            <div className='mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-amber-500/10 text-amber-400'>
              <FaShieldAlt />
            </div>
            <h2 className='mb-2 text-lg font-semibold text-white'>
              Tiempo de respuesta
            </h2>
            <p className='text-sm text-[#9aa1b1]'>
              Hasta 48 horas hábiles
              <br />
              para consultas generales
              <br />
              hasta 24 horas para pagos
            </p>
          </div>
        </div>

        <section className='rounded-2xl border border-white/10 bg-white/[0.02] p-8'>
          <h2 className='mb-6 text-2xl font-bold text-white'>
            Envíanos un mensaje
          </h2>

          <form onSubmit={handleSubmit} className='space-y-5'>
            {/* Honeypot anti-spam: oculto visualmente pero presente en el DOM */}
            <div className='hidden' aria-hidden='true'>
              <label htmlFor='website'>
                No llenes este campo si eres humano
              </label>
              <input
                type='text'
                id='website'
                name='website'
                tabIndex={-1}
                autoComplete='off'
                value={form.website}
                onChange={handleChange}
              />
            </div>

            <div className='grid gap-5 sm:grid-cols-2'>
              <div>
                <label htmlFor='nombre' className={labelClass}>
                  Nombre completo
                </label>
                <input
                  id='nombre'
                  name='nombre'
                  type='text'
                  required
                  maxLength={100}
                  value={form.nombre}
                  onChange={handleChange}
                  placeholder='Tu nombre'
                  className={inputClass}
                />
              </div>

              <div>
                <label htmlFor='email' className={labelClass}>
                  Correo electrónico
                </label>
                <input
                  id='email'
                  name='email'
                  type='email'
                  required
                  maxLength={150}
                  value={form.email}
                  onChange={handleChange}
                  placeholder='tucorreo@ejemplo.com'
                  className={inputClass}
                />
              </div>
            </div>

            <div>
              <label htmlFor='asunto' className={labelClass}>
                Asunto
              </label>
              <select
                id='asunto'
                name='asunto'
                required
                value={form.asunto}
                onChange={handleChange}
                className={inputClass}
              >
                <option value=''>Selecciona un asunto...</option>
                {ASUNTOS_PREDEFINIDOS.map((a) => (
                  <option key={a} value={a}>
                    {a}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor='mensaje' className={labelClass}>
                Mensaje
              </label>
              <textarea
                id='mensaje'
                name='mensaje'
                required
                maxLength={2000}
                rows={6}
                value={form.mensaje}
                onChange={handleChange}
                placeholder='Escribe aquí tu mensaje...'
                className={`${inputClass} resize-y`}
              />
              <p className='mt-1 text-right text-xs text-white/40'>
                {form.mensaje.length}/2000
              </p>
            </div>

            <div>
              <button
                type='submit'
                disabled={enviarContacto.isPending}
                className='inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-60'
              >
                {enviarContacto.isPending ? (
                  <>
                    <FaSpinner className='animate-spin' />
                    Enviando...
                  </>
                ) : (
                  'Enviar mensaje'
                )}
              </button>
            </div>
          </form>
        </section>

        <section className='rounded-2xl border border-white/10 bg-white/[0.02] p-8'>
          <h2 className='mb-6 text-2xl font-bold text-white'>Temas frecuentes</h2>

          <div className='space-y-6 text-[#cdd3e0]'>
            <div>
              <h3 className='mb-1 font-semibold text-white'>
                Pagos y suscripciones
              </h3>
              <p className='text-sm'>
                Dudas sobre tu plan, facturación, cancelación, reembolsos.
                Incluye tu correo de cuenta y el ID de orden de PayPal si
                aplica. Consulta también nuestra{' '}
                <Link
                  to='/reembolso'
                  className='text-blue-400 hover:text-blue-300 underline'
                >
                  Política de Reembolso
                </Link>
                .
              </p>
            </div>

            <div>
              <h3 className='mb-1 font-semibold text-white'>Problemas técnicos</h3>
              <p className='text-sm'>
                Error al publicar un anuncio, no recibí notificaciones,
                problemas para iniciar sesión. Indica captura del error,
                navegador y pasos para reproducirlo.
              </p>
            </div>

            <div>
              <h3 className='mb-1 font-semibold text-white'>
                Verificación de cuenta
              </h3>
              <p className='text-sm'>
                Si eres Dealer y necesitas verificar tu cuenta para desbloquear
                funciones avanzadas, envía tus documentos desde tu panel en{' '}
                <Link
                  to='/dashboard/mi-perfil'
                  className='text-blue-400 hover:text-blue-300 underline'
                >
                  Mi Perfil
                </Link>
                .
              </p>
            </div>

            <div>
              <h3 className='mb-1 font-semibold text-white'>
                Disputas entre usuarios
              </h3>
              <p className='text-sm'>
                AutoMarket RD actúa como intermediario y no participa en
                contratos de compraventa. Para disputas sobre vehículos,
                contacta directamente al Dealer o a las autoridades competentes.
              </p>
            </div>
          </div>
        </section>
      </article>
    </LayoutPublico>
  );
}
