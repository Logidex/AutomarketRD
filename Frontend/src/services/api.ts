import axios from 'axios';

// Resuelve la URL base de la API en tres pasos:
//  1) Si el frontend se sirve desde un túnel de VS Code (Dev Tunnels),
//     deriva la API del mismo túnel cambiando el puerto -5173 por -8080.
//  2) Si existe VITE_API_URL, lo usa (entornos compilados/despliegues).
//  3) Fallback a localhost para desarrollo puro en la misma máquina.
function resolverBaseURL(): string {
  const envUrl = import.meta.env.VITE_API_URL as string | undefined;

  if (typeof window !== "undefined") {
    const hostname = window.location.hostname;

    // Ej. "bhb991zw-5173.use2.devtunnels.ms" → api de "bhb991zw-8080.use2.devtunnels.ms"
    if (hostname.endsWith(".devtunnels.ms")) {
      const tunnelAPI = hostname.replace(
        /-\d+\.(.*devtunnels\.ms)$/,
        "-8080.$1"
      );
      return `https://${tunnelAPI}`;
    }
  }

  return envUrl ?? "http://localhost:8080";
}

const api = axios.create({
  baseURL: resolverBaseURL(),
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: false,
});

// Interceptor para agregar el token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para manejar errores y leer el mensaje del backend
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Sesión expirada o token inválido: limpiar sesión y volver al login
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');

      if (!window.location.pathname.startsWith('/login')) {
        window.location.href = '/login';
      }
    }

    // Límite de peticiones alcanzado (rate limiting): el backend responde 429 sin cuerpo
    if (error.response?.status === 429) {
      error.message =
        'Has hecho demasiadas solicitudes. Espera unos minutos e inténtalo de nuevo.';
    }

    // Si el backend respondió con un error (4xx, 5xx)
    if (error.response) {
      const data = error.response.data;

      // 1. Lee el mensaje del backend
      const backendMessage = data?.mensaje || data?.message;

      // 2. Si no hay mensaje, intenta leer los errores de validación
      //    (ValidationProblemDetails de ASP.NET Core, e.g. "errors": {...})
      const erroresValidacion = data?.errors;

      if (backendMessage) {
        error.message = backendMessage;
      } else if (
        erroresValidacion &&
        typeof erroresValidacion === "object"
      ) {
        const detalles = Object.values(erroresValidacion)
          .flat()
          .filter((v): v is string => typeof v === "string");

        if (detalles.length > 0) {
          error.message = detalles.join(" ");
        }
      }
    }

    return Promise.reject(error);
  }
);

export default api;