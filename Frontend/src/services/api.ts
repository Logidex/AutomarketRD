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

// --- CSRF: leer cookie y enviar header en métodos mutantes ---
function obtenerCookie(nombre: string): string | null {
  const Valor = document.cookie
    .split("; ")
    .find((fila) => fila.startsWith(`${nombre}=`))
    ?.split("=")[1];
  return Valor ?? null;
}

api.interceptors.request.use((config) => {
  const method = (config.method ?? "get").toUpperCase();
  if (["POST", "PUT", "PATCH", "DELETE"].includes(method)) {
    const token = obtenerCookie("automarket_csrf");
    if (token) {
      config.headers["X-CSRF-Token"] = token;
    }
  }
  return config;
});

export const API_BASE_URL = baseURL;

// Manejar respuestas y errores
// Evita redirigir varias veces cuando varias peticiones fallan en paralelo
// con la sesión expirada (p. ej. al cargar un dashboard con varias consultas).
let yaRedirigidoAPorSesion = false;

// Refresco single-flight: aunque 10 peticiones reciban 401 a la vez, solo
// se lanza UNA llamada a /api/auth/refrescar y todas esperan su resultado.
let refrescoEnCurso: Promise<boolean> | null = null;

function intentarRefresco(): Promise<boolean> {
  refrescoEnCurso ??= import("./auth.service")
    .then(({ authService }) => authService.refrescarSesion())
    .then(() => true)
    .catch(() => false)
    .finally(() => {
      refrescoEnCurso = null;
    });

  return refrescoEnCurso;
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error.response?.status;

    // Sesión expirada: se intenta renovar en silencio una sola vez; si el
    // refresh falla, se limpia el estado local y se redirige al login. Se
    // omiten las rutas de auth porque ahí la UI maneja el error directamente.
    if (status === 401) {
      const url = error.config?.url ?? "";
      const esRutaAuth =
        url.includes("/api/auth/login") ||
        url.includes("/api/auth/logout") ||
        url.includes("/api/auth/refrescar");

      if (!esRutaAuth && !error.config?._reintentadoTrasRefresco) {
        const refrescado = await intentarRefresco();

        if (refrescado && error.config) {
          error.config._reintentadoTrasRefresco = true;
          return api.request(error.config);
        }

        if (!yaRedirigidoAPorSesion) {
          yaRedirigidoAPorSesion = true;

          // Limpia el usuario guardado y borra las cookies en el servidor.
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
    }

    // Rate limiting
    if (status === 429) {
      error.message =
        "Has hecho demasiadas solicitudes. Espera unos minutos e inténtalo de nuevo.";
    }

    // CSRF token inválido
    if (status === 403) {
      const mensaje = error.response?.data?.mensaje;
      if (mensaje?.includes("CSRF")) {
        error.message =
          "Tu sesión ha expirado. Recarga la página e inténtalo de nuevo.";
      }
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
