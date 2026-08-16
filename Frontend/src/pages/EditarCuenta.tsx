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

function useEditarCuenta() {
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
    if (actualizarDatos.isPending) return;
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
    if (cambiarPassword.isPending) return;
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
    if (confirmarCambioPassword.isPending) return;
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
    if (solicitarCambioEmail.isPending) return;
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
    if (confirmarCambioEmail.isPending) return;
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

  return {
    cuenta,
    cargando,
    enviando,
    nombre,
    apellido,
    telefono,
    passwordActual,
    nuevaPassword,
    repetirPassword,
    pasoCodigoPassword,
    codigoPassword,
    pasoCodigo,
    passwordEmail,
    nuevoEmail,
    codigo,
    setNombre,
    setApellido,
    setTelefono,
    setPasswordActual,
    setNuevaPassword,
    setRepetirPassword,
    setPasoCodigoPassword,
    setCodigoPassword,
    setPasoCodigo,
    setPasswordEmail,
    setNuevoEmail,
    setCodigo,
    guardarDatos,
    solicitarCodigoPassword,
    confirmarCodigoPassword,
    solicitarCodigo,
    confirmarCodigo,
  };
}

const inputClase =
  'w-full rounded-lg border border-line bg-input px-3 py-2.5 text-sm placeholder-gray-500 transition-colors focus:border-blue-500 focus:outline-none';
const labelClase = 'mb-1 block text-xs font-medium text-ink-2';

interface PropsDatos {
  nombre: string;
  apellido: string;
  telefono: string;
  enviando: boolean;
  onChangeNombre: (valor: string) => void;
  onChangeApellido: (valor: string) => void;
  onChangeTelefono: (valor: string) => void;
  onGuardar: () => void;
}

function SeccionDatosPersonales({
  nombre,
  apellido,
  telefono,
  enviando,
  onChangeNombre,
  onChangeApellido,
  onChangeTelefono,
  onGuardar,
}: PropsDatos) {
  return (
    <section className="rounded-2xl border border-line bg-surface p-6">
      <h2 className="flex items-center gap-2 text-lg font-bold">
        <FaUser className="text-blue-400" />
        Datos personales
      </h2>

      <div className="mt-5 grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="nombre" className={labelClase}>Nombre *</label>
          <input
            id="nombre"
            type="text"
            maxLength={100}
            value={nombre}
            onChange={(e) => onChangeNombre(e.target.value)}
            className={inputClase}
          />
        </div>
        <div>
          <label htmlFor="apellido" className={labelClase}>Apellido *</label>
          <input
            id="apellido"
            type="text"
            maxLength={100}
            value={apellido}
            onChange={(e) => onChangeApellido(e.target.value)}
            className={inputClase}
          />
        </div>
        <div>
          <label htmlFor="telefono" className={labelClase}>Teléfono</label>
          <input
            id="telefono"
            type="tel"
            maxLength={20}
            value={telefono}
            onChange={(e) => onChangeTelefono(e.target.value)}
            className={inputClase}
          />
        </div>
      </div>

      <button
        type="button"
        onClick={onGuardar}
        disabled={enviando}
        className="mt-5 inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
      >
        <FaSave />
        Guardar cambios
      </button>
    </section>
  );
}

interface PropsPassword {
  pasoCodigo: boolean;
  passwordActual: string;
  nuevaPassword: string;
  repetirPassword: string;
  codigo: string;
  enviando: boolean;
  onChangePasswordActual: (valor: string) => void;
  onChangeNuevaPassword: (valor: string) => void;
  onChangeRepetirPassword: (valor: string) => void;
  onChangeCodigo: (valor: string) => void;
  onSolicitar: () => void;
  onConfirmar: () => void;
  onVolver: () => void;
}

function SeccionPassword({
  pasoCodigo,
  passwordActual,
  nuevaPassword,
  repetirPassword,
  codigo,
  enviando,
  onChangePasswordActual,
  onChangeNuevaPassword,
  onChangeRepetirPassword,
  onChangeCodigo,
  onSolicitar,
  onConfirmar,
  onVolver,
}: PropsPassword) {
  return (
    <section className="mt-6 rounded-2xl border border-line bg-surface p-6">
      <h2 className="flex items-center gap-2 text-lg font-bold">
        <FaKey className="text-yellow-400" />
        Cambiar contraseña
      </h2>
      <p className="mt-1 text-sm text-ink-2">
        {pasoCodigo
          ? 'Paso 2 de 2: ingresa el código que enviamos a tu correo.'
          : 'Paso 1 de 2: confirma con tu contraseña actual. Recibirás un código en tu correo.'}
      </p>

      {!pasoCodigo ? (
        <div className="mt-5 space-y-4">
          <div>
            <label htmlFor="passwordActual" className={labelClase}>Contraseña actual *</label>
            <input
              id="passwordActual"
              type="password"
              value={passwordActual}
              onChange={(e) => onChangePasswordActual(e.target.value)}
              className={inputClase}
            />
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="nuevaPassword" className={labelClase}>Nueva contraseña *</label>
              <input
                id="nuevaPassword"
                type="password"
                value={nuevaPassword}
                onChange={(e) => onChangeNuevaPassword(e.target.value)}
                className={inputClase}
              />
            </div>
            <div>
              <label htmlFor="repetirPassword" className={labelClase}>Repetir nueva contraseña *</label>
              <input
                id="repetirPassword"
                type="password"
                value={repetirPassword}
                onChange={(e) => onChangeRepetirPassword(e.target.value)}
                className={inputClase}
              />
            </div>
          </div>

          <button
            type="button"
            onClick={onSolicitar}
            disabled={enviando}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <FaEnvelopeOpenText />
            Enviar código a mi correo
          </button>
        </div>
      ) : (
        <div className="mt-5 space-y-4">
          <div>
            <label htmlFor="codigoPassword" className={labelClase}>
              Código de confirmación (6 dígitos) *
            </label>
            <input
              id="codigoPassword"
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={codigo}
              onChange={(e) => onChangeCodigo(e.target.value.replace(/\D/g, ''))}
              className={`${inputClase} tracking-[0.5em]`}
            />
          </div>
          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={onConfirmar}
              disabled={enviando}
              className="inline-flex items-center gap-2 rounded-lg bg-green-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <FaLock />
              Confirmar cambio
            </button>
            <button
              type="button"
              onClick={onVolver}
              disabled={enviando}
              className="rounded-lg border border-line px-5 py-2.5 text-sm text-ink-2 transition-colors hover:text-ink"
            >
              Volver
            </button>
          </div>
        </div>
      )}
    </section>
  );
}

interface PropsCorreo {
  cuentaEmail: string | undefined;
  emailConfirmado: boolean;
  pasoCodigo: boolean;
  passwordEmail: string;
  nuevoEmail: string;
  codigo: string;
  enviando: boolean;
  onChangePasswordEmail: (valor: string) => void;
  onChangeNuevoEmail: (valor: string) => void;
  onChangeCodigo: (valor: string) => void;
  onSolicitar: () => void;
  onConfirmar: () => void;
  onVolver: () => void;
}

function SeccionCorreo({
  cuentaEmail,
  emailConfirmado,
  pasoCodigo,
  passwordEmail,
  nuevoEmail,
  codigo,
  enviando,
  onChangePasswordEmail,
  onChangeNuevoEmail,
  onChangeCodigo,
  onSolicitar,
  onConfirmar,
  onVolver,
}: PropsCorreo) {
  return (
    <section className="mt-6 rounded-2xl border border-line bg-surface p-6">
      <h2 className="flex items-center gap-2 text-lg font-bold">
        <FaEnvelope className="text-green-400" />
        Cambiar correo electrónico
      </h2>
      <p className="mt-1 text-sm text-ink-2">
        {pasoCodigo
          ? 'Paso 2 de 2: ingresa el código que enviamos al nuevo correo.'
          : 'Paso 1 de 2: confirma con tu contraseña y recibe un código en el nuevo correo.'}
      </p>
      <p className="mt-1 text-sm text-ink-2">
        Correo actual: <strong className="text-ink-2">{cuentaEmail}</strong>
        {emailConfirmado ? (
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
              <label htmlFor="passwordEmail" className={labelClase}>Contraseña actual *</label>
              <input
                id="passwordEmail"
                type="password"
                value={passwordEmail}
                onChange={(e) => onChangePasswordEmail(e.target.value)}
                className={inputClase}
              />
            </div>
            <div>
              <label htmlFor="nuevoEmail" className={labelClase}>Nuevo correo *</label>
              <input
                id="nuevoEmail"
                type="email"
                maxLength={150}
                value={nuevoEmail}
                onChange={(e) => onChangeNuevoEmail(e.target.value)}
                className={inputClase}
              />
            </div>
            <button
              type="button"
              onClick={onSolicitar}
              disabled={enviando}
              className="inline-flex items-center gap-2 rounded-lg bg-blue-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-600 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <FaEnvelopeOpenText />
              Enviar código al nuevo correo
            </button>
          </>
        ) : (
          <>
            <div>
              <label htmlFor="codigoEmail" className={labelClase}>
                Código de confirmación (6 dígitos) *
              </label>
              <input
                id="codigoEmail"
                type="text"
                inputMode="numeric"
                maxLength={6}
                value={codigo}
                onChange={(e) => onChangeCodigo(e.target.value.replace(/\D/g, ''))}
                className={`${inputClase} tracking-[0.5em]`}
              />
            </div>
            <div className="flex flex-wrap gap-3">
              <button
                type="button"
                onClick={onConfirmar}
                disabled={enviando}
                className="inline-flex items-center gap-2 rounded-lg bg-green-500 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <FaLock />
                Confirmar cambio
              </button>
              <button
                type="button"
                onClick={onVolver}
                disabled={enviando}
                className="rounded-lg border border-line px-5 py-2.5 text-sm text-ink-2 transition-colors hover:text-ink"
              >
                Volver
              </button>
            </div>
          </>
        )}
      </div>
    </section>
  );
}

export default function EditarCuenta() {
  const {
    cuenta,
    cargando,
    enviando,
    nombre,
    apellido,
    telefono,
    passwordActual,
    nuevaPassword,
    repetirPassword,
    pasoCodigoPassword,
    codigoPassword,
    pasoCodigo,
    passwordEmail,
    nuevoEmail,
    codigo,
    setNombre,
    setApellido,
    setTelefono,
    setPasswordActual,
    setNuevaPassword,
    setRepetirPassword,
    setPasoCodigoPassword,
    setCodigoPassword,
    setPasoCodigo,
    setPasswordEmail,
    setNuevoEmail,
    setCodigo,
    guardarDatos,
    solicitarCodigoPassword,
    confirmarCodigoPassword,
    solicitarCodigo,
    confirmarCodigo,
  } = useEditarCuenta();

  if (cargando) {
    return <Spinner />;
  }

  return (
    <div className="min-h-screen bg-page text-ink">
      <header className="flex items-center justify-between border-b border-line px-6 py-5 sm:px-8">
        <Link
          to="/perfil"
          className="inline-flex items-center gap-2 text-sm font-medium text-ink-2 transition-colors hover:text-ink"
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
          <p className="mt-1 text-sm text-ink-2">
            Actualiza tus datos personales, contraseña y correo electrónico.
          </p>
        </div>

        <SeccionDatosPersonales
          nombre={nombre}
          apellido={apellido}
          telefono={telefono}
          enviando={enviando}
          onChangeNombre={setNombre}
          onChangeApellido={setApellido}
          onChangeTelefono={setTelefono}
          onGuardar={guardarDatos}
        />

        <SeccionPassword
          pasoCodigo={pasoCodigoPassword}
          passwordActual={passwordActual}
          nuevaPassword={nuevaPassword}
          repetirPassword={repetirPassword}
          codigo={codigoPassword}
          enviando={enviando}
          onChangePasswordActual={setPasswordActual}
          onChangeNuevaPassword={setNuevaPassword}
          onChangeRepetirPassword={setRepetirPassword}
          onChangeCodigo={setCodigoPassword}
          onSolicitar={solicitarCodigoPassword}
          onConfirmar={confirmarCodigoPassword}
          onVolver={() => {
            setPasoCodigoPassword(false);
            setCodigoPassword('');
          }}
        />

        <SeccionCorreo
          cuentaEmail={cuenta?.email}
          emailConfirmado={cuenta?.emailConfirmado === true}
          pasoCodigo={pasoCodigo}
          passwordEmail={passwordEmail}
          nuevoEmail={nuevoEmail}
          codigo={codigo}
          enviando={enviando}
          onChangePasswordEmail={setPasswordEmail}
          onChangeNuevoEmail={setNuevoEmail}
          onChangeCodigo={setCodigo}
          onSolicitar={solicitarCodigo}
          onConfirmar={confirmarCodigo}
          onVolver={() => {
            setPasoCodigo(false);
            setCodigo('');
          }}
        />
      </main>
    </div>
  );
}