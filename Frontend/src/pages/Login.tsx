/* eslint-disable @typescript-eslint/no-unused-vars */
import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import Swal from "sweetalert2";
import { authService } from "../services/auth.service";
import logo from "../assets/AutoMarketRD_Logo.svg";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  const handleSubmit = async (e?: React.FormEvent) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }

    setLoading(true);

    try {
      const response = await authService.login({ email, password });

      if (response.token) {
        await Swal.fire({
          icon: "success",
          title: "¡Bienvenido!",
          text: response.mensaje,
          timer: 1500,
          showConfirmButton: false,
        });
        navigate("/dashboard");
      } else {
        await Swal.fire({
          icon: "error",
          title: "Error al iniciar sesión",
          text: "Correo electrónico o contraseña incorrectos.",
          confirmButtonColor: "#3b82f6",
        });
      }
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Error al iniciar sesión",
        text: "Correo electrónico o contraseña incorrectos.",
        confirmButtonColor: "#3b82f6",
      });
    } finally {
      setLoading(false);
    }
  };

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
            Bienvenido de nuevo
          </h1>
          <p className="text-[#9aa1b1] mb-10">
            Accede a tu cuenta para explorar los mejores vehículos del mercado.
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
            Iniciar Sesión
          </h2>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-600 mb-2">
                Email
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="tu@email.com"
                className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-2">
                Contraseña
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full px-4 py-3 bg-[#f7f9fc] border border-[#e1e7f0] rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
                required
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3.5 bg-blue-500 text-white rounded-lg font-semibold hover:bg-blue-600 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors"
            >
              {loading ? "Iniciando..." : "Iniciar Sesión"}
            </button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            ¿No tienes cuenta?{" "}
            <Link to="/registro" className="text-blue-500 font-semibold hover:underline">
              Regístrate aquí
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
