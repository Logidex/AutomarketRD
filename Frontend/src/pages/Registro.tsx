import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import Swal from "sweetalert2";
import { MdVisibility, MdVisibilityOff } from "react-icons/md";
import { authService } from "../services/auth.service";
import logo from "../assets/AutoMarketRD_Logo.svg";

interface RegistroFormData {
  nombre: string;
  apellido: string;
  email: string;
  password: string;
  rol: string;
  telefonoPersonal: string;
  nombreAgencia: string;
  agenciaRNC: string;
  ubicacionAgencia: string;
  telefonoAgencia: string;
}

interface CamposDealerProps {
  formData: RegistroFormData;
  handleChange: (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => void;
}

function CamposDealer({ formData, handleChange }: CamposDealerProps) {
  return (
    <div className="border-t border-line pt-5 mt-3">
      <h3 className="text-lg font-semibold text-blue-500 mb-4">
        Información de la Agencia
      </h3>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label
            htmlFor="nombreAgencia"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            Nombre de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="nombreAgencia"
            type="text"
            name="nombreAgencia"
            value={formData.nombreAgencia}
            onChange={handleChange}
            placeholder="AutoVentas RD"
            className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
            required
          />
        </div>
        <div>
          <label
            htmlFor="agenciaRNC"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            RNC de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="agenciaRNC"
            type="text"
            name="agenciaRNC"
            value={formData.agenciaRNC}
            onChange={handleChange}
            placeholder="1-30-12345-6"
            className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
            required
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
        <div>
          <label
            htmlFor="ubicacionAgencia"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            Ubicación de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="ubicacionAgencia"
            type="text"
            name="ubicacionAgencia"
            value={formData.ubicacionAgencia}
            onChange={handleChange}
            placeholder="Santo Domingo"
            className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
            required
          />
        </div>
        <div>
          <label
            htmlFor="telefonoAgencia"
            className="block text-sm font-medium text-ink-2 mb-2"
          >
            Teléfono de la Agencia <span className="text-red-500">*</span>
          </label>
          <input
            id="telefonoAgencia"
            type="tel"
            name="telefonoAgencia"
            value={formData.telefonoAgencia}
            onChange={handleChange}
            placeholder="809-555-5555"
            className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
            required
          />
        </div>
      </div>
    </div>
  );
}

export default function Registro() {
  const [formData, setFormData] = useState<RegistroFormData>({
    nombre: "",
    apellido: "",
    email: "",
    password: "",
    rol: "Comprador",
    telefonoPersonal: "",
    nombreAgencia: "",
    agenciaRNC: "",
    ubicacionAgencia: "",
    telefonoAgencia: "",
  });

  const [loading, setLoading] = useState(false);
  const [mostrarPassword, setMostrarPassword] = useState(false);
  const [confirmarPassword, setConfirmarPassword] = useState("");
  const [mostrarConfirmacion, setMostrarConfirmacion] = useState(false);
  const navigate = useNavigate();

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== confirmarPassword) {
      await Swal.fire({
        icon: "error",
        title: "Las contraseñas no coinciden",
        text: "Verifica que ambos campos de contraseña sean iguales.",
        confirmButtonColor: "#3b82f6",
      });
      return;
    }

    setLoading(true);

    try {
      const response = await authService.register(formData);

      if (response.exito) {
        const esDealerRegistrado = formData.rol === "Dealer";

        if (esDealerRegistrado) {
          try {
            await authService.login({
              email: formData.email,
              password: formData.password,
            });
          } catch {
            // Si el auto-login falla, la página de suscripción lo redirigirá a /login
          }

          await Swal.fire({
            icon: "success",
            title: "¡Registro exitoso!",
            html: `Tu cuenta Dealer fue creada con el plan <strong>Gratis</strong> (1 anuncio).<br/><br/>Te enviamos un correo de confirmación a <strong>${formData.email}</strong>. Confírmalo para poder obtener la insignia de <strong>Dealer Verificado</strong>.<br/><br/>⭐ <strong>No te pierdas este paso:</strong> entra a tu panel y completa <strong>Mi Perfil</strong> con el logo, horarios y descripción de tu agencia — los compradores confían más en perfiles completos.<br/><br/>Ahora elige la suscripción que mejor se adapte a tu agencia.`,
            confirmButtonColor: "#3b82f6",
          });
          navigate("/suscripcion");
        } else {
          await Swal.fire({
            icon: "success",
            title: "¡Registro exitoso!",
            text: response.mensaje,
            confirmButtonColor: "#3b82f6",
          });
          navigate("/login");
        }
      } else {
        await Swal.fire({
          icon: "error",
          title: "Error al registrarse",
          text: response.mensaje,
          confirmButtonColor: "#3b82f6",
        });
      }
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error al registrarse",
        text: err instanceof Error ? err.message : "Error en el servidor",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setLoading(false);
    }
  };

  const esDealer = formData.rol === "Dealer";

  return (
    <div className="min-h-screen bg-page flex items-center justify-center p-4">
      <div className="w-full max-w-[950px] bg-surface rounded-2xl shadow-2xl overflow-hidden flex flex-col md:flex-row">

        {/* Columna Izquierda - Visual */}
        <div className="md:flex-1 bg-[#11141a] p-12 text-white flex flex-col">
          <div className="mb-10">
            <div className="flex items-center justify-center w-full">
              <img
                src={logo}
                alt="AutoMarket RD"
                className="w-48 md:w-56 object-contain"
              />
            </div>
          </div>

          <h1 className="text-3xl font-bold mb-4">
            Únete al mercado
          </h1>
          <p className="text-white/70 mb-10">
            Crea tu cuenta y comienza a explorar o publicar vehículos en
            AutoMarket RD.
          </p>

          <div className="flex-1 flex items-center justify-center">
            <div className="w-40 h-40 rounded-full bg-gradient-to-br from-blue-500/20 to-transparent flex items-center justify-center relative">
              <div className="w-36 h-36 border-2 border-white/5 border-t-blue-500 rounded-full animate-spin"></div>
            </div>
          </div>
        </div>

        {/* Columna Derecha - Formulario */}
        <div className="md:flex-[1.2] bg-surface p-12 flex flex-col justify-center">
          <h2 className="text-2xl font-semibold mb-6 text-ink">
            Crear Cuenta
          </h2>

          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Nombre y Apellido */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="nombre"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Nombre
                </label>
                <input
                  id="nombre"
                  type="text"
                  name="nombre"
                  value={formData.nombre}
                  onChange={handleChange}
                  placeholder="Juan"
                  className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                  required
                />
              </div>
              <div>
                <label
                  htmlFor="apellido"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Apellido
                </label>
                <input
                  id="apellido"
                  type="text"
                  name="apellido"
                  value={formData.apellido}
                  onChange={handleChange}
                  placeholder="Pérez"
                  className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                  required
                />
              </div>
            </div>

            {/* Email y Teléfono */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="email"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Email
                </label>
                <input
                  id="email"
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="tu@email.com"
                  className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                  required
                />
              </div>
              <div>
                <label
                  htmlFor="telefonoPersonal"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Teléfono Personal
                </label>
                <input
                  id="telefonoPersonal"
                  type="tel"
                  name="telefonoPersonal"
                  value={formData.telefonoPersonal}
                  onChange={handleChange}
                  placeholder="809-555-5555"
                  className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                />
              </div>
            </div>

            {/* Contraseña y Confirmación */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="password"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Contraseña
                </label>
                <div className="relative">
                  <input
                    id="password"
                    type={mostrarPassword ? "text" : "password"}
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    placeholder="••••••••"
                    className="w-full px-4 py-3 pr-12 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                    required
                    minLength={6}
                  />
                  <button
                    type="button"
                    onClick={() => setMostrarPassword((v) => !v)}
                    aria-label={mostrarPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
                    className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarPassword ? "text-blue-500" : "text-ink-3"}`}
                  >
                    {mostrarPassword ? <MdVisibilityOff /> : <MdVisibility />}
                  </button>
                </div>
              </div>
              <div>
                <label
                  htmlFor="confirmarPassword"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Confirmar Contraseña
                </label>
                <div className="relative">
                  <input
                    id="confirmarPassword"
                    type={mostrarConfirmacion ? "text" : "password"}
                    value={confirmarPassword}
                    onChange={(e) => setConfirmarPassword(e.target.value)}
                    placeholder="••••••••"
                    className="w-full px-4 py-3 pr-12 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                    required
                    minLength={6}
                  />
                  <button
                    type="button"
                    onClick={() => setMostrarConfirmacion((v) => !v)}
                    aria-label={mostrarConfirmacion ? "Ocultar contraseña" : "Mostrar contraseña"}
                    className={`absolute right-3 top-1/2 -translate-y-1/2 transition-colors hover:text-blue-500 ${mostrarConfirmacion ? "text-blue-500" : "text-ink-3"}`}
                  >
                    {mostrarConfirmacion ? <MdVisibilityOff /> : <MdVisibility />}
                  </button>
                </div>
              </div>
            </div>

            {/* Tipo de Cuenta */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label
                  htmlFor="rol"
                  className="block text-sm font-medium text-ink-2 mb-2"
                >
                  Tipo de Cuenta
                </label>
                <select
                  id="rol"
                  name="rol"
                  value={formData.rol}
                  onChange={handleChange}
                  className="w-full px-4 py-3 bg-input text-ink border border-line rounded-lg focus:outline-none focus:border-blue-500 transition-colors placeholder:text-ink-3"
                  required
                >
                  <option value="Comprador">Comprador</option>
                  <option value="Vendedor">Vendedor</option>
                  <option value="Dealer">Dealer</option>
                </select>
              </div>
            </div>

            {/* Campos exclusivos para Dealer */}
            {esDealer && (
              <CamposDealer formData={formData} handleChange={handleChange} />
            )}

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-blue-500 text-white rounded-lg font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors mt-4"
            >
              {loading ? "Registrando..." : "Registrarse"}
            </button>
          </form>

          <p className="text-center text-sm text-ink-3 mt-6">
            ¿Ya tienes cuenta?{" "}
            <Link to="/login" className="text-blue-500 font-semibold hover:underline">
              Inicia sesión aquí
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}