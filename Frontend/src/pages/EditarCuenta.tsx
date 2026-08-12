import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Swal from 'sweetalert2';
import {
  FaArrowLeft,
  FaEnvelope,
  FaEnvelopeOpenText,
  FaKey,
  FaLock,
  FaSave,
  FaUser,
  FaUserEdit,
} from 'react-icons/fa';
import { authService } from '../services/auth.service';
import Spinner from '../components/Spinner';
import {
  useUsuarioCuenta,
  useActualizarDatos,
  useCambiarPassword,
  useConfirmarCambioPassword,
  useSolicitarCambioEmail,
  useConfirmarCambioEmail,
} from '../hooks/useUsuario';

const PASSWORD_MIN = 6;

export default function EditarCuenta() {
  const { data: cuenta, isLoading: cargando } = useUsuarioCuenta();
  const actualizarDatos = useActualizarDatos();
  const cambiarPassword = useCambiarPassword();
  const confirmarCambioPassword = useConfirmarCambioPassword();
  const solicitarCambioEmail = useSolicitarCambioEmail();
  const confirmarCambioEmail = useConfirmarCambioEmail();

  // Datos personales
  const [nombre, setNombre] = useState('');
  const [apellido, setApellido] = useState('');
  const [telefono, setTelefono] = useState('');

  // Contraseña
  const [passwordActual, setPasswordActual] = useState('');
  const [nuevaPassword, setNuevaPassword] = useState('');
  const [repetirPassword, setRepetirPassword] = useState('');
  const [pasoCodigoPassword, setPasoCodigoPassword] = useState(false);
  const [codigoPassword, setCodigoPassword] = useState('');

  // Cambio de correo (dos pasos)
  const [pasoCodigo, setPasoCodigo] = useState(false);
  const [passwordEmail, setPasswordEmail] = useState('');
  const [nuevoEmail, setNuevoEmail] = useState('');
  const [codigo, setCodigo] = useState('');

  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    if (!cuenta) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setNombre(cuenta.nombre);
    setApellido(cuenta.apellido);
    setTelefono(cuenta.telefonoPersonal ?? '');
  }, [cuenta]);

  const guardarDatos = async () => {
    if (nombre.trim().length === 0 || apellido.trim().length === 0) {
      Swal.fire(
        'Datos incompletos',
        'Nombre y apellido son obligatorios.',
        'warning'
      );
      return;
    }

    setEnviando(true);
    try {
      const actualizada = await actualizarDatos.mutateAsync({
        nombre: nombre.trim(),
        apellido: apellido.trim(),
        telefonoPersonal: telefono.trim() || null,
      });

      authService.actualizarUsuario({
        nombre: actualizada.nombre,
        apellido: actualizada.apellido,
        email: actualizada.email,
      });
      Swal.fire('Guardado', 'Tus datos fueron actualizados.', 'success');
    } catch (err) {
      Swal.fire(
        'Error',
        err instanceof Error ? err.message : 'No se pudieron guardar los datos.',
        'error'
      );
    } finally {
      setEnviando(false);
    }
  };

  const solicitarCodigoPassword = async () => {
    if (!passwordActual || !nuevaPassword) {
      Swal.fire(
        'Campos incompletos',
        'Completa la contraseña actual y la nueva.',
        'warning'
      );
      return;
    }

    if (nuevaPassword.length < PASSWORD_MIN) {
      Swal.fire(
        'Contraseña corta',
        `La nueva contraseña debe tener al menos ${PASSWORD_MIN} caracteres.`,
        'warning'
      );
      return;
    }

    if (nuevaPassword !== repetirPassword) {
      Swal.fire(
        'No coinciden',
        'La nueva contraseña y su confirmación no coinciden.',
        'warning'
      );
      return;
    }

    setEnviando(true);
    try {
      const resultado = await cambiarPassword.mutateAsync({
        passwordActual,
        nuevaPassword,
      });
      Swal.fire('Código enviado', resultado.mensaje, 'success');
      setPasoCodigoPassword(true);
    } catch (err) {
      Swal.fire(
        'Error',
        err instanceof Error
          ? err.message
          : 'No se pudo solicitar el cambio.',
        'error'
      );
    } finally {
      setEnviando(false);
    }
  };

  const confirmarCodigoPassword = async () => {
    if (codigoPassword.trim().length < 6) {
      Swal.fire(
        'Código incompleto',
        'Ingresa el código de 6 dígitos que recibiste.',
        'warning'
      );
      return;
    }

    setEnviando(true);
    try {
      const resultado = await confirmarCambioPassword.mutateAsync(
        codigoPassword.trim()
      );
      Swal.fire('Contraseña actualizada', resultado.mensaje, 'success');
      setPasoCodigoPassword(false);
      setCodigoPassword('');
      setPasswordActual('');
      setNuevaPassword('');
      setRepetirPassword('');
    } catch (err) {
      Swal.fire(
        'Error',
        err instanceof Error
          ? err.message
          : 'El código no pudo confirmarse.',
        'error'
      );
    } finally {
      setEnviando(false);
    }
  };

  const solicitarCodigo = async () => {
    if (!passwordEmail || !nuevoEmail) {
      Swal.fire(
        'Campos incompletos',
        'Ingresa tu contraseña actual y el nuevo correo.',
        'warning'
      );
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(nuevoEmail)) {
      Swal.fire('Correo inválido', 'Ingresa un correo electrónico válido.', 'warning');
      return;
    }

    setEnviando(true);
    try {
      const resultado = await solicitarCambioEmail.mutateAsync({
        passwordActual: passwordEmail,
        nuevoEmail: nuevoEmail.trim(),
      });
      Swal.fire('Código enviado', resultado.mensaje, 'success');
      setPasoCodigo(true);
    } catch (err) {
      Swal.fire(
        'Error',
        err instanceof Error
          ? err.message
          : 'No se pudo solicitar el cambio.',
        'error'
      );
    } finally {
      setEnviando(false);
    }
  };

  const confirmarCodigo = async () => {
    if (codigo.trim().length < 6) {
      Swal.fire(
        'Código incompleto',
        'Ingresa el código de 6 dígitos que recibiste.',
        'warning'
      );
      return;
    }

    setEnviando(true);
    try {
      const resultado = await confirmarCambioEmail.mutateAsync(
        codigo.trim()
      );
      const emailNuevo = nuevoEmail.trim();

      authService.actualizarUsuario({ email: emailNuevo });

      Swal.fire('Correo actualizado', resultado.mensaje, 'success');
      setPasoCodigo(false);
      setCodigo('');
      setPasswordEmail('');
      setNuevoEmail('');
    } catch (err) {
      Swal.fire(
        'Error',
        err instanceof Error
          ? err.message
          : 'El código no pudo confirmarse.',
        'error'
      );
    } finally {
      setEnviando(false);
    }
  };

  if (cargando) {
    return <Spinner />;
  }

  const inputClase =
    'w-full rounded-lg border border-white/10 bg-[#0c101b] px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none';
  const labelClase = 'mb-1 block text-xs font-medium text-[#9aa1b1]';

  return (
    <div className="min-h-screen bg-[#0c101b] text-white">
      <header className="flex items-center justify-between border-b border-white/10 px-6 py-5 sm:px-8">
        <Link
          to="/perfil"
          className="inline-flex items-center gap-2 text-sm font-medium text-[#9aa1b1] transition-colors hover:text-white"
        >
          <FaArrowLeft />
          Mi cuenta
        </Link>
      </header>

      <main className="mx-auto max-w-2xl px-6 py-10 sm:px-8">
        <div className="mb-8">
          <h1 className="flex items-center gap-3 text-2xl font-bold">
            <FaUserEdit className="text-blue-400" />
            Editar mi cuenta
          </h1>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            Actualiza tus datos personales, contraseña y correo electrónico.
          </p>
        </div>

        {/* SECCIÓN 1: DATOS PERSONALES */}
        <section className="rounded-2xl border border-white/10 bg-[#13161d] p-6">
          <h2 className="flex items-center gap-2 text-lg font-bold">
            <FaUser className="text-blue-400" />
            Datos personales
          </h2>

          <div className="mt-5 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className={labelClase}>Nombre *</label>
              <input
                type="text"
                maxLength={100}
                value={nombre}
                onChange={(e) => setNombre(e.target.value)}
                className={inputClase}
              />
            </div>
            <div>
              <label className={labelClase}>Apellido *</label>
              <input
                type="text"
                maxLength={100}
                value={apellido}
                onChange={(e) => setApellido(e.target.value)}
                className={inputClase}
              />
            </div>
            <div>
              <label className={labelClase}>Teléfono</label>
              <input
                type="tel"
                maxLength={20}
                value={telefono}
                onChange={(e) => setTelefono(e.target.value)}
                className={inputClase}
              />
            </div>
          </div>

          <button
            type="button"
            onClick={guardarDatos}
            disabled={enviando}
            className="mt-5 inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <FaSave />
            Guardar cambios
          </button>
        </section>

        {/* SECCIÓN 2: CONTRASEÑA */}
        <section className="mt-6 rounded-2xl border border-white/10 bg-[#13161d] p-6">
          <h2 className="flex items-center gap-2 text-lg font-bold">
            <FaKey className="text-yellow-400" />
            Cambiar contraseña
          </h2>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            {pasoCodigoPassword
              ? 'Paso 2 de 2: ingresa el código que enviamos a tu correo.'
              : 'Paso 1 de 2: confirma con tu contraseña actual. Recibirás un código en tu correo.'}
          </p>

          {!pasoCodigoPassword ? (
            <div className="mt-5 space-y-4">
              <div>
                <label className={labelClase}>Contraseña actual *</label>
                <input
                  type="password"
                  value={passwordActual}
                  onChange={(e) => setPasswordActual(e.target.value)}
                  className={inputClase}
                />
              </div>
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label className={labelClase}>Nueva contraseña *</label>
                  <input
                    type="password"
                    value={nuevaPassword}
                    onChange={(e) => setNuevaPassword(e.target.value)}
                    className={inputClase}
                  />
                </div>
                <div>
                  <label className={labelClase}>Repetir nueva contraseña *</label>
                  <input
                    type="password"
                    value={repetirPassword}
                    onChange={(e) => setRepetirPassword(e.target.value)}
                    className={inputClase}
                  />
                </div>
              </div>

              <button
                type="button"
                onClick={solicitarCodigoPassword}
                disabled={enviando}
                className="inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <FaEnvelopeOpenText />
                Enviar código a mi correo
              </button>
            </div>
          ) : (
            <div className="mt-5 space-y-4">
              <div>
                <label className={labelClase}>
                  Código de confirmación (6 dígitos) *
                </label>
                <input
                  type="text"
                  inputMode="numeric"
                  maxLength={6}
                  value={codigoPassword}
                  onChange={(e) =>
                    setCodigoPassword(e.target.value.replace(/\D/g, ''))
                  }
                  className={`${inputClase} tracking-[0.5em]`}
                />
              </div>
              <div className="flex flex-wrap gap-3">
                <button
                  type="button"
                  onClick={confirmarCodigoPassword}
                  disabled={enviando}
                  className="inline-flex items-center gap-2 rounded-lg bg-green-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <FaLock />
                  Confirmar cambio
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setPasoCodigoPassword(false);
                    setCodigoPassword('');
                  }}
                  disabled={enviando}
                  className="rounded-lg border border-white/10 px-5 py-2.5 text-sm text-[#9aa1b1] transition-colors hover:text-white"
                >
                  Volver
                </button>
              </div>
            </div>
          )}
        </section>

        {/* SECCIÓN 3: CORREO (DOS PASOS) */}
        <section className="mt-6 rounded-2xl border border-white/10 bg-[#13161d] p-6">
          <h2 className="flex items-center gap-2 text-lg font-bold">
            <FaEnvelope className="text-green-400" />
            Cambiar correo electrónico
          </h2>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            {pasoCodigo
              ? 'Paso 2 de 2: ingresa el código que enviamos al nuevo correo.'
              : 'Paso 1 de 2: confirma con tu contraseña y recibe un código en el nuevo correo.'}
          </p>
          <p className="mt-1 text-sm text-[#9aa1b1]">
            Correo actual: <strong className="text-gray-200">{cuenta?.email}</strong>
            {cuenta?.emailConfirmado ? (
              <span className="ml-2 rounded-full bg-green-500/10 px-2 py-0.5 text-xs text-green-400">
                confirmado
              </span>
            ) : (
              <span className="ml-2 rounded-full bg-yellow-500/10 px-2 py-0.5 text-xs text-yellow-400">
                sin confirmar
              </span>
            )}
          </p>

          <div className="mt-5 space-y-4">
            {!pasoCodigo ? (
              <>
                <div>
                  <label className={labelClase}>Contraseña actual *</label>
                  <input
                    type="password"
                    value={passwordEmail}
                    onChange={(e) => setPasswordEmail(e.target.value)}
                    className={inputClase}
                  />
                </div>
                <div>
                  <label className={labelClase}>Nuevo correo *</label>
                  <input
                    type="email"
                    maxLength={150}
                    value={nuevoEmail}
                    onChange={(e) => setNuevoEmail(e.target.value)}
                    className={inputClase}
                  />
                </div>
                <button
                  type="button"
                  onClick={solicitarCodigo}
                  disabled={enviando}
                  className="inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <FaEnvelopeOpenText />
                  Enviar código al nuevo correo
                </button>
              </>
            ) : (
              <>
                <div>
                  <label className={labelClase}>
                    Código de confirmación (6 dígitos) *
                  </label>
                  <input
                    type="text"
                    inputMode="numeric"
                    maxLength={6}
                    value={codigo}
                    onChange={(e) => setCodigo(e.target.value.replace(/\D/g, ''))}
                    className={`${inputClase} tracking-[0.5em]`}
                  />
                </div>
                <div className="flex flex-wrap gap-3">
                  <button
                    type="button"
                    onClick={confirmarCodigo}
                    disabled={enviando}
                    className="inline-flex items-center gap-2 rounded-lg bg-green-500 px-6 py-2.5 text-sm font-semibold transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    <FaLock />
                    Confirmar cambio
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setPasoCodigo(false);
                      setCodigo('');
                    }}
                    disabled={enviando}
                    className="rounded-lg border border-white/10 px-5 py-2.5 text-sm text-[#9aa1b1] transition-colors hover:text-white"
                  >
                    Volver
                  </button>
                </div>
              </>
            )}
          </div>
        </section>
      </main>
    </div>
  );
}