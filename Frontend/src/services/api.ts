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

// --- CSRF: token en memoria (funciona cross-origin) ---
// El patrón double-submit original leía la cookie con document.cookie, pero
// esto no funciona cuando el frontend y la API están en orígenes distintos
// (ej. Cloudflare Pages + api-staging.automarket-rd.com).
// Solución: GET /api/csrf retorna el token en el body Y en la cookie.
// El frontend almacena el token del body y lo envía en X-CSRF-Token.
let csrfToken: string | null = null;

api.interceptors.request.use((config) => {
  const method = (config.method ?? "get").toUpperCase();
  if (["POST", "PUT", "PATCH", "DELETE"].includes(method) && csrfToken) {
    config.headers["X-CSRF-Token"] = csrfToken;
  }
  return config;
});

export const API_BASE_URL = baseURL;

// Asegura que el token CSRF exista antes de cualquier mutación.
// GET /api/csrf retorna el token en el body; lo almacenamos en memoria
// para enviarlo en X-CSRF-Token en cada mutación.
let csrfBootstrapped = false;
export async function bootstrapCsrf(): Promise<void> {
  if (csrfBootstrapped) return;
  try {
    const { data } = await api.get<{ token: string }>("/api/csrf");
    csrfToken = data?.token ?? null;
  } catch {
    // Si falla, se reintentará en el próximo 403 CSRF.
  } finally {
    csrfBootstrapped = true;
  }
}

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

    // CSRF token inválido: re-obtener token y reintentar una vez
    if (status === 403) {
      const mensaje = error.response?.data?.mensaje;
      if (mensaje?.includes("CSRF") && !error.config?._csrfRetry) {
        error.config._csrfRetry = true;
        csrfBootstrapped = false;
        csrfToken = null;
        await bootstrapCsrf();
        return api.request(error.config);
      }
      error.message =
        "Tu sesión ha expirado. Recarga la página e inténtalo de nuevo.";
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
