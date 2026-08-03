import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import Swal from "sweetalert2";
import { authService } from "../services/auth.service";
import logo from "../assets/AutoMarketRD_Logo.svg";

export default function Registro() {
  const [formData, setFormData] = useState({
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
    setLoading(true);

    try {
      const response = await authService.register(formData);

      if (response.exito) {
        await Swal.fire({
          icon: "success",
          title: "¡Registro exitoso!",
          text: response.mensaje,
          confirmButtonColor: "#3b82f6",
        });
        navigate("/login");
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
    <div className="min-h-screen bg-[#0c101b] flex items-center justify-center p-4">
      <div className="w-full max-w-[950px] bg-white rounded-2xl shadow-2xl overflow-hidden flex flex-col md:flex-row">
        
        {/* Columna Izquierda - Visual */}
        <div className="md:flex-1 bg-[#0e1422] p-12 text-white flex flex-col">
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
          <p className="text-[#9aa1b1] mb-10">
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
        <div className="md:flex-[1.2] bg-white p-12 flex flex-col justify-center">
          <h2 className="text-2xl font-semibold mb-6 text-gray-800">
            Crear Cuenta
          </h2>

          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Nombre y Apellido */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">
                  Nombre
                </label>
                <input
                  type="text"
                  name="nombre"
                  value={formData.nombre}
                  onChange={handleChange}
                  placeholder="Juan"
                  className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">
                  Apellido
                </label>
                <input
                  type="text"
                  name="apellido"
                  value={formData.apellido}
                  onChange={handleChange}
                  placeholder="Pérez"
                  className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                  required
                />
              </div>
            </div>

            {/* Email y Teléfono */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">
                  Email
                </label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="tu@email.com"
                  className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">
                  Teléfono Personal
                </label>
                <input
                  type="tel"
                  name="telefonoPersonal"
                  value={formData.telefonoPersonal}
                  onChange={handleChange}
                  placeholder="809-555-5555"
                  className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                />
              </div>
            </div>

            {/* Contraseña y Rol */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">
                  Contraseña
                </label>
                <input
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="••••••••"
                  className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                  required
                  minLength={6}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">
                  Tipo de Cuenta
                </label>
                <select
                  name="rol"
                  value={formData.rol}
                  onChange={handleChange}
                  className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
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
              <div className="border-t border-[#e1e7f0] pt-5 mt-3">
                <h3 className="text-lg font-semibold text-blue-500 mb-4">
                  Información de la Agencia
                </h3>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-2">
                      Nombre de la Agencia <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="nombreAgencia"
                      value={formData.nombreAgencia}
                      onChange={handleChange}
                      placeholder="AutoVentas RD"
                      className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                      required={esDealer}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-2">
                      RNC de la Agencia <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="agenciaRNC"
                      value={formData.agenciaRNC}
                      onChange={handleChange}
                      placeholder="1-30-12345-6"
                      className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                      required={esDealer}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-2">
                      Ubicación de la Agencia <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="ubicacionAgencia"
                      value={formData.ubicacionAgencia}
                      onChange={handleChange}
                      placeholder="Santo Domingo"
                      className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                      required={esDealer}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-2">
                      Teléfono de la Agencia <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="tel"
                      name="telefonoAgencia"
                      value={formData.telefonoAgencia}
                      onChange={handleChange}
                      placeholder="809-555-5555"
                      className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                      required={esDealer}
                    />
                  </div>
                </div>
              </div>
            )}

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-blue-500 text-white rounded-lg font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors mt-4"
            >
              {loading ? "Registrando..." : "Registrarse"}
            </button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
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