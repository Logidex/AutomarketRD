import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5217',
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