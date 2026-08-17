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
// Evita redirigir varias veces cuando varias peticiones fallan en paralelo
// con la sesión expirada (p. ej. al cargar un dashboard con varias consultas).
let yaRedirigidoAPorSesion = false;

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;

    // Sesión expirada o cookie inválida: se limpia el estado local y se
    // redirige al login. Se omiten login/logout porque ahí la UI maneja el
    // error (credenciales incorrectas) sin necesidad de limpiar la sesión.
    if (status === 401) {
      const url = error.config?.url ?? "";
      const esCredencialesIncorrectas =
        url.includes("/api/auth/login") || url.includes("/api/auth/logout");

      if (!esCredencialesIncorrectas && !yaRedirigidoAPorSesion) {
        yaRedirigidoAPorSesion = true;

        // Limpia el usuario guardado y borra la cookie en el servidor.
        void import("./auth.service").then(({ authService }) => {
          authService.logout();
        });

        // Recarga completa para descartar estado en memoria con la sesión vieja.
        if (window.location.pathname !== "/login") {
          window.location.assign("/login");
        } else {
          yaRedirigidoAPorSesion = false;
        }
      }
    }

    // Rate limiting
    if (status === 429) {
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
