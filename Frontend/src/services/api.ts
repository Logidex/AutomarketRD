import axios from "axios";

const envUrl = (import.meta.env.VITE_API_URL as string | undefined)
  ?.trim();

// Si VITE_API_URL es "/api", se convierte en "".
// Así, los servicios pueden seguir usando rutas como:
// api.get("/api/anuncios")
const baseURL = envUrl?.replace(/\/api\/?$/i, "") ?? "";

const api = axios.create({
  ...(baseURL ? { baseURL } : {}),
  withCredentials: false,
});

export const API_BASE_URL = baseURL;

// Agregar el token JWT a cada solicitud
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// Manejar respuestas y errores
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Sesión expirada o token inválido
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");

      if (!window.location.pathname.startsWith("/login")) {
        window.location.href = "/login";
      }
    }

    // Rate limiting
    if (error.response?.status === 429) {
      error.message =
        "Has hecho demasiadas solicitudes. Espera unos minutos e inténtalo de nuevo.";
    }

    // Procesar errores enviados por el backend
    if (error.response) {
      const data = error.response.data;

      // Mensaje principal del backend
      const backendMessage = data?.mensaje || data?.message;

      // Errores de validación de ASP.NET Core
      const erroresValidacion = data?.errors;

      if (backendMessage) {
        error.message = backendMessage;
      } else if (
        erroresValidacion &&
        typeof erroresValidacion === "object"
      ) {
        const detalles = Object.values(erroresValidacion)
          .flat()
          .filter((value): value is string => typeof value === "string");

        if (detalles.length > 0) {
          error.message = detalles.join(" ");
        }
      }
    }

    return Promise.reject(error);
  },
);

export default api;