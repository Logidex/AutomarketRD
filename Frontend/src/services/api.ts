import axios from "axios";

const envUrl = (import.meta.env.VITE_API_URL as string | undefined)?.trim();

// Si VITE_API_URL es "/api", se convierte en "".
// Así, los servicios pueden seguir usando rutas como:
// api.get("/api/anuncios")
const baseURL = envUrl?.replace(/\/api\/?$/i, "") ?? "";

const api = axios.create({
  ...(baseURL ? { baseURL } : {}),
  // El JWT viaja como cookie HttpOnly; las solicitudes deben enviar credenciales.
  withCredentials: true,
});

export const API_BASE_URL = baseURL;

// Manejar respuestas y errores
api.interceptors.response.use(
  (response) => {
    console.log("[API] Response:", response.config.url, response.status);
    return response;
  },
  (error) => {
    console.error(
      "[API] Error:",
      error.config?.url,
      error.response?.status,
      error.response?.data,
    );
    // Sesión expirada o cookie inválida
    if (error.response?.status === 401) {
      console.error("[AUTH 401]", {
        url: error.config?.url,
        method: error.config?.method,
        status: error.response?.status,
        data: error.response?.data,
      });

      // No redirigimos todavía.
      // Primero identificaremos qué petición está fallando.
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
      } else if (erroresValidacion && typeof erroresValidacion === "object") {
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
